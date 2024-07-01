using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SciChart.Examples.Demo.Lib.Common;
using SciChart.Examples.Demo.Lib.Helpers.Grouping;
using SciChart.Examples.Demo.Lib.Traits;
using SciChart.UI.Reactive.Observability;
using Unity.Attributes;

namespace SciChart.Examples.Demo.Lib.ViewModels
{
    public class EverythingViewModel : ViewModelWithTraitsBase
    {
        private List<IGrouping> _sortingGroups;

        public EverythingViewModel(IDictionary<Guid, Example> stubExamples)
        {
            Examples = stubExamples;
        }

        [InjectionConstructor]
        public EverythingViewModel()
        {
            WithTrait<PopulateExamplesTrait>();            
            SelectedGroupingMode = SortingGroups[0];
        }

        public IDictionary<Guid, Example> Examples
        {
            get => GetDynamicValue<IDictionary<Guid, Example>>(); 
            set => SetDynamicValue(value);
        }

        public ObservableCollection<ISelectable> EverythingSource
        {
            get => GetDynamicValue<ObservableCollection<ISelectable>>();
            set => SetDynamicValue(value);
        }

        public string SelectedCategory
        {
            get => GetDynamicValue<string>(); 
            set => SetDynamicValue(value);
        }

        public IGrouping SelectedGroupingMode
        {
            get => GetDynamicValue<IGrouping>(); 
            set => SetDynamicValue(value);          
        }

        public List<IGrouping> SortingGroups
        {
            get
            {
                _sortingGroups ??= new List<IGrouping>
                {
                    new GroupingByCategory(),
                    new GroupingByFeature(),
                    new GroupingByName(),
                    new GroupingByMostUsed(),
                };

                return _sortingGroups;
            }
        }

        public void UpdateEverythingSource(Func<IDictionary<Guid, Example>, ObservableCollection<ISelectable>> groupingPredicate)
        {
            EverythingSource = groupingPredicate(Examples);
        }
    }
}