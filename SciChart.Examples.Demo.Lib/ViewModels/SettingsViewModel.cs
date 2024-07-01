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
using SciChart.Examples.Demo.Lib.Traits;
using SciChart.UI.Bootstrap;
using SciChart.UI.Reactive;
using SciChart.UI.Reactive.Observability;
using FullScreenAntiAliasingMode = SciChart.Drawing.VisualXcceleratorRasterizer.FullScreenAntiAliasingMode;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    [ExportType(typeof(ISettingsViewModel), CreateAs.Singleton)]
    public class SettingsViewModel : ViewModelWithTraitsBase, ISettingsViewModel
    {
        private Type _selectedRenderer;

        private VxRenderSettings _renderSettings = new();

#if NETFRAMEWORK

        public string TargetFramework => ".NET Framework 4.6.2";

#elif NETCOREAPP3_1

        public string TargetFramework => ".NET Core 3.1";

#elif NET

        public string TargetFramework => SciChartRuntimeInfo.IsXPF
            ? ".NET 6.0 Avalonia XPF"
            : ".NET 6.0 Windows";
#endif
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

        private void RecreateStyles()
        {
            CreateGlobalStyle<SciChartSurface>();
            CreateGlobalStyle<SciChartRadarSurface>();
            CreateGlobalStyle<SciStockChart>();
        }

        public Type SelectedRenderer
        {
            get => _selectedRenderer;
            set
            {
                if (_selectedRenderer == value || value == null) return;

                if (IsDirectXAvailable)
                {
                    IsDirectXEnabled = value == typeof(VisualXcceleratorRenderSurface);

                    if (IsDirectXEnabled)
                    {
                        try
                        {
                            VisualXcceleratorEngine.AssertSupportsDirectX();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }
                    }
                }

                _selectedRenderer = value;
                OnPropertyChanged(value);
            }
        }

        public bool UseAlternativeFillSourceD3D
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool EnableForceWaitForGPU
        {
            get => GetDynamicValue<bool>();
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

        public bool IsDirectXEnabled
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public SettingsViewModel()
        {
            WithTrait<AllowFeedbackSettingTrait>();

            if (VisualXcceleratorEngine.SupportsHardwareAcceleration)
            {
                if (SciChartRuntimeInfo.IsXPF || VisualXcceleratorEngine.IsUsingOpenGL)
                {
                    IsHardwareAcceleration = VisualXcceleratorEngine.HasOpenGLCapableGpu;
                    IsDirectXAvailable = false;
                }
                else
                {
                    IsHardwareAcceleration = VisualXcceleratorEngine.HasDirectX9CapableGpu ||
                                             VisualXcceleratorEngine.HasDirectX10OrBetterCapableGpu;

                    if (IsHardwareAcceleration)
                    {
                        IsDirectXAvailable = true;

                        UseD3D9 = VisualXcceleratorEngine.DirectXMode == DirectXMode.DirectX9c;
                        UseD3D11 = VisualXcceleratorEngine.DirectXMode == DirectXMode.DirectX11;

                        Use3DAA4x = VisualXcceleratorEngine.AntiAliasingMode == FullScreenAntiAliasingMode.MSAA4x;
                        Use3DAANone = VisualXcceleratorEngine.AntiAliasingMode == FullScreenAntiAliasingMode.None;

                        // Always force wait for draw in UIAutomationTestMode 
                        EnableForceWaitForGPU = DemoSettings.Instance.UIAutomationTestMode;                      
                    }                  
                }
            }

            if (SciChartRuntimeInfo.IsWindowsPlatform)
            {
                IsSimdAvailable = true;
                EnableSimd = true;
            }

            UseAlternativeFillSourceD3D = true;

            EnableResamplingCPlusPlus = true;
            EnableExtremeDrawingManager = false;

            EnableImpossibleMode = false;
            EnableDropShadows = true;

            SelectedRenderer = VisualXcceleratorEngine.SupportsHardwareAcceleration &&
                               !VisualXcceleratorEngine.IsGpuBlacklisted
                ? typeof(VisualXcceleratorRenderSurface)
                : typeof(XamlRenderSurface);

            Observable.CombineLatest(
                    this.WhenPropertyChanged(x => x.UseAlternativeFillSourceD3D),
                    this.WhenPropertyChanged(x => x.EnableForceWaitForGPU),
                    this.WhenPropertyChanged(x => x.EnableResamplingCPlusPlus),
                    this.WhenPropertyChanged(x => x.EnableImpossibleMode),
                    this.WhenPropertyChanged(x => x.EnableExtremeDrawingManager),
                    this.WhenPropertyChanged(x => x.SelectedRenderer),
                    Tuple.Create)
                .Throttle(TimeSpan.FromMilliseconds(1))
                .Subscribe(t =>
                {
                    VisualXcceleratorEngine.EnableForceWaitForGPU = t.Item2;

                    RecreateStyles();
                })
                .DisposeWith(this);

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
                        DirectXMode = UseD3D9
                            ? DirectXMode.DirectX9c
                            : DirectXMode.AutoDetect,
                        
                        FullScreenAntiAliasingMode = Use3DAA4x
                            ? FullScreenAntiAliasingMode.MSAA4x
                            : FullScreenAntiAliasingMode.None,
                    };

                    if (!renderSettings.Equals(_renderSettings))
                    {
                        // Restart 2D engine
                        VisualXcceleratorEngine.RestartEngine(renderSettings);
                        _renderSettings = renderSettings;
                    }
                });

            this.WhenPropertyChanged(x => x.EnableDropShadows)
                .Subscribe(b => EffectManager.EnableDropShadows = b)
                .DisposeWith(this);
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