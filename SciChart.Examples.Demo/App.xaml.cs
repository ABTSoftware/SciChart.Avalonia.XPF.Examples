// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// App.xaml.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SciChart.Charting.Visuals.RenderableSeries.Animations;
#if !XPF
using SciChart.Examples.Demo.Controls.BusyIndicator;
#endif
using SciChart.Examples.Demo.Helpers;
using SciChart.Examples.Demo.Helpers.UsageTracking;
using SciChart.Examples.ExternalDependencies.Common;
using SciChart.Examples.ExternalDependencies.Controls.ExceptionView;
using SciChart.UI.Bootstrap;
using SciChart.UI.Bootstrap.Utility;
using SciChart.Charting;
using SciChart.UI.Reactive.Traits;
using Unity;

#if XPF
using System.IO;
using System.Xml;
using log4net;
using log4net.Config;
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

#if XPF
        static App()
        {
            ConfigureLogging();
        }
#endif

        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            InitializeResources();
        }

#if XPF
        /// <summary>
        /// On XPF App.config is not deployed as SciChart.Examples.Demo.Xpf.dll.config (see SciChart.Examples.Demo.Xpf.csproj),
        /// so log4net is configured from the log4net section of App.config, embedded as a resource. XmlConfigurator.Configure()
        /// called by the SciChart.UI.Bootstrap logger then finds no section and leaves this configuration untouched.
        /// The log file goes to the local application data folder of the platform.
        /// </summary>
        private static void ConfigureLogging()
        {
            try
            {
                using var stream = typeof(App).Assembly.GetManifestResourceStream("SciChart.Examples.Demo.App.config");
                if (stream == null) return;

                var document = new XmlDocument();
                document.Load(stream);

                if (document.DocumentElement?["log4net"] is XmlElement log4NetElement)
                {
                    // Create: on Linux ~/.local/share may not exist yet, and GetFolderPath would return "" (a path relative to the exe)
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
                    var logFile = Path.Combine(appData, "SciChart", "SciChart.Examples.Demo.log");

                    foreach (XmlElement file in log4NetElement.SelectNodes("appender/file"))
                    {
                        file.SetAttribute("value", logFile);
                    }

                    XmlConfigurator.Configure(LogManager.GetRepository(typeof(App).Assembly), log4NetElement);
                }
            }
            catch
            {
                // The Demo runs without a log file
            }
        }
#endif

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
#if !XPF
            // The indicator is topmost, so it has to go before the modal exception dialog is shown
            BusyIndicatorService.Instance.HideImmediately();
#endif

            Log.Error("An unhandled exception occurred. Showing view to user...", e.Exception);

            var mainWindowViewModel = ServiceLocator.Container?.Resolve<IMainWindowViewModel>();
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

#if SCICHART_DEVTOOLS && XPF
            // Dev tools only. The benchmark is XPF only.
            // Render backend + present mode from the command line, applied BEFORE the engine starts
            // so they take effect for the whole session (manual verification). Examples:
            //   --backend=d3d11|d3d12|vulkan|opengl|metal   --present=readback|zerocopy
            // Drives both 2D and 3D. (The benchmark path applies these too via Run.)
            SciChart.Examples.Demo.DevTools.Benchmark.DemoBenchmark.ApplyRenderConfigFromArgs(e.Args);

            // Headless performance benchmark (--benchmark / /benchmark, or env SC_BENCH=1): host an
            // example, record, write CSV, exit.
            if (SciChart.Examples.Demo.DevTools.Benchmark.DemoBenchmark.IsEnabled(e.Args))
            {
                SciChart.Examples.Demo.DevTools.Benchmark.DemoBenchmark.Run(e.Args);
                return;
            }
#endif

#if !XPF
            // Started up front so the indicator is warm before the user can select an example. Disabled under UI
            // automation, where an extra topmost window would interfere with the tests. Excluded under XPF:
            // the indicator runs a Window on a second Dispatcher thread, which Avalonia's single UI thread
            // cannot support (SystemColors resolves through the Avalonia dispatcher and throws off-thread).
            BusyIndicatorService.Instance.IsEnabled = !UIAutomationTestMode;
            BusyIndicatorService.Instance.Initialize();
#endif

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
                    VisualXcceleratorEngine.EnableForceWaitForGPU = true;
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

#if !XPF
            try
            {
                BusyIndicatorService.Instance.Shutdown();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to shut down the busy indicator", ex);
            }
#endif

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
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"{AppConstants.ExamplesComponentPath}Themes/Navy.xaml", UriKind.RelativeOrAbsolute)
            });

            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"{AppConstants.ExternalComponentPath}Themes/Navy.xaml", UriKind.RelativeOrAbsolute)
            });

            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"{AppConstants.ExternalComponentPath}Resources/Styles/Controls.xaml", UriKind.RelativeOrAbsolute)
            });

            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"{AppConstants.DemoComponentPath}Themes/Navy.xaml", UriKind.RelativeOrAbsolute)
            });

            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"{AppConstants.DemoComponentPath}Resources/Styles/Controls.xaml", UriKind.RelativeOrAbsolute)
            });
        }
    }
}