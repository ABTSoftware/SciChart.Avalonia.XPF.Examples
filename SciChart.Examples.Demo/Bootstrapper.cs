using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using SciChart.Charting;
using SciChart.Charting.Visuals;
using SciChart.Core.Utility;
using SciChart.Drawing.VisualXcceleratorRasterizer;
using SciChart.Examples.Demo.Common;
using SciChart.Examples.Demo.Helpers;
using SciChart.Examples.Demo.Search;
using SciChart.Examples.Demo.ViewModels;
using SciChart.UI.Bootstrap;
using SciChart.UI.Bootstrap.Utility;
using SciChart.UI.Reactive.Async;
using SciChart.UI.Reactive.Observability;
using Unity;

namespace SciChart.Examples.Demo
{
    internal interface IBootstrapper
    {
        event EventHandler<EventArgs> WhenInit;
    }

    internal class Bootstrapper : AbtBootstrapper, IBootstrapper
    {
        public event EventHandler<EventArgs> WhenInit;

        private readonly ILogFacade _logger = LogManagerFacade.GetLogger(typeof(Bootstrapper));

        public Bootstrapper(IUnityContainer container, IAttributedTypeDiscoveryService service) : base(container, service)
        {
            Container.RegisterInstance<IBootstrapper>(this);

            var sc = new SchedulerContext(
                new SharedScheduler(TaskScheduler.FromCurrentSynchronizationContext(), DispatcherSchedulerEx.Current),
                new SharedScheduler(TaskScheduler.Default, Scheduler.Default));

            Container.RegisterInstance<ISchedulerContext>(sc);

            ObservableObjectBase.DispatcherSynchronizationContext = SynchronizationContext.Current;
        }

        public async Task InitializeAsync()
        {
			_logger.InfoFormat("Initializing Async");	
			_logger.InfoFormat("1 of 5 base.Initialize()...");
			
            base.Initialize();
			
			_logger.InfoFormat("1 of 5 base.Initialize()...Done");

            var loadTask = SciChart2DInitializer.LoadLibrariesAndLicenseAsync();
			
			_logger.InfoFormat("2 of 5 IModule.Initialize()...");
			_logger.InfoFormat("3 of 5 Create Search Index...");

            var indexTask = Task.Run(() =>
            {
                try
                {
                    var module = Container.Resolve<IModule>();
                    module.Initialize();

                    CreateInvertedIndex.CreateIndex(module.Examples);
                    CreateInvertedIndex.CreateIndexForCode(module.Examples);
                }
                catch (Exception ex)
                {
                    _logger.Error("An error has occurred while initializing the module or creating the search index: ", ex);
                    throw;
                }
            });

            await loadTask;
			
			_logger.InfoFormat("4 of 5 Initialize Visual Xccelerator Engine...");

            var engineTask = Task.CompletedTask;

            try
            {
                VisualXcceleratorEngine.UseAutoShutdown = false;

                if (SciChartRuntimeInfo.IsXPF || VisualXcceleratorEngine.ActiveGraphicsAPI == VxGraphicsAPI.OpenGL)
                {
                    VisualXcceleratorEngine.RestartEngine();
                }
                else
                {
#if !XPF
                    engineTask = VisualXcceleratorEngine.RestartEngineAsync();
#endif
                }
            }
            catch
            {
                // Suppress VXEngine initialization errors. All rendering will occur with a fallback
            }

            await Task.WhenAll(indexTask, engineTask);
			
			_logger.InfoFormat("2 of 5 IModule.Initialize()...Done");
			_logger.InfoFormat("3 of 5 Create Search Index...Done");
			_logger.InfoFormat("4 of 5 Initialize Visual Xccelerator Engine...Done");
			
			_logger.InfoFormat("5 of 5 Resolve View Models...");

            await Task.Run(() =>
            {
                var mainViewModel = Container.Resolve<IMainWindowViewModel>();
                var settingsViewModel = Container.Resolve<ISettingsViewModel>();
                var exampleViewModel = Container.Resolve<IExampleViewModel>();

                mainViewModel.SearchBoxEnabled = true;
                mainViewModel.InitReady = true;

                settingsViewModel.InitReady = true;
                exampleViewModel.InitReady = true;
            });
			
			_logger.InfoFormat("5 of 5 Resolve View Models...Done");
        }

        public void OnInitComplete()
        {
            var handler = WhenInit;

            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
