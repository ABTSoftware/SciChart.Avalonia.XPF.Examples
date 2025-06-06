﻿// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2025. All rights reserved.
//  
// Web: http://www.scichart.com
//   Support: support@scichart.com
//   Sales:   sales@scichart.com
// 
// UpdateScatterPointsViewModel.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use. 
// 
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied. 
// *************************************************************************************
using System;
using System.Diagnostics;
using System.Timers;
using SciChart.Charting;
using SciChart.Charting.Common.Helpers;
using SciChart.Charting.Model.DataSeries;
using SciChart.Core;
using SciChart.Drawing.Common;
using SciChart.Drawing.VisualXcceleratorRasterizer;
using SciChart.Examples.ExternalDependencies.Common;

namespace SciChart.Examples.Examples.PerformanceDemos2D.UpdateScatter
{
    public class UpdateScatterPointsViewModel : BaseViewModel
    {
        private XyDataSeries<double, double> _victims;
        private XyDataSeries<double, double> _defenders;

        private IRenderSurface _renderSurface;
        private bool _warningIsVisible;
        private bool _isRunning;

        private World _simulation;
        private Timer _timer;

        private readonly int _pointCount;
        private volatile bool _working = false;

        private readonly ObjectPool<XyDataSeries<double>> _seriesPool;

        public UpdateScatterPointsViewModel()
        {
            // SL does not support hardware acceleration, so the point count is lowered deliberately in this edition
            _pointCount = VisualXcceleratorEngine.SupportsHardwareAcceleration ? 100000 : 30000;
            _seriesPool = new ObjectPool<XyDataSeries<double>>(10, series =>
            {
                series.AcceptsUnsortedData = true;
                series.Capacity = _pointCount;
                return series;
            });

            RunExampleCommand = new ActionCommand(OnRunExample);
            StopExampleCommand = new ActionCommand(OnStopExample);
        }

        public ActionCommand RunExampleCommand { get; }

        public ActionCommand StopExampleCommand { get; }

        public XyDataSeries<double, double> Victims
        {
            get => _victims;
            set
            {
                if (_victims == value) return;
                _victims = value;
                OnPropertyChanged(nameof(Victims));
            }
        }

        public XyDataSeries<double, double> Defenders
        {
            get => _defenders;
            set
            {
                if (_defenders == value) return;
                _defenders = value;
                OnPropertyChanged(nameof(Defenders));
            }
        }

        public bool WarningIsVisible
        {
            get => _warningIsVisible;
            set
            {
                if (_warningIsVisible == value) return;
                _warningIsVisible = value;
                OnPropertyChanged(nameof(WarningIsVisible));
            }
        }

        public IRenderSurface RenderSurface
        {
            get => _renderSurface;
            set
            {
                if (_renderSurface == value) return;

                _renderSurface = value;

                if (_renderSurface is VisualXcceleratorRenderSurface)
                {
                    WarningIsVisible = false;
                    OnRunExample();
                }
                else
                {
                    WarningIsVisible = true;
                    OnStopExample();
                }
            }
        }

        public void OnStopExample()
        {
            if (_timer != null)
            {
                lock (_timer)
                {
                    _timer.Stop();
                    _timer = null;
                }
            }
            
            Victims?.Clear();
            Defenders?.Clear();

            _isRunning = false;
        }

        private void OnRunExample()
        {
            if (!_isRunning &&
                _renderSurface is VisualXcceleratorRenderSurface)
            {
                _simulation = new World(_pointCount);
                _simulation.Populate();

                Victims = _seriesPool.Get();
                Defenders = _seriesPool.Get();

                // We can append on a background thread.
                // The simulation is computationally expensive, so we move this to another thread. 
                _timer = new Timer(1000.0 / 60.0);
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();

                _isRunning = true;
            }
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            if (_working) return;

            lock (_timer)
            {
                _working = true;

                // The simulation is very computationally expensive, so we update in parallel
                // What we observe is 400ms is required to update position of 1,000,000 elements
                var sw = Stopwatch.StartNew();
                _simulation.Tick(4);
                sw.Stop();
                //Console.WriteLine("Tick: {0}ms", sw.ElapsedMilliseconds);

                // By creating and filling a new series per-frame, we avoid the need
                // to call SuspendUpdates on the series, which will lock the parent chart.
                // 
                // Therefore we can append on another thread (other than UI Thread) and the UI will
                // draw the last series on the UI thread, while we are filling this series on the background thread
                // 
                // The trade-off is memory usage. Each time you create and discard an XyDataSeries you are putting additional
                // pressure on the garbage collector.                 

                var victims = _seriesPool.Get();
                var defenders = _seriesPool.Get();

                using (victims.SuspendUpdates())
                using (defenders.SuspendUpdates())
                {
                    victims.Clear();
                    defenders.Clear();

                    victims.AcceptsUnsortedData = true;
                    defenders.AcceptsUnsortedData = true;

                    var persons = _simulation.People;
                    for (int i = 0; i < persons.Length; ++i)
                    {
                        Person person = persons[i];
                        if (person.IsVictim)
                        {
                            victims.Append(person.Pos.X, person.Pos.Y);
                        }
                        else
                        {
                            defenders.Append(person.Pos.X, person.Pos.Y);
                        }
                    }

                    _seriesPool.Put((XyDataSeries<double>)Victims);
                    _seriesPool.Put((XyDataSeries<double>)Defenders);

                    Victims = victims;
                    Defenders = defenders;
                }

                _working = false;
            }
        }
    }
}