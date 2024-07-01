using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.ViewModels;

namespace SciChart.Examples.Demo.Lib.Helpers.Grouping
{
    public class GroupingByMostUsed : IGrouping
    {
        public GroupingMode GroupingMode { get; set; }

        public GroupingByMostUsed()
        {
            GroupingMode = GroupingMode.MostUsed;
        }

        public ObservableCollection<ISelectable> GroupingPredicate(IDictionary<Guid, Example> examples)
        {
            var groupExamples = new ObservableCollection<ISelectable>
            {
                new EverythingGroupViewModel
                {
                    GroupingIndex = 0,
                    GroupingName = "Most Used"
                }                
            };

            foreach (var example in examples.Select(x => x.Value))
            {
                groupExamples.Add(example);
            }

            return groupExamples;
        }
    }
}