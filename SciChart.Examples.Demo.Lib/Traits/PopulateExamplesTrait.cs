using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.ViewModels;
using SciChart.UI.Reactive;
using SciChart.UI.Reactive.Async;
using SciChart.UI.Reactive.Observability;
using SciChart.UI.Reactive.Traits;

namespace SciChart.Examples.Demo.Lib.Traits
{
    public class PopulateExamplesTrait : ViewModelTrait<EverythingViewModel>
    {
        public PopulateExamplesTrait(EverythingViewModel target, IModule module, ISchedulerContext schedulerContext) : base(target)
        {
            var categoryObservable = Target.WhenPropertyChanged(x => x.SelectedCategory).Do(category =>
            {
                var allExamples = module.Examples;
                var categoryExamples = new Dictionary<Guid, Example>();

                foreach (var example in allExamples)
                {
                    if (string.IsNullOrEmpty(category) || example.Value.TopLevelCategory == category)
                    {
                        categoryExamples.Add(example.Key, example.Value);
                    }
                }

                Target.Examples = categoryExamples;
            });

            Observable.CombineLatest(Target.WhenPropertyChanged(x => x.SelectedGroupingMode), categoryObservable, Tuple.Create)
                .Where(tpl => tpl.Item1 != null && tpl.Item2 != null)
                .ObserveOn(schedulerContext.Dispatcher)
                .Subscribe(tpl => Target.UpdateEverythingSource(tpl.Item1.GroupingPredicate))
                .DisposeWith(this);
        }
    }
}