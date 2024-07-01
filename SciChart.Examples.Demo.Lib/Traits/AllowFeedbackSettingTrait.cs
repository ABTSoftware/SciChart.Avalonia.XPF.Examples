using System;
using System.Reactive.Linq;
using SciChart.Examples.Demo.Lib.Helpers.UsageTracking;
using SciChart.Examples.Demo.Lib.ViewModels;
using SciChart.UI.Reactive;
using SciChart.UI.Reactive.Observability;
using SciChart.UI.Reactive.Traits;

namespace SciChart.Examples.Demo.Lib.Traits
{
    public class AllowFeedbackSettingTrait : ViewModelTrait<SettingsViewModel>
    {
        private readonly ISyncUsageHelper _usageHelper;

        public AllowFeedbackSettingTrait(SettingsViewModel target, ISyncUsageHelper usageHelper) : base(target)
        {
            _usageHelper = usageHelper;

            target.WhenPropertyChanged(x => x.AllowFeedback)
                .Skip(1)
                .Subscribe(allowFeedback =>
                {
                    usageHelper.Enabled = allowFeedback;
                }).DisposeWith(this);

            _usageHelper.EnabledChanged += (s, e) =>
            {
                target.AllowFeedback = _usageHelper.Enabled;
            };
        }
    }
}