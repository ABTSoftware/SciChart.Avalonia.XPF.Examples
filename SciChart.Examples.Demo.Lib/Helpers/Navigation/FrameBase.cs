using System;
using System.Windows.Controls;

namespace SciChart.Examples.Demo.Lib.Helpers.Navigation
{
    public abstract class FrameBase
    {
        private Frame _frame;

        public Frame Frame
        {
            get { return _frame; }
            set
            {
                _frame = value;
            }
        }

        public abstract void Navigate(Uri uri);

        public abstract void GoBack();

        public abstract void GoForward();

        public bool CanGoBack()
        {
            return _frame != null && _frame.CanGoBack;
        }

        public bool CanGoForward()
        {
            return _frame != null && _frame.CanGoForward;
        }

        public bool CanNavigateTo(string uri)
        {
            var isCurrent = IsCurrentSource(uri);
            return Frame != null && !isCurrent;
        }

        private bool IsCurrentSource(string uri)
        {
            return Frame != null &&
                   Frame.CurrentSource != null &&
                   Frame.CurrentSource.OriginalString == uri;
        }

        public Uri GetCurrentSource()
        {
            Uri uri = null;

            if (Frame != null)
            {
                uri = Frame.CurrentSource;
            }
            return uri;
        }
    }
}