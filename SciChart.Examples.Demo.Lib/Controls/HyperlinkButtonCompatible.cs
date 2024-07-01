using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using SciChart.Core.Extensions;

namespace SciChart.Examples.Demo.Lib.Controls
{
    public class HyperlinkButtonCompatible : Button, INotifyPropertyChanged
    {
        private string _navigateUri;

        public string NavigateUri
        {
            get => _navigateUri;
            set
            {
                _navigateUri = value;
                OnPropertyChanged(nameof(NavigateUri));
            }
        }

        public HyperlinkButtonCompatible()
        {
            Cursor = Cursors.Hand;
        }

        protected override void OnClick()
        {
            base.OnClick();

            if (!string.IsNullOrWhiteSpace(NavigateUri))
            {
                var uri = new Uri(NavigateUri);
                uri.Launch();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}