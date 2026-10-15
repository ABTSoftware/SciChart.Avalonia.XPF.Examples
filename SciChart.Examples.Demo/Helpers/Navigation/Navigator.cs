// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// Navigator.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Unity;
using SciChart.Charting.HistoryManagers;
#if !XPF
using SciChart.Examples.Demo.Controls.BusyIndicator;
#endif
using SciChart.Examples.Demo.Helpers.UsageTracking;
using SciChart.Examples.Demo.ViewModels;
using SciChart.UI.Bootstrap;
using ActionCommand = SciChart.Charting.Common.Helpers.ActionCommand;

namespace SciChart.Examples.Demo.Helpers.Navigation
{
    public class NavigationHistory : HistoryStack<object>
    {
        public NavigationHistory()
        {
            Current = AppPage.HomePageId;
        }
    }

    public class Navigator
    {
        private class ApplicationPageWrapper
        {
            private UserControl _view;
            public AppPage AppPage { get; set; }
            public UserControl View
            {
                get { return _view; }
                set
                {
                    var dView = _view as IDisposable;
                    if (dView != null)
                    {
                        // Disposes the view if it implements IDisposable
                        dView.Dispose();
                    }
                    _view = value;
                }
            }
        }

        #region AttachedProperties

        public static readonly DependencyProperty StartPageProperty = DependencyProperty.RegisterAttached("StartPage", typeof(Guid), typeof(Navigator), new PropertyMetadata(OnStartPageAttached));

        public static void SetStartPage(DependencyObject element, Guid value)
        {
            element.SetValue(StartPageProperty, value);
        }

        public static Guid GetStartPage(DependencyObject element)
        {
            return (Guid)element.GetValue(StartPageProperty);
        }

        public static readonly DependencyProperty ExampleStartPageProperty = DependencyProperty.RegisterAttached("ExampleStartPage", typeof(Guid), typeof(Navigator), new PropertyMetadata(OnExampleStartPageAttached));

        public static void SetExampleStartPage(DependencyObject element, Guid value)
        {
            element.SetValue(ExampleStartPageProperty, value);
        }

        public static Guid GetExampleStartPage(DependencyObject element)
        {
            return (Guid)element.GetValue(ExampleStartPageProperty);
        }

        #endregion

        public const string ExampleNavigationPattern = AppConstants.ExamplesComponentPath + "Examples/{example}";

        private readonly IUsageCalculator _usageCalculator;
        private readonly NavigationHistory _history;

        private readonly MainFrame _mainFrame;
        private readonly ExamplesFrame _examplesFrame;

        private ApplicationPageWrapper _currentExample;

        private Example _pendingExample;

        public AppPage CurrentPage { get; protected set; }
        public AppPage CurrentExample => _currentExample?.AppPage;

        public Action<UserControl, AppPage> AfterNavigation { get; set; }

        public void Navigate(Guid pageId)
        {
            var page = Module.ChartingPages[pageId];
            _mainFrame.Navigate(new Uri(page.Uri, UriKind.RelativeOrAbsolute));

            GoBackCommand.RaiseCanExecuteChanged();
            GoForwardCommand.RaiseCanExecuteChanged();
            NavigateToHomeCommand.RaiseCanExecuteChanged();
        }

