using System.Windows;
using System.Windows.Controls;
using SciChart.Examples.Demo.Lib.Common;

namespace SciChart.Examples.Demo.Lib.Helpers.Navigation
{
    [TemplateVisualState(GroupName = "TransitionStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "TransitionStates", Name = "Transition")]
    public class TransitioningFrame : Frame
    {
        private ContentPresenter _currentContentPresentationSite;

        private ContentPresenter _previousContentPresentationSite;

        public TransitioningFrame()
        {
            DefaultStyleKey = typeof(TransitioningFrame);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _previousContentPresentationSite = GetTemplateChild("PreviousContentPresentationSite") as ContentPresenter;

            _currentContentPresentationSite = GetTemplateChild("CurrentContentPresentationSite") as ContentPresenter;

            if (_currentContentPresentationSite != null)
            {
                _currentContentPresentationSite.Content = Content;
            }
        }

        public void SetContentNull()
        {
            if (_currentContentPresentationSite != null)
            {
                _currentContentPresentationSite.Content = null;
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if ((_currentContentPresentationSite != null) && (_previousContentPresentationSite != null))
            {
                _currentContentPresentationSite.Content = newContent;

                if (!DemoSettings.Instance.UIAutomationTestMode)
                {
                    VisualStateManager.GoToState(this, "Normal", false);
                    VisualStateManager.GoToState(this, "Transition", true);
                }
            }
        }
    }
}
