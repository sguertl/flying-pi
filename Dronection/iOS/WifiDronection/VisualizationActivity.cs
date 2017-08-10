using System;
using System.Drawing;
using iOSCharts;
using CoreGraphics;
using Foundation;
using UIKit;

namespace WiFiDronection
{
    [Register("VisualizationActivity")]
    public class VisualizationActivity : UIView
    {

        LineChartView lcv;

        public VisualizationActivity()
        {
            Initialize();
        }

        public VisualizationActivity(RectangleF bounds) : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Red;
            lcv = new LineChartView();

            ChartDataEntry[] cde = new ChartDataEntry[10];
            string labelName = "";

            LineChartDataSet lcds = new LineChartDataSet(cde, labelName);

            LineChartData lcd = new LineChartData();

            lcd.AddDataSet(lcds);

            lcv.Add(this);
            lcv.Data = lcd;

     

            lcv.SetNeedsDisplay();


        }
    }
}