        public void Navigate(Example example)
        {
#if !XPF
            // Also covers GoBack, GoForward and opening the first example from the home page, none of which go through
            // Module.NavigateToExampleCommand. Show() is idempotent, so the overlap with it is harmless
            BusyIndicatorService.Instance.Show();
#endif

            // CurrentPage stays null until the main frame reports its first navigation, and a caller coming from
            // outside the UI - the UI Automation ValuePattern the smoke tests navigate with - can arrive before
            // that
            if (CurrentPage?.PageId == AppPage.ExamplesPageId)
            {
                if (CanNavigateTo(example))
                {
                    var page = Module.ChartingPages[example.PageId];
                    _examplesFrame.Navigate(new Uri(page.Uri, UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                // The examples frame does not exist yet, so remember what was asked for and finish the journey
                // in OnMainFrameAfterNavigation. Without this the example is dropped and the application just
                // sits on the examples page - which is what a ValuePattern caller gets on the first navigation
                // after launch, since ExampleView carries the example through a OneTime binding that a caller
                // arriving from outside the UI cannot rely on having been evaluated yet
                var navigationAlreadyPending = _pendingExample != null;
                _pendingExample = example;

                // Re-issuing this would restart the navigation already in flight, and a caller that retries -
                // as the smoke tests do while waiting for the example - would keep it permanently restarting
                if (!navigationAlreadyPending)
                {
                    Navigate(AppPage.ExamplesPageId);
                }
            }
        }

        public bool CanNavigateTo(Guid pageId)
        {
            return _mainFrame.CanNavigateTo(Module.ChartingPages[pageId].Uri);
        }

        public bool CanNavigateTo(Example example)
        {
            return _examplesFrame.CanNavigateTo(Module.ChartingPages[example.PageId].Uri);
        }

        public ActionCommand GoBackCommand { get; }

        public ActionCommand GoForwardCommand { get; }

        public ActionCommand NavigateToHomeCommand { get; }

        public bool CanGoBack => _history.CanUndo();

        public bool CanGoForward => _history.CanRedo();

        public void GoBack()
        {
            var lastExampleView = _currentExample;

            _history.Undo();

            if (_history.Current is Example example)
            {
                Navigate(example);
                ServiceLocator.Container.Resolve<IModule>().CurrentExample = example;
            }
            else
            {
                Navigate((Guid)_history.Current);

                if (_history.Current is Guid pageId && pageId == AppPage.HomePageId)
                {
                    ServiceLocator.Container.Resolve<IMainWindowViewModel>().HideSearchCommand.Execute(null);
                    ServiceLocator.Container.Resolve<IMainWindowViewModel>().ResetSelectedCommand.Execute(null);
                }
            }

            ServiceLocator.Container.Resolve<IExampleViewModel>().BreadCrumbViewModel.IsShowingBreadcrumbNavigation = false;
            ServiceLocator.Container.Resolve<IExampleViewModel>().ExportExampleViewModel.IsExportVisible = false;

            GoBackCommand.RaiseCanExecuteChanged();
            GoForwardCommand.RaiseCanExecuteChanged();
            NavigateToHomeCommand.RaiseCanExecuteChanged();
            
            if (CurrentExample is ExampleAppPage lastExamplePage)
            {
                // Required to release memory from last example 
                lastExamplePage.ViewModel = null;                
            }

            if (lastExampleView != null)
            {
                lastExampleView.View = null;
            }

            ScheduleMemoryCleanup();
        }

        public void GoForward()
        {
            var lastExampleView = _currentExample;
            _history.Redo();

            if (_history.Current is Example example)
            {
                Navigate(example);
                ServiceLocator.Container.Resolve<IModule>().CurrentExample = example;
            }
            else
            {
                Navigate((Guid)_history.Current);

                if (_history.Current is Guid pageId && pageId == AppPage.HomePageId)
                {
                    ServiceLocator.Container.Resolve<IMainWindowViewModel>().HideSearchCommand.Execute(null);
                    ServiceLocator.Container.Resolve<IMainWindowViewModel>().ResetSelectedCommand.Execute(null);
                }
            }

            ServiceLocator.Container.Resolve<IExampleViewModel>().ExportExampleViewModel.IsExportVisible = false;
            ServiceLocator.Container.Resolve<IExampleViewModel>().BreadCrumbViewModel.IsShowingBreadcrumbNavigation = false;

            GoBackCommand.RaiseCanExecuteChanged();
            GoForwardCommand.RaiseCanExecuteChanged();
            NavigateToHomeCommand.RaiseCanExecuteChanged();
            if (CurrentExample is ExampleAppPage lastExamplePage)
            {
                // Required to release memory from last example 
                lastExamplePage.ViewModel = null;
            }

            if (lastExampleView != null)
            {
                lastExampleView.View = null;
            }

            ScheduleMemoryCleanup();
        }

        private void OnExamplesFrameBeforeNavigation()
        {
            if (_currentExample != null)
            {
                _currentExample.View = null;

                ScheduleMemoryCleanup();
            }
        }

        private void OnExamplesFrameAfterNavigation(Exception e, UserControl view)
        {
            _currentExample = _currentExample ?? (new ApplicationPageWrapper());
            _currentExample.View = view;

            if (view != null)
            {
                _currentExample.AppPage = GetCurrentExample();

                if (_currentExample.AppPage != null)
                {
                    _currentExample.AppPage.NewViewModel();

                    AfterNavigation(_currentExample.View, _currentExample.AppPage);
                }
            }
            else
            {
                _currentExample.AppPage = null;
            }

            _usageCalculator.UpdateUsage(CurrentExample);

#if !XPF
            // Outside the view != null check, so that a failed navigation also clears the indicator
            BusyIndicatorService.Instance.HideWhenRendered();
#endif
        }

        private void OnMainFrameAfterNavigation()
        {
            CurrentPage = GetCurrentPage();

            GoBackCommand.RaiseCanExecuteChanged();
            GoForwardCommand.RaiseCanExecuteChanged();
            NavigateToHomeCommand.RaiseCanExecuteChanged();

            _usageCalculator.UpdateUsage(CurrentPage);

            // Deliver an example that was requested before the examples page existed. Cleared unconditionally:
            // once the main frame has landed somewhere else, the request is stale
            var pendingExample = _pendingExample;
            _pendingExample = null;

            if (pendingExample != null && CurrentPage?.PageId == AppPage.ExamplesPageId)
            {
                // A no-op when ExampleView's ExampleStartPage binding already opened it - CanNavigateTo(Example)
                // reports false once the examples frame is on that uri
                Navigate(pendingExample);
            }
        }

        public AppPage GetCurrentExample()
        {
            var uri = _examplesFrame.GetCurrentSource();

            if (uri == null)
                return null;

            var firstOrDefault = Module.ChartingPages.Where(item => item.Value.Uri == uri.OriginalString)
                .Select(item => item.Value)
                .FirstOrDefault();

            return firstOrDefault;
        }

        public AppPage GetCurrentPage()
        {
            var uri = _mainFrame.GetCurrentSource();

            if (uri == null)
                return null;

            var firstOrDefault = Module.ChartingPages.Where(item => item.Value.Uri == uri.OriginalString)
                .Select(item => item.Value)
                .FirstOrDefault();

            return firstOrDefault;
        }

        private static void OnStartPageAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var navigationFrame = (Frame)d;
            var page = GetStartPage(navigationFrame);

            _instance._mainFrame.Frame = navigationFrame;
            _instance._mainFrame.AfterNavigation = _instance.OnMainFrameAfterNavigation;

            _instance.Navigate(page);
            _instance._history.Push(AppPage.HomePageId);
        }

        private static void OnExampleStartPageAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var navigationFrame = (Frame)d;

            _instance._examplesFrame.Frame = navigationFrame;
            _instance._examplesFrame.AfterNavigation = _instance.OnExamplesFrameAfterNavigation;
            _instance._examplesFrame.BeforeNavigation = _instance.OnExamplesFrameBeforeNavigation;

            var page = GetExampleStartPage(navigationFrame);
            var module = ServiceLocator.Container.Resolve<IModule>();
            var example = module.Examples.Single(ex => ex.Key == page).Value;

            _instance.Navigate(example);
        }

        #region Singleton

        private static Navigator _instance;

        public static Navigator Instance => _instance ?? (_instance = new Navigator());

        private Navigator()
        {
            _usageCalculator = ServiceLocator.Container.Resolve<IUsageCalculator>();
            _history = new NavigationHistory();

            _mainFrame = new MainFrame();
            _examplesFrame = new ExamplesFrame();

            GoBackCommand = new ActionCommand(GoBack, () => CanGoBack);
            GoForwardCommand = new ActionCommand(GoForward, () => CanGoForward);
            NavigateToHomeCommand = new ActionCommand(() => Instance.GoToHome(), () => Instance.CurrentPage == null || Instance.CurrentPage.PageId != AppPage.HomePageId);
        }

        private void GoToHome()
        {
            var lastExampleView = _currentExample;

            Instance.Navigate(AppPage.HomePageId);
            _history.Push(AppPage.HomePageId);

            ServiceLocator.Container.Resolve<IExampleViewModel>().BreadCrumbViewModel.IsShowingBreadcrumbNavigation = false;
            ServiceLocator.Container.Resolve<IExampleViewModel>().ExportExampleViewModel.IsExportVisible = false;
            ServiceLocator.Container.Resolve<IMainWindowViewModel>().HideSearchCommand.Execute(null);
            ServiceLocator.Container.Resolve<IMainWindowViewModel>().ResetSelectedCommand.Execute(null);

            // Clears memory on going to home
            ((TransitioningFrame)_examplesFrame.Frame).SetContentNull();
            if (CurrentExample is ExampleAppPage lastExamplePage)
            {
                // Required to release memory from last example 
                lastExamplePage.ViewModel = null;
            }

            if (lastExampleView != null)
            {
                lastExampleView.View = null;
            }

            ScheduleMemoryCleanup();
        }

        #endregion

        private static bool _memoryCleanupPending;

        /// <summary>
        /// Requests a background (non-blocking) Gen2 collection to release the memory held by the previous example.
        /// Posted at idle priority so that switching examples is not stalled by a blocking collect on the UI thread.
        /// We deliberately do not call <see cref="GC.WaitForPendingFinalizers"/> here: the finalizer thread drains the
        /// queue on its own (which is what releases the SciChart native/GPU resources), whereas waiting for it on the
        /// UI thread freezes the application and can deadlock against locks held by those finalizers.
        /// </summary>
        private static void ScheduleMemoryCleanup()
        {
            if (_memoryCleanupPending) return;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null) return;

            // Coalesces rapid example switching into a single collection
            _memoryCleanupPending = true;

            dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                _memoryCleanupPending = false;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false); //NOSONAR
            }));
        }

        public void Push(Example example)
        {
            _history.Push(example);
        }
    }
}