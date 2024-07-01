using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SciChart.Examples.Demo.Lib.Common;

namespace SciChart.Examples.Demo.Lib.Helpers.Grouping
{
    public interface IGrouping
    {
        GroupingMode GroupingMode { get; set; }

        ObservableCollection<ISelectable> GroupingPredicate(IDictionary<Guid, Example> examples);
    }
}