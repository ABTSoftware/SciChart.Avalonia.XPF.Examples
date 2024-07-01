using System.ComponentModel;

namespace SciChart.Examples.Demo.Lib.Helpers.Grouping
{
    public enum GroupingMode
    {
        [Description("Feature")]
        Feature,      
        
        [Description("Name")]
        Name,

        [Description("Category")]
        Category,

        [Description("Date Released")]
        DateReleased,

        [Description("Most Used")]
        MostUsed,
    }
}