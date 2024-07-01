using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SciChart.Charting.Model.DataSeries;
using SciChart.Core;
using SciChart.Examples.ExternalDependencies.Common;
using SciChart.Examples.ExternalDependencies.Data;

namespace SciChart.Examples.Examples.ExportAChart.ExportAndScreenshotsChart
{
    /// <summary>
    /// Interaction logic for ExportAndScreenshotOptionsChart.xaml
    /// </summary>
    public partial class ExportAndScreenshotOptionsChart : UserControl
    {
        public ExportAndScreenshotOptionsChart()
        {
            InitializeComponent();
            sciChart.Loaded += OnLoaded_SciChartSurface;
        }

        private void OnLoaded_SciChartSurface(object sender, RoutedEventArgs e)
        {
            // Create multiple DataSeries to store OHLC candlestick data, and Xy moving average data
            var dataSeries0 = new OhlcDataSeries<DateTime, double>() { SeriesName = "Light Blue" };
            var dataSeries1 = new XyDataSeries<DateTime, double>() { SeriesName = "Violet" };
            var dataSeries2 = new XyDataSeries<DateTime, double>() { SeriesName = "Aqua" };
            var dataSeries3 = new XyDataSeries<DateTime, double>() { SeriesName = "Rosy" };

            // Prices are in the format Time, Open, High, Low, Close (all IList)
            var prices = DataManager.Instance.GetPriceData(Instrument.Indu.Value, TimeFrame.Daily);

            // Append data to series. 
            // First series is rendered as a Candlestick Chart so we append OHLC data
            dataSeries0.Append(prices.TimeData, prices.OpenData, prices.HighData, prices.LowData, prices.CloseData);

            // Subsequent series append moving average of the close prices
            dataSeries1.Append(prices.TimeData, DataManager.Instance.ComputeMovingAverage(prices.CloseData, 100));
            dataSeries2.Append(prices.TimeData, DataManager.Instance.ComputeMovingAverage(prices.CloseData, 50));
            dataSeries3.Append(prices.TimeData, DataManager.Instance.ComputeMovingAverage(prices.CloseData, 20));

            sciChart.RenderableSeries[0].DataSeries = dataSeries0;
            sciChart.RenderableSeries[1].DataSeries = dataSeries1;
            sciChart.RenderableSeries[2].DataSeries = dataSeries2;
            sciChart.RenderableSeries[3].DataSeries = dataSeries3;

            boxAnnotation.X1 = new DateTime(2011, 7, 21, 17, 2, 39);
            boxAnnotation.X2 = new DateTime(2011, 3, 16, 6, 44, 18);

            buyTxtAnnot.X1 = new DateTime(2011, 3, 14, 0, 24, 11);
            buyTxtAnnot.X2 = buyTxtAnnot.X1;

            sellTxtAnnot.X1 = new DateTime(2011, 7, 15, 21, 19, 28);
            sellTxtAnnot.X2 = sellTxtAnnot.X1;

            sciChart.ZoomExtents();
        }

        private void ExportToPng(object sender, RoutedEventArgs e)
        {
            if (GetAndCheckPath("PNG | *.png", out string filePath))
            {
                sciChart.ExportToFile(filePath, ExportType.Png, false);
            }
        }

        private void ExportPngInMemory(object sender, RoutedEventArgs e)
        {
            if (GetAndCheckPath("PNG | *.png", out string filePath))
            {
                sciChart.ExportToFile(filePath, ExportType.Png, false, new Size(2000, 1500));
            }
        }

        private static bool GetAndCheckPath(string filter, out string path)
        {
            var isValidPath = false;
            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            path = null;

            while (true)
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    if (IsFileGoodForWriting(saveFileDialog.FileName))
                    {
                        path = saveFileDialog.FileName;
                        isValidPath = true;
                        break;
                    }
                    else
                    {
                        MessageBox.Show("File is inaccesible for writing or you can not create file in this location, please choose another one.");
                    }
                }
                else
                {
                    break;
                }
            }

            return isValidPath;
        }

        /// <summary>
        /// Check if file is Good for writing
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns></returns>
        public static bool IsFileGoodForWriting(string filePath)
        {
            FileStream stream = null;

            try
            {
                stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);

            }
            catch (Exception)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }

            //file is not locked
            return true;
        }
    }
}
