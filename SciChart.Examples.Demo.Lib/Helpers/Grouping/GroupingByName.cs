using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.ViewModels;

namespace SciChart.Examples.Demo.Lib.Helpers.Grouping
{
    public class GroupingByName : IGrouping
    {
        public GroupingMode GroupingMode { get; set; }

        public GroupingByName()
        {
            GroupingMode = GroupingMode.Name;
        }

        public ObservableCollection<ISelectable> GroupingPredicate(IDictionary<Guid, Example> examples)
        {
            var groups = examples
                .Select(example => new { Letter = example.Value.Title.FirstOrDefault(), Example = example.Value })
                .OrderBy(arg => arg.Example.Title)
                .GroupBy(arg => arg.Letter);

            var groupIndex = -1;
            var groupExamples = new ObservableCollection<ISelectable>();

            foreach (var pairs in groups)
            {
                groupIndex++;
                groupExamples.Add(new EverythingGroupViewModel
                {
                    GroupingIndex = groupIndex,
                    GroupingName = pairs.Key.ToString()                    
                });

                foreach (var example in pairs)
                {
                    groupExamples.Add(example.Example);
                }
            }

            return groupExamples;
        }
    }
}