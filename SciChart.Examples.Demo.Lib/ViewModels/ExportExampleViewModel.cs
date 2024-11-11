using System;
using System.ComponentModel;
using System.Threading.Tasks;
using SciChart.Charting.Common.Helpers;
using SciChart.Core.Utility;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Helpers.ProjectExport;
using SciChart.UI.Reactive.Observability;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    public class ExportExampleViewModel : ViewModelWithTraitsBase, IDataErrorInfo
    {
        private readonly ExampleViewModel _parent;

        public ExportExampleViewModel(IModule module, ExampleViewModel parent)
        {
            _parent = parent;

            IsXpfExport = SciChartRuntimeInfo.IsXPF;
            IsXpfLicense = SciChartRuntimeInfo.IsXPF;

            // TODO This code must be removed after uploading XPF NuGet package
            // Do not forget to check and update the ExportExampleView.xaml
            IsFolderPath = SciChartRuntimeInfo.IsXPF;

            ExportMajorVersion = SciChartRuntimeInfo.GetVersion()?.Split('.')[0].Trim('v') ?? "*";
            LibrariesPath = ExportExampleHelper.TryAutomaticallyFindAssemblies();

            SelectExportPathCommand = new ActionCommand(() =>
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ExportPath = dialog.SelectedPath;
                }
            });

            SelectLibraryCommand = new ActionCommand(() =>
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LibrariesPath = dialog.SelectedPath;
                }
            });

            ExportCommand = new ActionCommand(async () =>
            {
                var librariesPath = IsFolderPath ? LibrariesPath : string.Empty;

                string projectName = null;

                CanExport = false;

                if (IsXpfExport)
                {
                    var xpfLicenseKey = IsXpfLicense ? XpfLicenseKey : string.Empty;

                    projectName = ProjectWriter.WriteXpfProject(module.CurrentExample, ExportPath, librariesPath, xpfLicenseKey);
                }
                else
                {
                    projectName = ProjectWriter.WriteProject(module.CurrentExample, ExportPath, librariesPath);
                }

                if (string.IsNullOrEmpty(projectName))
                {
                    OnExportError = true;

                    await Task.Delay(4000);

                    OnExportError = false;
                }
                else
                {
                    OnExportSuccess = true;

                    if (_parent.Usage != null)
                    {
                        _parent.Usage.Exported = true;
                    }

                    await Task.Delay(2000);

                    OnExportSuccess = false;
                }

                CanExport = true;

            }, () => IsValid);

            CancelCommand = new ActionCommand(() => IsExportVisible = false);      
        }

        public ActionCommand SelectExportPathCommand { get; }
        public ActionCommand SelectLibraryCommand { get; }

        public ActionCommand ExportCommand { get; }
        public ActionCommand CancelCommand { get; }

        public string ExportMajorVersion { get; }

        public bool IsXpfExport { get; }

        public bool IsExportVisible
        {
            get => GetDynamicValue<bool>();
            set
            {
                if (IsExportVisible != value)
                {
                    SetDynamicValue(value);

                    CanExport = true;
                    OnExportSuccess = false;
                    OnExportError = false;

                    if (IsExportVisible)
                    {
                        _parent.FeedbackViewModel.IsFeedbackVisible = false;
                        _parent.BreadCrumbViewModel.IsShowingBreadcrumbNavigation = false;
                    }

                    _parent.InvalidateDialogProperties();
                }
            }
        }

        public bool CanExport
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public string ExportPath
        {
            get => GetDynamicValue<string>();
            set
            {
                SetDynamicValue(value);
                ExportCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool IsFolderPath
        {
            get => GetDynamicValue<bool>();
            set
            {
                SetDynamicValue(value);
                ExportCommand?.RaiseCanExecuteChanged();
            }
        }

        public string LibrariesPath
        {
            get => GetDynamicValue<string>();
            set
            {
                SetDynamicValue(value);
                ExportCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool IsXpfLicense
        {
            get => GetDynamicValue<bool>();
            set
            {
                SetDynamicValue(value);
                ExportCommand?.RaiseCanExecuteChanged();
            }
        }

        public string XpfLicenseKey
        {
            get => GetDynamicValue<string>();
            set
            {
                SetDynamicValue(value);
                ExportCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool OnExportSuccess
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        public bool OnExportError
        {
            get => GetDynamicValue<bool>();
            set => SetDynamicValue(value);
        }

        #region IDataErrorInfo

        private readonly string[] ValidatedProperties =
        {
            nameof(ExportPath),
            nameof(LibrariesPath),
            nameof(XpfLicenseKey)
        };

        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                {
                    if (GetValidationError(property) != null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public string Error { get; }

        public string this[string propertyName]
        {
            get { return GetValidationError(propertyName); }
        }

        private string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
            {
                return null;
            }

            string error = null;

            switch (propertyName)
            {
                case nameof(ExportPath):
                    error = ValidateExportPath();
                    break;

                case nameof(LibrariesPath):
                    error = ValidateLibrariesPath();
                    break;

                case nameof(XpfLicenseKey):
                    error = ValidateXpfLicenseKey();
                    break;
            }

            return error;
        }

        private string ValidateExportPath()
        {
            if (DirectoryHelper.IsValidDirectoryPath(ExportPath, out string error))
            {
                DirectoryHelper.HasWriteAccessToDirectory(ExportPath, out error);
            }

            return error;
        }

        private string ValidateLibrariesPath()
        {
            string error = null;

            if (IsFolderPath && DirectoryHelper.IsValidDirectoryPath(LibrariesPath, out error))
            {
                if (!ExportExampleHelper.SearchForCoreAssemblies(LibrariesPath))
                {
                    error = "We are sorry, but you have to manually select path to SciChart installation directory!";
                }
            }

            return error;
        }

        private string ValidateXpfLicenseKey()
        {
            string error = null;

            if (IsXpfExport && IsXpfLicense)
            {
                if (string.IsNullOrEmpty(XpfLicenseKey))
                {
                    error = "We are sorry, but you have to provide a license key!";
                }
            }

            return error;
        }

        #endregion
    }
}