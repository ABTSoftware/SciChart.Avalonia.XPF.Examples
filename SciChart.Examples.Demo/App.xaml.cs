using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SciChart.Charting.Visuals.RenderableSeries.Animations;
using SciChart.Examples.Demo.Helpers;
using SciChart.Examples.Demo.Helpers.UsageTracking;
using SciChart.Examples.ExternalDependencies.Common;
using SciChart.Examples.ExternalDependencies.Controls.ExceptionView;
using SciChart.UI.Bootstrap;
using SciChart.UI.Bootstrap.Utility;
using SciChart.UI.Reactive.Traits;
using Unity;

#if !XPF
using SciChart.Charting;
#endif

namespace SciChart.Examples.Demo
{
    public partial class App : Application
    {
        private static ILogFacade _log;
        private Bootstrapper _bootstrapper;

        private const string DevMode = "/DEVMODE";
        private const string TestMode = "/UIAUTOMATIONTESTMODE";
        private const int SplashDelay = 3000;

        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            InitializeComponent();

            InitializeResources();
        }

        public static ILogFacade Log
        {
            get
            {
                if (UIAutomationTestMode)
                {
                    _log ??= new ConsoleLogger();
                }
                else
                {
                    _log ??= LogManagerFacade.GetLogger(typeof(App));
                }
                return _log;
            }
        }

        /// <summary>
        /// UIAutomationTestMode is enabled when /uiautomationTestMode is passed as command argument, and enables as fast as possible startup without animations, delays or unnecessary services. Used by UIAutomationTests
        /// </summary>
        public static bool UIAutomationTestMode { get; private set; }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error("An unhandled exception occurred. Showing view to user...", e.Exception);

            var mainWindowViewModel = ServiceLocator.Container.Resolve<IMainWindowViewModel>();
            var exampleName = mainWindowViewModel?.SelectedExample?.Title ?? "No Example";
            var message = $"Selected Example: {exampleName}";

            var exceptionView = new ExceptionView(e.Exception, message)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            exceptionView.ShowDialog();

            e.Handled = true;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Contains(DevMode, StringComparer.InvariantCultureIgnoreCase))
            {
                DeveloperModManager.Manage.IsDeveloperMode = true;
            }

            if (e.Args.Contains(TestMode, StringComparer.InvariantCultureIgnoreCase))
            {
                // Used in automation testing, disable animations and delays in transitions 
                UIAutomationTestMode = true;
                SeriesAnimationBase.GlobalEnableAnimations = false;
            }

            try
            {
                Thread.CurrentThread.Name = "UI Thread";

                Log.Debug("--------------------------------------------------------------");
                Log.DebugFormat("SciChart.Examples.Demo: Session Started {0:dd MMM yyyy HH:mm:ss}", DateTime.Now);
                Log.Debug("--------------------------------------------------------------");

                var assembliesToSearch = new[]
                {
                    typeof(MainWindowViewModel).Assembly,
                    typeof(AbtBootstrapper).Assembly, // SciChart.UI.Bootstrap
                    typeof(IViewModelTrait).Assembly, // SciChart.UI.Reactive 
                };

                var assemblyDiscovery = new ExplicitAssemblyDiscovery(assembliesToSearch);
                var typeDiscoveryService = new AttributedTypeDiscoveryService(assemblyDiscovery);

                _bootstrapper = new Bootstrapper(ServiceLocator.Container, typeDiscoveryService);

                var initTask = _bootstrapper.InitializeAsync();
                var splashTask = Task.Delay(SplashDelay);
#if !XPF
                MainWindow = new MainWindow();
#else
                MainWindow = new MainWindowXpf();
#endif
                MainWindow.Show();

                await Task.WhenAll(initTask, splashTask);

                if (UIAutomationTestMode)
                {
#if !XPF
                    VisualXcceleratorEngine.EnableForceWaitForGPU = true;
#endif
                }
                else
                {
                    // Do this on background thread
                    _ = Task.Run(() =>
                    {
                        // Syncing usages 
                        var syncHelper = ServiceLocator.Container.Resolve<ISyncUsageHelper>();
                        syncHelper.LoadFromIsolatedStorage();

                        // Try sync with service
                        syncHelper.GetRatingsFromServer();
                        syncHelper.SendUsagesToServer();
                        syncHelper.SetUsageOnExamples();
                    });
                }

                _bootstrapper.OnInitComplete();
            }
            catch (Exception ex)
            {
                Log.Error("Exception:\n\n{0}", ex);

                MessageBox.Show(ex.Message, "Unhandled Exception");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (!UIAutomationTestMode)
            {
                var usageCalc = ServiceLocator.Container.Resolve<IUsageCalculator>();

                usageCalc.UpdateUsage(null);

                var syncHelper = ServiceLocator.Container.Resolve<ISyncUsageHelper>();

                // Consider doing this a bit more often.
                // And not on close as it generates server errors.
                // syncHelper.SendUsagesToServer();
                syncHelper.WriteToIsolatedStorage();
            }
        }

        private static void InitializeResources()
        {
            var appResources = new string[]
            {
                "Themes/Navy.xaml",
                "Resources/Styles/Icons.xaml",
                "Resources/Styles/OpacityMask.xaml",
                "Resources/Styles/TextBlock.xaml",
                "Resources/Styles/AnimatedImage.xaml",
                "Resources/Styles/Buttons.xaml",
                "Resources/Styles/Breadcrumb.xaml",
                "Resources/Styles/EndlessItemsControl.xaml",
                "Resources/Styles/Hyperlink.xaml",
                "Resources/Styles/ListBox.xaml",
                "Resources/Styles/LoadingIndicator.xaml",
                "Resources/Styles/NavigationTabControl.xaml",
                "Resources/Styles/Scrollbars.xaml",
                "Resources/Styles/ShowcaseControl.xaml",
                "Resources/Styles/Tile.xaml",
                "Resources/Styles/TransitioningFrame.xaml",
                "Resources/Styles/WatermarkedAutocomplete.xaml",
                "Resources/Styles/TreeView.xaml"
            };

            foreach (var resourcePath in appResources)
            {
                Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"{AppConstants.DemoComponentPath}{resourcePath}", UriKind.RelativeOrAbsolute)
                });
            }

            var appStyles = new Dictionary<Type, string>
            {
                { typeof(CheckBox), "ToolbarPopupCheckBoxStyle" },
                { typeof(ComboBox), "ToolbarPopupComboBoxStyle" },
                { typeof(DatePicker), "ToolbarPopupDatePickerStyle" },
                { typeof(RadioButton), "ToolbarPopupRadioButtonStyle" },
                { typeof(Slider), "ToolbarPopupSliderStyle" },
                { typeof(TextBox), "ToolbarPopupTextBoxStyle" }
            };

            foreach (var styleDesc in appStyles)
            {
                var baseStyle = (Style)Current.FindResource(styleDesc.Value);

                Current.Resources.Add(styleDesc.Key, new Style(styleDesc.Key, baseStyle));
            }
        }
    }
}