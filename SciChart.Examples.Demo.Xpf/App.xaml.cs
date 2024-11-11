using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SciChart.Charting;
using SciChart.Charting.Visuals.RenderableSeries.Animations;
using SciChart.Examples.Demo.Lib.Bootstrapper;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Helpers.UsageTracking;
using SciChart.Examples.Demo.Lib.ViewModels;
using SciChart.Examples.ExternalDependencies.Common;
using SciChart.Examples.ExternalDependencies.Controls.ExceptionView;
using SciChart.UI.Bootstrap;
using SciChart.UI.Bootstrap.Utility;
using SciChart.UI.Reactive.Traits;
using Unity;

namespace SciChart.Examples.Demo.Xpf
{
    public partial class App : Application
    {
        private static ILogFacade _log;
        private Bootstrapper _bootstrapper;

        private const string DevMode = "/DEVMODE";
        private const string QuickStart = "/UIAUTOMATIONTESTMODE";
        private const int SplashDelay = 3000;

        public App()
        {
            Startup += OnStartup;
            Exit += OnExit;
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            InitializeComponent();
        }

        public static ILogFacade Log
        {
            get
            {
                _log ??= new ConsoleLogger();
                return _log;
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error("An unhandled exception occurred. Showing view to user...", e.Exception);

            var exceptionView = new ExceptionView(e.Exception)
            {
                Owner = Current?.MainWindow,
                WindowStartupLocation = Current != null
                    ? WindowStartupLocation.CenterOwner
                    : WindowStartupLocation.CenterScreen
            };

            exceptionView.ShowDialog();

            e.Handled = true;
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
#if !HIDE3D
            var surface3DStyle = new Style(typeof(Charting3D.SciChart3DSurface));
            surface3DStyle.Setters.Add(new Setter(ThemeManager.ThemeProperty, "SciChartv7Navy"));
            Resources.Add(typeof(Charting3D.SciChart3DSurface), surface3DStyle);
#endif
            if (e.Args.Contains(DevMode, StringComparer.InvariantCultureIgnoreCase))
            {
                DeveloperModManager.Manage.IsDeveloperMode = true;
            }

            if (e.Args.Contains(QuickStart, StringComparer.InvariantCultureIgnoreCase))
            {
                // Used in automation testing, disable animations and delays in transitions 
                DemoSettings.Instance.UIAutomationTestMode = true;
                SeriesAnimationBase.GlobalEnableAnimations = false;
            }

            try
            {
                Thread.CurrentThread.Name = "UI Thread";

                var assembliesToSearch = new[]
                {
                    typeof(MainWindowViewModel).Assembly, // SciChart.Examples.Demo.Lib
                    typeof(AbtBootstrapper).Assembly,     // SciChart.UI.Bootstrap
                    typeof(IViewModelTrait).Assembly,     // SciChart.UI.Reactive 
                };

                var assemblyDiscovery = new ExplicitAssemblyDiscovery(assembliesToSearch);
                var typeDiscoveryService = new AttributedTypeDiscoveryService(assemblyDiscovery);

                _bootstrapper = new Bootstrapper(ServiceLocator.Container, typeDiscoveryService);

                await Task.WhenAll(_bootstrapper.InitializeAsync(), Task.Delay(SplashDelay));

                if (!DemoSettings.Instance.UIAutomationTestMode)               
                {
                    // Do this on background thread
                    _ = Task.Run(() =>
                    {
                        //Syncing usages 
                        var syncHelper = ServiceLocator.Container.Resolve<ISyncUsageHelper>();
                        syncHelper.LoadFromIsolatedStorage();

                        //Try sync with service
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

        private void OnExit(object sender, ExitEventArgs exitEventArgs)
        {
            if (!DemoSettings.Instance.UIAutomationTestMode)
            {
                var usageCalc = ServiceLocator.Container.Resolve<IUsageCalculator>();

                usageCalc.UpdateUsage(null);

                var syncHelper = ServiceLocator.Container.Resolve<ISyncUsageHelper>();

                // Consider doing this a bit more often.
                // And not on close as it generates server errors.
                //syncHelper.SendUsagesToServer();
                syncHelper.WriteToIsolatedStorage();
            }
        }
    }
}