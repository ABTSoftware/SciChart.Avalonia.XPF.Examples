// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// LoadingIndicator.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
using System.Windows;
using System.Windows.Controls;

namespace SciChart.Examples.Demo.Controls
{
    public class LoadingIndicator : ContentControl
    {
        public static readonly DependencyProperty EllipseDiameterProperty = DependencyProperty.Register
            (nameof(EllipseDiameter), typeof(double), typeof(LoadingIndicator), new PropertyMetadata(8d));

        public static readonly DependencyProperty EllipseIntervalProperty = DependencyProperty.Register
            (nameof(EllipseInterval), typeof(double), typeof(LoadingIndicator), new PropertyMetadata(4d));

        public double EllipseDiameter
        {
            get => (double)GetValue(EllipseDiameterProperty);
            set => SetValue(EllipseDiameterProperty, value);
        }

        public double EllipseInterval
        {
            get => (double)GetValue(EllipseIntervalProperty);
            set => SetValue(EllipseIntervalProperty, value);
        }

        public LoadingIndicator()
        {
            DefaultStyleKey = typeof(LoadingIndicator);
        }
    }
}