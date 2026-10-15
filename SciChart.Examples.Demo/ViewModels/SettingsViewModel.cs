// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// SettingsViewModel.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using SciChart.Charting;
using SciChart.Charting.Common.AttachedProperties;
using SciChart.Charting.Common.Extensions;
using SciChart.Charting.Common.MarkupExtensions;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.TradeChart;
using SciChart.Charting3D;
using SciChart.Core.Utility;
using SciChart.Data.Numerics.PointResamplers;
using SciChart.Drawing.VisualXcceleratorRasterizer;
using SciChart.Examples.Demo.Common;
using SciChart.Examples.Demo.Common.Converters;
using SciChart.Examples.Demo.Helpers;
using SciChart.Examples.Demo.Helpers.Navigation;
using SciChart.Examples.Examples.HeatmapChartTypes.PolarHeatmapCustomization;
using SciChart.UI.Bootstrap;
using SciChart.UI.Reactive;
using SciChart.UI.Reactive.Observability;

#if !XPF
using System.Threading.Tasks;
using SciChart.Drawing.HighSpeedRasterizer;
#else
using SciChart.Drawing.XamlRasterizer;
#endif

namespace SciChart.Examples.Demo.ViewModels
{
    public interface ISettingsViewModel
    {
        bool InitReady { get; set; }
        IMainWindowViewModel ParentViewModel { get; set; }
        void OnIsVisibleChanged(bool isVisible);
    }

    [ExportType(typeof(ISettingsViewModel), CreateAs.Singleton)]
    public class SettingsViewModel : ViewModelWithTraitsBase, ISettingsViewModel
    {
        private Type _selectedRenderer;
        private VxRenderSettings _renderSettings = new VxRenderSettings();

