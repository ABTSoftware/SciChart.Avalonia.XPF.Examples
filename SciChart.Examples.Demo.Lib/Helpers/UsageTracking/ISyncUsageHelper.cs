﻿using System;

namespace SciChart.Examples.Demo.Lib.Helpers.UsageTracking
{
    public interface ISyncUsageHelper
    {
        event EventHandler<EventArgs> EnabledChanged;

        bool Enabled { get; set; }
        void SendUsagesToServer();
        void GetRatingsFromServer();
        void LoadFromIsolatedStorage();
        void WriteToIsolatedStorage();
        void SetUsageOnExamples();
    }
}