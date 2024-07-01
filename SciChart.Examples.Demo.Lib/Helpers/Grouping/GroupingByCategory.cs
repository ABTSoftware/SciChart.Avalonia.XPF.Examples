using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.ViewModels;

namespace SciChart.Examples.Demo.Lib.Helpers.Grouping
{
    public class GroupingByCategory : IGrouping
    {
        public GroupingMode GroupingMode { get; set; }

        public GroupingByCategory()
        {
            GroupingMode = GroupingMode.Category;
        }

        public ObservableCollection<ISelectable> GroupingPredicate(IDictionary<Guid, Example> examples)
        {
            var groups = examples
                .OrderBy(example => example.Value.Title)
                .GroupBy(example => example.Value.Group)
                .OrderBy(group => group.Key);

            var groupIndex = -1;
            var groupExamples = new ObservableCollection<ISelectable>();

            foreach (IGrouping<string, KeyValuePair<Guid, Example>> pairs in groups)
            {
                groupIndex++;
                groupExamples.Add(new EverythingGroupViewModel
                {
                    GroupingIndex = groupIndex,
                    GroupingName = pairs.Key                   
                });

                foreach (var example in pairs.Select(x => x.Value))
                {
                    groupExamples.Add(example);
                }
            }

            return groupExamples;
        }
    }
}