        public SettingsViewModel()
        {
            WithTrait<AllowFeedbackSettingBehaviour>();

            // Engine restarts and RecreateStyles must run on the UI thread; Throttle leaves the
            // handler on a pool thread. Bound to Application.Current.Dispatcher, not
            // DispatcherSchedulerEx.Current: this ctor is resolved on a thread-pool thread.
            var uiScheduler = Application.Current != null
                ? (IScheduler)new DispatcherSchedulerEx(Application.Current.Dispatcher)
                : Scheduler.CurrentThread;
            
            VersionInfo = $"{SciChartRuntimeInfo.GetVersion()} ({SciChartRuntimeInfo.GetTargetFramework()})";
            
            if (VisualXcceleratorEngine.SupportsHardwareAcceleration && !VisualXcceleratorEngine.IsGpuBlacklisted)
            {
                IsHardwareAccelerated = true;

                // Always force wait for draw in UIAutomationTestMode
                EnableForceWaitForGPU = App.UIAutomationTestMode;

                if (SciChartRuntimeInfo.IsWindowsPlatform)
                {
                    IsDirectXActive = true;

                    Use3DAA4x= VisualXcceleratorEngine.GraphicsApiInfo.AntiAliasingMode == FullScreenAntiAliasingMode.MSAA4x;
                    Use3DAANone = VisualXcceleratorEngine.GraphicsApiInfo.AntiAliasingMode == FullScreenAntiAliasingMode.None;
                }
            }

            if (SciChartRuntimeInfo.IsWindowsPlatform)
            {
                IsSimdVisible = true;
                EnableSimd = true;
            }

            Is3DZAxisUp = false;
            Is3DZAxisUpOverridden = false;
            // Seed from the actual current present mode (the dev tools may have set it from the command
            // line) instead of hardcoding readback, so the Settings toggle doesn't stomp the requested mode.
            UseAlternativeFillSource = VisualXcceleratorEngine.UseAlternativeFillSource;

            EnableResamplingCPlusPlus = true;
            EnableImpossibleMode = false;

#if !XPF
            IsSoftwareRendererVisible = true;
            SelectedRenderer = IsHardwareAccelerated
                ? typeof(VisualXcceleratorRenderSurface)
                : typeof(HighSpeedRenderSurface);
#else
            IsSoftwareRendererVisible = false;
            SelectedRenderer = IsHardwareAccelerated
                ? typeof(VisualXcceleratorRenderSurface)
                : typeof(XamlRenderSurface);
#endif
            Observable.CombineLatest(
                    this.WhenPropertyChanged(x => x.UseAlternativeFillSource),
                    this.WhenPropertyChanged(x => x.EnableForceWaitForGPU),
                    this.WhenPropertyChanged(x => x.EnableResamplingCPlusPlus),
                    this.WhenPropertyChanged(x => x.EnableImpossibleMode),
                    this.WhenPropertyChanged(x => x.SelectedRenderer),
                    Tuple.Create)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(uiScheduler)
                .Subscribe(tuple =>
                {
                    // Both are global and apply to 2D and 3D charts (UseAlternativeFillSource: true = readback,
                    // false = zero-copy); existing charts switch when they change.
                    VisualXcceleratorEngine.UseAlternativeFillSource = tuple.Item1;
                    VisualXcceleratorEngine.EnableForceWaitForGPU = tuple.Item2;

                    RecreateStyles();
                })
                .DisposeWith(this);

            if (IsDirectXActive)
            {
                Observable.CombineLatest(
                        this.WhenPropertyChanged(x => x.Use3DAANone),
                        this.WhenPropertyChanged(x => x.Use3DAA4x),
                        Tuple.Create)
                    .Skip(1)
                    .Throttle(TimeSpan.FromMilliseconds(100))
                    .ObserveOn(uiScheduler)
                    .Subscribe(async tuple =>
                    {
                        try
                        {
                            IsD3DBusy = true;

                            var renderSettings = new VxRenderSettings
                            {
                                // Keep the DirectX mode the engine runs with (the dev tools can change it)
                                DirectXMode = (VisualXcceleratorEngine.GraphicsApiInfo as DirectXGraphicsApiInfo)?.DirectXMode
                                    ?? DirectXMode.DirectX11,
                                
                                FullScreenAntiAliasingMode= Use3DAA4x
                                    ? FullScreenAntiAliasingMode.MSAA4x
                                    : FullScreenAntiAliasingMode.None
                            };

                            if (!renderSettings.Equals(_renderSettings))
                            {
                                _renderSettings = renderSettings;
#if !XPF
                                var task = VisualXcceleratorEngine.RestartEngineAsync(renderSettings);
                                await Task.WhenAll(task, Task.Delay(1000));
                                // Keep the 3D engine on the same backend + AA as 2D (Viewport3D has its
                                // own FullScreenAntiAliasingMode enum). No-ops if already matching.
                                Viewport3D.Restart3DEngineWith(renderSettings.DirectXMode, Use3DAA4x
                                        ? FullScreenAntiAliasingMode.MSAA4x
                                        : FullScreenAntiAliasingMode.None);
#else
                                VisualXcceleratorEngine.RestartEngine(renderSettings);
#endif
                            }
                        }
                        finally
                        {
                            IsD3DBusy = false;
                        }
                    });
            }

            this.WhenPropertyChanged(x => x.Is3DZAxisUp)
                .Subscribe(is3DZAxisUp =>
                {
                    Viewport3D.SetViewportOrientation(is3DZAxisUp
                        ? Viewport3DOrientation.ZAxisUp
                        : Viewport3DOrientation.YAxisUp);

                })
                .DisposeWith(this);

#if SCICHART_DEVTOOLS
            // Dev tools (dev builds only, never deployed): graphics backend and adapter selection. Created on
            // the UI thread, as this constructor runs on a thread-pool thread
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                DevToolsSettings = DevTools.Graphics.GraphicsBackendSettingsView.Create(OnDevToolsEnginesRestarted)));
#endif

            if (!SciChartRuntimeInfo.IsXPF)
            {
                IsDropShadowsVisible = true;
                EnableDropShadows = true;

                this.WhenPropertyChanged(x => x.EnableDropShadows)
                    .Subscribe(b => EffectManager.EnableDropShadows = b)
                    .DisposeWith(this);
            }
        }

        public string VersionInfo { get; }

        public bool InitReady
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public IMainWindowViewModel ParentViewModel
        {
            get => GetDynamicValue<IMainWindowViewModel>();
            set => SetDynamicValue(value);
        }

        public bool IsD3DBusy
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

public bool Use3DAANone
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool Use3DAA4x
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool Is3DZAxisUp
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool Is3DZAxisUpOverridden
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool AllowFeedback
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool IsDropShadowsVisible
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

