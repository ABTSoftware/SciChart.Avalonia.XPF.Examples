using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;
using SciChart.Charting;
using SciChart.Charting.Common.AttachedProperties;
using SciChart.Charting.Common.Extensions;
using SciChart.Charting.Common.MarkupExtensions;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.TradeChart;
using SciChart.Core.Utility;
using SciChart.Data.Numerics.PointResamplers;
using SciChart.Drawing.VisualXcceleratorRasterizer;
using SciChart.Drawing.XamlRasterizer;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Common.Converters;
using SciChart.Examples.Demo.Lib.Helpers.Navigation;
using SciChart.Examples.Demo.Lib.Traits;
using SciChart.UI.Bootstrap;
using SciChart.UI.Reactive;
using SciChart.UI.Reactive.Observability;
using FullScreenAntiAliasingMode = SciChart.Drawing.VisualXcceleratorRasterizer.FullScreenAntiAliasingMode;

#if !HIDE3D
using SciChart.Charting3D;
#endif

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    [ExportType(typeof(ISettingsViewModel), CreateAs.Singleton)]
    public class SettingsViewModel : ViewModelWithTraitsBase, ISettingsViewModel
    {
        private Type _selectedRenderer;

        private VxRenderSettings _renderSettings = new();

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

        public bool IsHardwareAcceleration
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool IsDirectXAvailable
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool IsD3D911Available
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool UseD3D9
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool UseD3D11
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

        public bool IsVxRenderer
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
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

        public bool Is3DAvailable
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool Enable3DZAxisUp
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

        public bool IsSimdAvailable
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

        public SettingsViewModel()
        {
            WithTrait<AllowFeedbackSettingTrait>();

            VersionInfo = $"{SciChartRuntimeInfo.GetVersion()}, {SciChartRuntimeInfo.GetTargetFramework()}";

            if (VisualXcceleratorEngine.SupportsHardwareAcceleration)
            {
                IsHardwareAcceleration = true;

                if (VisualXcceleratorEngine.ActiveGraphicsAPI == VxGraphicsAPI.DirectX)
                {
                    IsDirectXAvailable = true;

                    if (!SciChartRuntimeInfo.IsXPF)
                    {
                        IsD3D911Available = true;

                        UseD3D9 = VisualXcceleratorEngine.DirectXMode == DirectXMode.DirectX9c;
                        UseD3D11 = VisualXcceleratorEngine.DirectXMode == DirectXMode.DirectX11;

                        // Always force wait for draw in UIAutomationTestMode 
                        EnableForceWaitForGPU = DemoSettings.Instance.UIAutomationTestMode;
                    }

                    Use3DAA4x = VisualXcceleratorEngine.AntiAliasingMode == FullScreenAntiAliasingMode.MSAA4x;
                    Use3DAANone = VisualXcceleratorEngine.AntiAliasingMode == FullScreenAntiAliasingMode.None;
                }
            }

            if (SciChartRuntimeInfo.IsWindowsPlatform)
            {
                IsSimdAvailable = true;
                EnableSimd = true;
            }

            Enable3DZAxisUp = false;
            Is3DZAxisUpOverridden = false;
            UseAlternativeFillSource = true;

            EnableResamplingCPlusPlus = true;
            EnableExtremeDrawingManager = false;

            EnableImpossibleMode = false;
            EnableDropShadows = true;

            SelectedRenderer = VisualXcceleratorEngine.SupportsHardwareAcceleration &&
                               !VisualXcceleratorEngine.IsGpuBlacklisted
                ? typeof(VisualXcceleratorRenderSurface)
                : typeof(XamlRenderSurface);

            Observable.CombineLatest(
                    this.WhenPropertyChanged(x => x.UseAlternativeFillSource),
                    this.WhenPropertyChanged(x => x.EnableForceWaitForGPU),
                    this.WhenPropertyChanged(x => x.EnableResamplingCPlusPlus),
                    this.WhenPropertyChanged(x => x.EnableImpossibleMode),
                    this.WhenPropertyChanged(x => x.EnableExtremeDrawingManager),
                    this.WhenPropertyChanged(x => x.SelectedRenderer),
                    Tuple.Create)
                .Throttle(TimeSpan.FromMilliseconds(1))
                .Subscribe(t =>
                {
#if !XPF
                    VisualXcceleratorEngine.EnableForceWaitForGPU = t.Item2;
#endif
                    RecreateStyles();
                })
                .DisposeWith(this);

            if (IsDirectXAvailable)
            {
                Observable.CombineLatest(
                        this.WhenPropertyChanged(x => x.UseD3D9),
                        this.WhenPropertyChanged(x => x.UseD3D11),
                        this.WhenPropertyChanged(x => x.Use3DAANone),
                        this.WhenPropertyChanged(x => x.Use3DAA4x),
                        Tuple.Create)
                    .Skip(1)
                    .Throttle(TimeSpan.FromMilliseconds(1))
                    .Subscribe(t =>
                    {
                        var renderSettings = new VxRenderSettings
                        {
#if !XPF
                            DirectXMode = UseD3D9
                                ? DirectXMode.DirectX9c
                                : DirectXMode.AutoDetect,
#endif
                            FullScreenAntiAliasingMode = Use3DAA4x
                                ? FullScreenAntiAliasingMode.MSAA4x
                                : FullScreenAntiAliasingMode.None
                        };

                        if (!renderSettings.Equals(_renderSettings))
                        {
                            // Restart 2D engine
                            VisualXcceleratorEngine.RestartEngine(renderSettings);
                            _renderSettings = renderSettings;
                        }
                    });
            }

#if !HIDE3D
            Is3DAvailable = true;

            this.WhenPropertyChanged(x => x.Enable3DZAxisUp)
                .Subscribe(zAxisUp =>
                {
                    Viewport3D.SetViewportOrientation(zAxisUp
                        ? Viewport3DOrientation.ZAxisUp
                        : Viewport3DOrientation.YAxisUp);
                })
                .DisposeWith(this);
#endif
            this.WhenPropertyChanged(x => x.EnableDropShadows)
                .Subscribe(value => EffectManager.EnableDropShadows = value)
                .DisposeWith(this);
        }

        public void OnIsVisibleChanged(bool isVisible)
        {
#if !HIDE3D
            if (!isVisible) return;

            Enable3DZAxisUp = Viewport3D.ViewportOrientation == Viewport3DOrientation.ZAxisUp;

            if (Navigator.Instance.CurrentPage is ExamplesHostAppPage)
            {
                Is3DZAxisUpOverridden = Navigator.Instance.CurrentExample.Uri.Contains("ZAxisUp3D");
            }
            else
            {
                Is3DZAxisUpOverridden = false;
            }
#endif
        }

        private void RecreateStyles()
        {
            CreateGlobalStyle<SciChartSurface>();
            CreateGlobalStyle<SciChartRadarSurface>();
            CreateGlobalStyle<SciStockChart>();
        }

        private void CreateGlobalStyle<T>() where T : SciChartSurfaceBase
        {
            var overrideStyle = new Style(typeof(T));
#if !XPF
            var binding = new Binding
            {
                Source = this,
                Converter = new RendererSettingConverter(),
                Mode = BindingMode.OneWay
            };

            overrideStyle.Setters.Add(new Setter(SciChartSurfaceBase.RenderSurfaceProperty, binding));
#endif
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