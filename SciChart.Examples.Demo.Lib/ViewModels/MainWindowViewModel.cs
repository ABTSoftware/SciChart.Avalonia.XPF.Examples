using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SciChart.Charting;
using SciChart.Charting.Visuals;
using SciChart.Core.Utility;
using SciChart.Drawing.VisualXcceleratorRasterizer;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Helpers.HtmlExport;
using SciChart.Examples.Demo.Lib.Helpers.Navigation;
using SciChart.Examples.Demo.Lib.Helpers.ProjectExport;
using SciChart.Examples.Demo.Lib.Traits;
using SciChart.Examples.ExternalDependencies.Common;
using SciChart.UI.Bootstrap;
using SciChart.UI.Reactive.Observability;
using SciChart.Wpf.UI.Transitionz;
using Unity.Attributes;
using ActionCommand = SciChart.Charting.Common.Helpers.ActionCommand;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    [ExportType(typeof(IMainWindowViewModel), CreateAs.Singleton)]
    public class MainWindowViewModel : ViewModelWithTraitsBase, IMainWindowViewModel
    {
        private readonly IBlurParams _defaultParams = new BlurParams() { Duration = 200, From = 10, To = 0 };
        private readonly IBlurParams _blurredParams = new BlurParams() { Duration = 200, From = 0, To = 10 };

        public Guid HomePage => AppPage.HomePageId;
        public string VersionAndLicenseInfo => SciChartSurface.VersionAndLicenseInfo;

        public string VXHardwareAcceleration { get; }
        public List<string> AutoCompleteDataSource { get; }

        public IEnumerable<ExampleCategoryViewModel> Categories { get; set; }
        public IEnumerable<Example> ShowcaseExamples { get; set; }

        public bool IsSettingsShow
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool InitReady
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool SearchBoxEnabled
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public EverythingViewModel EverythingViewModel
        {
            get => GetDynamicValue<EverythingViewModel>();
            set => SetDynamicValue(value);
        }

        public ObservableCollection<ISelectable> SearchResults
        {
            get => GetDynamicValue<ObservableCollection<ISelectable>>();
            set => SetDynamicValue(value);
        }

        public bool HasSearchResults
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool IsBusy
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public ExampleCategoryViewModel SelectedCategory
        {
            get => GetDynamicValue<ExampleCategoryViewModel>();
            set => SetDynamicValue(value);
        }

        public Example SelectedShowcaseExample
        {
            get => GetDynamicValue<Example>();
            set => SetDynamicValue(value);
        }

        public string SearchText
        {
            get => GetDynamicValue<string>();
            set => SetDynamicValue(value);
        }

        public IBlurParams BlurOnSearchParams
        {
            get => GetDynamicValue<IBlurParams>();
            set => SetDynamicValue(value);
        }

        public bool IsMainPage
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public IBlurParams BlurBackgroundParams
        {
            get => GetDynamicValue<IBlurParams>();
            set => SetDynamicValue(value);
        }

        public Example SelectedExample
        {
            get => GetDynamicValue<Example>();
            set
            {
                SetDynamicValue(value);
                ExportToHtmlCommand.RaiseCanExecuteChanged();
                ExportAllHtmlCommand.RaiseCanExecuteChanged();
                ExportAllSolutionsCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand GoBackCommand => Navigator.Instance.GoBackCommand;
        public ICommand GoForwardCommand => Navigator.Instance.GoForwardCommand;

        public ActionCommand NavigateToHomeCommand => Navigator.Instance.NavigateToHomeCommand;

        public ActionCommand HideSearchCommand { get; }
        public ActionCommand ResetSelectedCommand { get; }
        public ActionCommand GCCollectCommand { get; }

        public ActionCommand ShowSettingsCommand { get; }
        public ActionCommand HideSettingsCommand { get; }

        public ActionCommand ExportToHtmlCommand { get; }
        public ActionCommand ExportAllHtmlCommand { get; }
        public ActionCommand ExportAllSolutionsCommand { get; }

        [InjectionConstructor]
        public MainWindowViewModel(IModule module)
        {
            SearchText = string.Empty;
            SearchResults = new ObservableCollection<ISelectable>();

            AutoCompleteDataSource = module.Examples.Select(ex => ex.Value.Title).ToList();
            VXHardwareAcceleration = VisualXcceleratorEngine.SupportsHardwareAcceleration
                    ? (VisualXcceleratorEngine.ActiveGraphicsAPI == VxGraphicsAPI.OpenGL ? "OpenGL" : "DirectX")
                    : "Not Available";
     
            WithTrait<AutoCompleteSearchTrait>();
            WithTrait<ShowcaseCategoriesTrait>();

            HideSearchCommand = new ActionCommand(() => SearchText = null);

            ResetSelectedCommand = new ActionCommand(() =>
            {
                SelectedCategory = Categories.FirstOrDefault(x => x.IsHomeCategory);
                SelectedShowcaseExample = ShowcaseExamples.FirstOrDefault();
            });

            ShowSettingsCommand = new ActionCommand(() =>
            {
                IsSettingsShow = true;
                BlurOnSearchParams = _blurredParams;
            });

            HideSettingsCommand = new ActionCommand(() =>
            {
                IsSettingsShow = false;
                BlurOnSearchParams = _defaultParams;
            });

            ExportToHtmlCommand = new ActionCommand(() =>
            {
                HideSettingsCommand.Execute(null);
                DeveloperModManager.Manage.IsDeveloperMode = false;
                TimedMethod.Invoke(() => HtmlExportHelper.ExportExampleToHtml(SelectedExample)).After(1000).Go();

            }, () => SelectedExample != null && !SciChartRuntimeInfo.IsXPF);

            ExportAllHtmlCommand = new ActionCommand(() =>
            {
                HideSettingsCommand.Execute(null);
                DeveloperModManager.Manage.IsDeveloperMode = false;
                TimedMethod.Invoke(() => HtmlExportHelper.ExportExamplesToHtml(module)).After(1000).Go();

            }, () => SelectedExample != null && !SciChartRuntimeInfo.IsXPF);

            ExportAllSolutionsCommand = new ActionCommand(() =>
            {
                HideSettingsCommand.Execute(null);
                ExportExampleHelper.ExportExamplesToSolutions(module);

            }, () => SelectedExample != null && !SciChartRuntimeInfo.IsXPF);

            GCCollectCommand = new ActionCommand(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForFullGCApproach();
            });
        }
    }
}