/// <summary>
        /// The dev tools' graphics backend settings view, shown in Developer Mode. Null unless the Demo is
        /// built against the SciChart sources with the dev tools (SCICHART_DEVTOOLS).
        /// </summary>
        public object DevToolsSettings
        {
            get => GetDynamicValue<object>();
            set => SetDynamicValue(value);
        }

        public bool EnableDropShadows
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool EnableResamplingCPlusPlus
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool IsSimdVisible
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool EnableSimd
        {
            get => GetDynamicValue<bool>();
            set
            {
                SetDynamicValue(value);

                ExtremeResamplersFactory.Instance.AccelerationMode = value
                    ? ExtremeResamplerAccelerationMode.Auto
                    : ExtremeResamplerAccelerationMode.None;
            }
        }

        public bool EnableImpossibleMode
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

#if SCICHART_DEVTOOLS
        // A dev tools backend/adapter change has restarted the engines: rebuild the chart styles so the
        // surfaces re-create their render targets, and refresh what the Demo shows about the engine.
        private void OnDevToolsEnginesRestarted()
        {
            RecreateStyles();

            // Refresh the "Hardware Acceleration" status label to the new backend.
            // Dispatched async (off this restart stack) — raising it inline here deadlocks.
            Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                () => (ParentViewModel as MainWindowViewModel)?.RefreshHardwareAccelerationInfo());
        }

#endif
        private void RecreateStyles()
        {
            // Default SciChart surfaces
            CreateGlobalStyle<SciChartSurface>();
            CreateGlobalStyle<SciChartRadarSurface>();
            CreateGlobalStyle<SciChartTernarySurface>();
            CreateGlobalStyle<SciStockChart>();

            // Custom Example surfaces
            CreateGlobalStyle<DailyMaxTemperatureHeatmapSurface>();
        }

        public Type SelectedRenderer
        {
            get => _selectedRenderer;
            set
            {
                if (_selectedRenderer == value || value == null) return;

                IsVxRenderer = value == typeof(VisualXcceleratorRenderSurface);

                if (IsVxRenderer)
                {
                    try
                    {
                        VisualXcceleratorEngine.AssertSupportsHardwareAcceleration();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }

                _selectedRenderer = value;
                OnPropertyChanged(value);
            }
        }

        public bool UseAlternativeFillSource
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool EnableForceWaitForGPU
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool IsHardwareAccelerated
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool IsDirectXActive
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool IsVxRenderer
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool IsSoftwareRendererVisible
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        /// <summary>
        /// This callback is called every time when the SettingsView is opened. It can be used to disable some settings for certain examples.
        /// At the moment, the Z-Axis Up feature is disabled in some examples.
        /// </summary>
        public void OnIsVisibleChanged(bool isVisible)
        {
            if (!isVisible) return;

            Is3DZAxisUp = Viewport3D.ViewportOrientation == Viewport3DOrientation.ZAxisUp;

            if (Navigator.Instance.CurrentPage is ExamplesHostAppPage)
            {
                Is3DZAxisUpOverridden = Navigator.Instance.CurrentExample.Uri.Contains("ZAxisUp3D") ||
                                        Navigator.Instance.CurrentExample.Uri.Contains("Antenna3DRadiationPattern");
            }
            else
            {
                Is3DZAxisUpOverridden = false;
            }
        }

        private void CreateGlobalStyle<T>() where T : SciChartSurfaceBase
        {
            var overrideStyle = new Style(typeof(T));

            var binding = new Binding
            {
                Source = this,
                Converter = new RendererSettingConverter(),
                Mode = BindingMode.OneWay
            };

            overrideStyle.Setters.Add(new Setter(SciChartSurfaceBase.RenderSurfaceProperty, binding));
            overrideStyle.Setters.Add(new Setter(VisualXcceleratorEngine.EnableImpossibleModeProperty, EnableImpossibleMode));

            overrideStyle.Setters.Add(new Setter(PerformanceHelper.EnableExtremeResamplersProperty, EnableResamplingCPlusPlus));

            var currentTheme = "SciChartv7Navy";

            if (Application.Current.Resources[typeof(T)] is Style sourceStyle)
            {
                var sourceTheme = sourceStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property.Name == "Theme");
                currentTheme = sourceTheme?.Value.ToString() ?? currentTheme;
            }

            overrideStyle.Setters.Add(new Setter(ThemeManager.ThemeProperty, currentTheme));

            if (Application.Current.Resources.Contains(typeof(T)))
                Application.Current.Resources.Remove(typeof(T));

            Application.Current.Resources.Add(typeof(T), overrideStyle);
        }
    }
}