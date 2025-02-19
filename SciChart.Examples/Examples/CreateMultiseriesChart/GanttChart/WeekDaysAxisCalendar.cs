﻿using System;
using SciChart.Charting.Numerics.Calendars;

namespace SciChart.Examples.Examples.CreateMultiseriesChart.GanttChart
{
    public class WeekDaysAxisCalendar : DiscontinuousDateTimeCalendarBase
    {
        public WeekDaysAxisCalendar()
        {
            SkipDaysInWeek.Add(DayOfWeek.Saturday);
            SkipDaysInWeek.Add(DayOfWeek.Sunday);
        }
    }
}