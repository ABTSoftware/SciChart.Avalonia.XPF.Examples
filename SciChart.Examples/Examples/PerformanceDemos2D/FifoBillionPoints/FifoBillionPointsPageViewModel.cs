﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using SciChart.Charting.Common.Helpers;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Core.Extensions;
using SciChart.Examples.ExternalDependencies.Common;

namespace SciChart.Examples.Examples.PerformanceDemos2D.FifoBillionPoints
{
    public class FifoBillionPointsPageViewModel : BaseViewModel
    {
        private bool _isRunning;
        private bool _isStopped;
        private string _loadingMessage;

        private RenderSyncedTimer _timer;
        private PointCount _selectedPointCount;

        private const int AppendCount = 10_000; // The number of points to append per timer tick
        private const int TimerIntervalMs = 10; // Interval of timer tick 

        private readonly float[] _xBuffer = new float[AppendCount];
        private readonly float[] _yBuffer = new float[AppendCount];

        public FifoBillionPointsPageViewModel()
        {
            _isRunning = false;
            _isStopped = true;

            RunCommand = new ActionCommand(OnRun);
            PauseCommand = new ActionCommand(OnPause);
            StopCommand = new ActionCommand(OnStop);
            ViewportManager = new SurfaceViewportManager(OnRendererChanged);

            // Add the point count options 
            AllPointCounts.Add(new PointCount("1 Million", 5, 200_000));
            AllPointCounts.Add(new PointCount("5 Million", 5, 1_000_000));
            AllPointCounts.Add(new PointCount("10 Million", 5, 2_000_000));
            AllPointCounts.Add(new PointCount("50 Million", 5, 10_000_000));

            // If you have 8GB of RAM or more you can render 100M (will require just 1GB but to be safe...)
            AllPointCounts.Add(new PointCount("100 Million", 5, 20_000_000));

            // 1 Billion points requires 8GB of free RAM or it will hit swap drive 
            if (Environment.Is64BitProcess)
            {
                // Note: these point counts require the experimental VisualXccelerator.EnableImpossibleMode flag set to true on the chart 
                AllPointCounts.Add(new PointCount("500 Million", 5, 100_000_000));
                AllPointCounts.Add(new PointCount("1 Billion", 5, 200_000_000));
            }

            // Setup some warnings
            PerformanceWarnings = GetPerformanceWarnings();

            // Get ready to rock & roll 
            SelectedPointCount = AllPointCounts.Last();
        }

        public string PerformanceWarnings { get; private set; }

        public bool HasWarnings => !string.IsNullOrEmpty(PerformanceWarnings);

        public ObservableCollection<IRenderableSeriesViewModel> Series { get; } = new ObservableCollection<IRenderableSeriesViewModel>();

        public ObservableCollection<PointCount> AllPointCounts { get; } = new ObservableCollection<PointCount>();

        public SurfaceViewportManager ViewportManager { get; }

        public PointCount SelectedPointCount
        {
            get => _selectedPointCount;
            set
            {
                _selectedPointCount = value;
                OnPropertyChanged(nameof(SelectedPointCount));
            }
        }

        public ActionCommand RunCommand { get; }
        public ActionCommand PauseCommand { get; }
        public ActionCommand StopCommand { get; }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public bool IsStopped
        {
            get => _isStopped;
            set
            {
                _isStopped = value;
                OnPropertyChanged(nameof(IsStopped));
            }
        }

        public bool IsLoading => !string.IsNullOrEmpty(LoadingMessage);

        public string LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                _loadingMessage = value;

                OnPropertyChanged(nameof(LoadingMessage));
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private void OnRendererChanged()
        {
            // If renderer changes, need to re-attach the timer
            if (_timer != null)
            {
                _timer.Stop();
                _timer.SafeDispose();

                // Create and restart the timer
                _timer = new RenderSyncedTimer(TimeSpan.FromMilliseconds(TimerIntervalMs),
                    ViewportManager.RenderSurface,
                    OnTimerTick);

                if (IsRunning) _timer.Start();
            }
        }

        private async void OnRun()
        {
            if (!IsRunning)
            {
                _timer ??= new RenderSyncedTimer(TimeSpan.FromMilliseconds(TimerIntervalMs),
                    ViewportManager.RenderSurface,
                    OnTimerTick);

                IsRunning = true;

                if (IsStopped)
                {
                    var seriesCount = SelectedPointCount.SeriesCount;
                    var pointCount = SelectedPointCount.PointsCount;

                    LoadingMessage = $"Generating {SelectedPointCount.DisplayName} Points...";

                    var series = await CreateSeriesAsync(seriesCount, pointCount);
                    using (ViewportManager.ParentSurface.SuspendUpdates())
                    {
                        series.ForEachDo(x => Series.Add(x));
                    }

                    LoadingMessage = null;
                }

                IsStopped = false;

                _timer.Start();
            }
        }

