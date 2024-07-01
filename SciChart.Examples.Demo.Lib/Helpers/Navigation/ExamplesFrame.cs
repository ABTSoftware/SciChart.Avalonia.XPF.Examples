using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SciChart.Examples.Demo.Lib.Helpers.Navigation
{
    public class ExamplesFrame : FrameBase
    {
        public Action<Exception, UserControl> AfterNavigation { get; set; }
        public Action BeforeNavigation { get; set; }

        public override void Navigate(Uri uri)
        {
            if (Frame != null)
            {
                AddHandler();
                Frame.Navigate(uri);
            }
        }

        public override void GoBack()
        {
            if (Frame != null)
            {
                AddHandler();
                Frame.GoBack();
            }
        }

        public override void GoForward()
        {
            if (Frame != null)
            {
                AddHandler();
                Frame.GoForward();
            }
        }

        private void AddHandler()
        {
            NavigatedEventHandler successHandler = null;
            NavigationFailedEventHandler failureHandler = null;
            NavigatingCancelEventHandler beforeHandler = null;

            successHandler = (s, e) =>
            {
                Frame.Navigated -= successHandler;
                Frame.NavigationFailed -= failureHandler;

                AfterNavigation(null, e.Content as UserControl);
            };

            failureHandler = (s, e) =>
            {
                Frame.Navigated -= successHandler;
                Frame.NavigationFailed -= failureHandler;

                AfterNavigation(e.Exception, null);
            };

            beforeHandler = (s, e) =>
            {
                Frame.Navigating -= beforeHandler;

                BeforeNavigation();
            };

            Frame.Navigated += successHandler;
            Frame.NavigationFailed += failureHandler;
            Frame.Navigating += beforeHandler;
        }
    }
}