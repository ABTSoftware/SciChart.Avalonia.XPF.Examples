using System;
using SciChart.Examples.ExternalDependencies.Common;

namespace SciChart.Examples.Demo.Lib.Helpers.Navigation
{
    public abstract class AppPage
    {
        protected Type _viewModelType;
        private BaseViewModel _viewModel;

        static AppPage()
        {
            HomePageId = Guid.NewGuid();
            ChartingPageId = Guid.NewGuid();
            ExamplesPageId = Guid.NewGuid();
            EverythingPageId = Guid.NewGuid();
        }
     
        public string Uri { get; protected set; }

        public BaseViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    NewViewModel();
                }
                return _viewModel;
            }
            set
            {
                // Disposes the viewmodel if it implements IDisposable
                _viewModel?.Dispose();
                _viewModel = value;
            }
        }

        public void NewViewModel()
        {
            _viewModel = _viewModelType != null ? (BaseViewModel)Activator.CreateInstance(_viewModelType) : null;
        }

        public Guid PageId { get; protected set; }

        public static Guid HomePageId { get; protected set; }
        public static Guid ChartingPageId { get; protected set; }
        public static Guid ExamplesPageId { get; protected set; }
        public static Guid EverythingPageId { get; protected set; }
    }

    public class ExampleAppPage : AppPage
    {
        public string Title { get; set; }

        public ExampleAppPage(string title, string viewModel, string view)
        {
            Title = title;

            var examplesAssembly = typeof(ExampleLoader).Assembly;
            string typeName = AppConstants.AssemblyName + viewModel;

            _viewModelType = Type.GetType(typeName + ", " + examplesAssembly.FullName);
            Uri = AppConstants.ComponentPath + view;
    
            PageId = Guid.NewGuid();
        }

        //TODO Remove later, added for testing purpose
        public override string ToString()
        {
            return string.Format("Page : {0}", Title);
        }
    }

    public abstract class ViewAppPage : AppPage
    {
        protected abstract string View { get; }
        protected abstract string DesiredUri { get; }
        protected abstract Guid PageGuid { get; }

        protected ViewAppPage()
        {
            Uri = AppConstants.DemoComponentPath + View;
            PageId = PageGuid;      
        }
    }

    public class HomeAppPage : ViewAppPage
    {
        protected override string View { get { return "Views/HomeView.xaml"; } }
        protected override string DesiredUri { get { return "/Home"; } }
        protected override Guid PageGuid { get { return HomePageId; } }
    }
       
    public class EverythingAppPage : ViewAppPage
    {
        protected override string View { get { return "Views/EverythingView.xaml"; } }
        protected override string DesiredUri { get { return "/Everything"; } }
        protected override Guid PageGuid { get { return EverythingPageId; } }
    }
   
    public class ExamplesHostAppPage : ViewAppPage
    {
        protected override string View { get { return "Views/ExampleView.xaml"; } }
        protected override string DesiredUri { get { return "/Example"; } }
        protected override Guid PageGuid { get { return ExamplesPageId; } }
    }  
}