        private async Task<List<IRenderableSeriesViewModel>> CreateSeriesAsync(int seriesCount, int pointCount)
        {
            var seriesColors = new Color[]
            {
                ColorFromUInt(0xFF50C7E0),
                ColorFromUInt(0xFFF48420),
                ColorFromUInt(0xFF882B91),
                ColorFromUInt(0xFF30BC9A),
                ColorFromUInt(0xFFEC0F6C),
                ColorFromUInt(0xFF364BA0),
            };

            List<IRenderableSeriesViewModel> result = null;
            var generateSeriesDataTask = new Task<List<IRenderableSeriesViewModel>>(() =>
            {
                // Create N series of M points async. Return to calling code to set on the chart 
                IRenderableSeriesViewModel[] series = new IRenderableSeriesViewModel[seriesCount];

                CancellationTokenSource cts = new();
                ParallelOptions options = new()
                {
                    CancellationToken = cts.Token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                // We generate data in parallel as just generating 1,000,000,000 points takes a long time no matter how fast your chart is! 
                Parallel.For(0, seriesCount, options, i =>
                {
                    // Temporary buffer for fast filling of DataSeries
                    var xBuffer = new float[AppendCount];
                    var yBuffer = new float[AppendCount];

                    var randomSeed = i * short.MaxValue;
                    var randomWalkGenerator = new Rand(randomSeed);

                    XyDataSeries<float, float> xyDataSeries = null;
                    // Creation of XyDataSeries with Capacity allocates the memory immediately
                    // Catch any OutOfMemory exceptions
                    try
                    {
                        xyDataSeries = new XyDataSeries<float, float>
                        {
                            // Required for scrolling / streaming 'first in first out' charts
                            FifoCapacity = pointCount,

                            Capacity = pointCount,

                            // Optional to improve performance when you know in advance whether 
                            // data is sorted ascending and contains float.NaN or not 
                            DataDistributionCalculator = new UserDefinedDistributionCalculator<float, float>
                            {
                                ContainsNaN = false,
                                IsEvenlySpaced = true,
                                IsSortedAscending = true,
                            },

                            // Just associate a random walk generator with the series for more consistent random generation
                            Tag = randomWalkGenerator
                        };
                    }
                    catch
                    {
                        cts.Cancel();
                    }

                    int yOffset = i * 2;
                    for (int j = 0; j < pointCount; j += AppendCount)
                    {
                        for (int k = 0; k < AppendCount; k++)
                        {
                            xBuffer[k] = j + k;
                            yBuffer[k] = randomWalkGenerator.NextWalk() + yOffset;
                        }

                        xyDataSeries.Append(xBuffer, yBuffer);
                    }

                    // Store the series 
                    series[i] = new LineRenderableSeriesViewModel
                    {
                        DataSeries = xyDataSeries,
                        Stroke = i >= seriesColors.Length ? GetRandomColor() : seriesColors[i]
                    };
                });

                return series.ToList();
            });

            // Run the task
            try
            {
                generateSeriesDataTask.Start();
                result = await generateSeriesDataTask;
            }
            catch
            {
                // Any exceptions during data generation mean that we cannot proceed
                result.Clear();

                PerformanceWarnings += "Low system RAM, try on 16GB machine.";
                        
                OnPropertyChanged(nameof(PerformanceWarnings));
                OnPropertyChanged(nameof(HasWarnings));
            }

            return result;
        }

        private static Color GetRandomColor()
        {
            return Color.FromRgb(Rand.NextByte(55), Rand.NextByte(55), Rand.NextByte(55));
        }

        private static Color ColorFromUInt(uint color)
        {
            return Color.FromArgb((byte)(color >> 24), (byte)(color >> 16), (byte)(color >> 8), (byte)color);
        }

        private static string GetPerformanceWarnings()
        {
            var warnings = new StringBuilder();
#if DEBUG
            // Debug mode is the cause of all performance woes. Try release mode?
            warnings.Append("Debug mode is slow, try Release. ");           
#endif
            if (Debugger.IsAttached)
            {
                // Its considerably slower to run the code when debugger is attached. Warn the user
                warnings.Append("Debugger is attached, try without. ");
            }

            return warnings.ToString();
        }

        private void OnPause()
        {
            IsRunning = false;
            IsStopped = false;

            _timer.Stop();
            ViewportManager.ZoomExtentsX();
        }

        private async void OnStop()
        {
            if (!IsStopped)
            {
                _timer.Stop();

                IsRunning = false;
                IsStopped = true;

                LoadingMessage = "Releasing memory...";

                var series = Series;
                Series.Clear();

                // Release memory
                await Task.Run(() =>
                {
                    series.ForEachDo(x =>
                    {
                        x.DataSeries.Clear(true);
                        x.DataSeries = null;
                    });
                });
            }

            // For example purposes, we're including GC.Collect. We don't recommend you do this in a production app
            await Task.Run(() => GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced));

            LoadingMessage = null;
        }

        /// <summary>
        /// Free memory when the example is unloaded 
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnStop();
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            _timer = null;
        }

        private void OnTimerTick()
        {
            using (ViewportManager.ParentSurface.SuspendUpdates())
            {
                int seriesIndex = 0;
                foreach (var series in Series)
                {
                    var dataSeries = (XyDataSeries<float, float>)series.DataSeries;
                    var randomWalkGenerator = (Rand)dataSeries.Tag;
                    var startIndex = (int)dataSeries.XValues.Last() + 1;

                    int yOffset = seriesIndex * 2;
                    for (int i = 0, j = startIndex; i < AppendCount; i++, j++)
                    {
                        _xBuffer[i] = j;
                        _yBuffer[i] = randomWalkGenerator.NextWalk() + yOffset;
                    }

                    dataSeries.Append(_xBuffer, _yBuffer);
                    seriesIndex++;
                }
            }
        }
    }
}