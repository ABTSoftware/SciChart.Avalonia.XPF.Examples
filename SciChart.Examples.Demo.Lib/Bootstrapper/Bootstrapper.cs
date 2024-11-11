using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using SciChart.Charting;
using SciChart.Charting.Visuals;
using SciChart.Core.Utility;
using SciChart.Drawing.VisualXcceleratorRasterizer;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Search;
using SciChart.Examples.Demo.Lib.ViewModels;
using SciChart.UI.Bootstrap;
using SciChart.UI.Bootstrap.Utility;
using SciChart.UI.Reactive.Async;
using SciChart.UI.Reactive.Observability;
using Unity;

namespace SciChart.Examples.Demo.Lib.Bootstrapper
{
    public class Bootstrapper : AbtBootstrapper, IBootstrapper
    {
        public event EventHandler<EventArgs> WhenInit;

        private readonly ILogFacade _logger = LogManagerFacade.GetLogger(typeof(Bootstrapper));

        public Bootstrapper(IUnityContainer container, IAttributedTypeDiscoveryService service) : base(container, service)
        {
            // IUnityContainer container is ServiceLocator.Container
        }

        private void InitializeEngine()
        {
            try
            {
                VisualXcceleratorEngine.UseAutoShutdown = false;
                VisualXcceleratorEngine.RestartEngine();
            }
            catch
            {
                // Suppress VXEngine initialization errors. All rendering will occur with a fallback
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.InfoFormat("Initializing Async");
                Container.RegisterInstance<IBootstrapper>(this);

                var sc = new SchedulerContext(
                    new SharedScheduler(TaskScheduler.FromCurrentSynchronizationContext(), DispatcherSchedulerEx.Current),
                    new SharedScheduler(TaskScheduler.Default, Scheduler.Default));

                Container.RegisterInstance<ISchedulerContext>(sc);
                ObservableObjectBase.DispatcherSynchronizationContext = SynchronizationContext.Current;
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred in the initialization block: ", ex);
                throw;
            }

            var loadTask = SciChart2DInitializer.LoadLibrariesAndLicenseAsync();

            var indexTask = Task.Run(() =>
            {
                try
                {
                    base.Initialize();

                    var module = Container.Resolve<IModule>();
                    module.Initialize();

                    CreateInvertedIndex.CreateIndex(module.Examples);
                    CreateInvertedIndex.CreateIndexForCode(module.Examples);
                }
                catch (Exception ex)
                {
                    _logger.Error("One or more errors occurred while creating the search index: ", ex);
                    throw;
                }
            });

            await Task.WhenAll(loadTask, indexTask);

            var isSyncEngineInit = SciChartRuntimeInfo.IsLinuxPlatform ||
                                   VisualXcceleratorEngine.ActiveGraphicsAPI == VxGraphicsAPI.OpenGL;

            var initTask = Task.Run(() =>
            {              
                if (!isSyncEngineInit)
                {
                    InitializeEngine();
                }

                var mainViewModel = Container.Resolve<IMainWindowViewModel>();
                var settingsViewModel = Container.Resolve<ISettingsViewModel>();
                var exampleViewModel = Container.Resolve<IExampleViewModel>();

                mainViewModel.SearchBoxEnabled = true;
                mainViewModel.InitReady = true;

                settingsViewModel.InitReady = true;
                exampleViewModel.InitReady = true;
            });
            
            if (isSyncEngineInit)
            {
                InitializeEngine();
            }
            
            await initTask;
        }

        public void OnInitComplete()
        {
            var handler = WhenInit;

            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
