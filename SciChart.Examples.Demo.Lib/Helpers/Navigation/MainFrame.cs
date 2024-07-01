using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SciChart.Examples.Demo.Lib.Helpers.Navigation
{
    public class MainFrame : FrameBase
    {
        public Action AfterNavigation { get; set; }
      
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

            successHandler = (s, e) =>
            {
                Frame.Navigated -= successHandler;
                Frame.NavigationFailed -= failureHandler;

                AfterNavigation();
            };

            failureHandler = (s, e) =>
            {
                Frame.Navigated -= successHandler;
                Frame.NavigationFailed -= failureHandler;

                AfterNavigation();
            };

            Frame.Navigated += successHandler;
            Frame.NavigationFailed += failureHandler;
        }
    }
}