using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;
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
using SciChart.Examples.Demo.Common.Converters;
using SciChart.Examples.Demo.Helpers;
using SciChart.Examples.Demo.Helpers.Navigation;
using SciChart.Examples.Examples.HeatmapChartTypes.PolarHeatmapCustomization;
using SciChart.UI.Bootstrap;
using SciChart.UI.Reactive;
using SciChart.UI.Reactive.Observability;
using FullScreenAntiAliasingMode = SciChart.Drawing.VisualXcceleratorRasterizer.FullScreenAntiAliasingMode;

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

            VersionInfo = $"{SciChartRuntimeInfo.GetVersion()} ({SciChartRuntimeInfo.GetTargetFramework()})";

            if (VisualXcceleratorEngine.SupportsHardwareAcceleration && !VisualXcceleratorEngine.IsGpuBlacklisted)
            {
                IsHardwareAccelerated = true;

                if (VisualXcceleratorEngine.ActiveGraphicsAPI == VxGraphicsAPI.DirectX)
                {
                    IsDirectXActive = true;

                    if (!SciChartRuntimeInfo.IsXPF)
                    {
                        IsD3DVisible = true;

                        UseD3D9 = VisualXcceleratorEngine.DirectXMode == DirectXMode.DirectX9c;
                        UseD3D11 = VisualXcceleratorEngine.DirectXMode == DirectXMode.DirectX11;

                        // Always force wait for draw in UIAutomationTestMode 
                        EnableForceWaitForGPU = App.UIAutomationTestMode;
                    }

                    Use3DAA4x = VisualXcceleratorEngine.AntiAliasingMode == FullScreenAntiAliasingMode.MSAA4x;
                    Use3DAANone = VisualXcceleratorEngine.AntiAliasingMode == FullScreenAntiAliasingMode.None;
                }
            }

            if (SciChartRuntimeInfo.IsWindowsPlatform)
            {
                IsSimdVisible = true;
                EnableSimd = true;
            }

            Is3DZAxisUp = false;
            Is3DZAxisUpOverridden = false;
            UseAlternativeFillSource = true;

            EnableResamplingCPlusPlus = true;
            EnableExtremeDrawingManager = false;
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
                    this.WhenPropertyChanged(x => x.EnableExtremeDrawingManager),
                    this.WhenPropertyChanged(x => x.SelectedRenderer),
                    Tuple.Create)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Subscribe(tuple =>
                {
#if !XPF
                    Viewport3D.UseAlternativeFillSource = tuple.Item1;
                    Viewport3D.ForceStallUntilGPUIsIdle = tuple.Item2;
                    VisualXcceleratorEngine.EnableForceWaitForGPU = tuple.Item2;
#endif
                    RecreateStyles();
                })
                .DisposeWith(this);

            if (IsDirectXActive)
            {
                Observable.CombineLatest(
                        this.WhenPropertyChanged(x => x.UseD3D9),
                        this.WhenPropertyChanged(x => x.UseD3D11),
                        this.WhenPropertyChanged(x => x.Use3DAANone),
                        this.WhenPropertyChanged(x => x.Use3DAA4x),
                        Tuple.Create)
                    .Skip(1)
                    .Throttle(TimeSpan.FromMilliseconds(100))
                    .ObserveOn(Scheduler.CurrentThread)
                    .Subscribe(async tuple =>
                    {
                        try
                        {
                            IsD3DBusy = true;

                            var renderSettings = new VxRenderSettings
                            {
#if !XPF
                                DirectXMode = UseD3D9
                                    ? DirectXMode.DirectX9c
                                    : DirectXMode.DirectX11,
#endif
                                FullScreenAntiAliasingMode = Use3DAA4x
                                    ? FullScreenAntiAliasingMode.MSAA4x
                                    : FullScreenAntiAliasingMode.None
                            };

                            if (!renderSettings.Equals(_renderSettings))
                            {
                                _renderSettings = renderSettings;
#if !XPF
                                var task = VisualXcceleratorEngine.RestartEngineAsync(renderSettings);
                                await Task.WhenAll(task, Task.Delay(1000));
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

        public bool IsD3DVisible
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool UseD3D11
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool UseD3D9
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

        public bool EnableExtremeDrawingManager
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

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

        public void OnIsVisibleChanged(bool isVisible)
        {
            if (!isVisible) return;

            Is3DZAxisUp = Viewport3D.ViewportOrientation == Viewport3DOrientation.ZAxisUp;

            if (Navigator.Instance.CurrentPage is ExamplesHostAppPage)
            {
                Is3DZAxisUpOverridden = Navigator.Instance.CurrentExample.Uri.Contains("ZAxisUp3D");
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
            overrideStyle.Setters.Add(new Setter(PerformanceHelper.EnableExtremeDrawingManagerProperty, EnableExtremeDrawingManager));

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