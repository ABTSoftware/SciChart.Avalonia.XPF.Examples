namespace SciChart.Examples.Demo.Lib.ViewModels
{
    public interface ISettingsViewModel
    {
        bool InitReady { get; set; }

        IMainWindowViewModel ParentViewModel { get; set; }

        void OnIsVisibleChanged(bool isVisible);
    }
}