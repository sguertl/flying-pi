using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Interfaces.Datasets;
using MikePhil.Charting.Data;
using Android.Graphics;

namespace BTDronection
{
    [Activity(Label = "ShowVisualizationDataActivity", Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorLandscape)]
    public class ShowVisualizationDataActivity : Activity
    {

        private LineChart mLineChart;
        private List<Entry> mEntries;
        private CurrentVisualizationData mCurVisData;
        private ILineDataSet[] mDataSet;
        private LineData mLineData;

        private Color[] mColorList = { Color.Red, Color.Green, Color.Blue, Color.Brown };
        private List<int> mColors;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ShowVisualizationDataActivity);

            Init();
            AddPointsToEntries();

            this.mLineData = new LineData(mDataSet);
            this.mLineChart.Data = mLineData;

            this.mLineChart.XAxis.SetDrawLimitLinesBehindData(true);
            this.mLineChart.Invalidate();
        }

        private void AddPointsToEntries()
        {
            int count = 0;
            foreach (KeyValuePair<string, List<DataPoint>> dp in mCurVisData.Points)
            {
                this.mEntries = new List<Entry>();
                mColors = new List<int>();

                foreach (DataPoint dp2 in dp.Value)
                {
                    mEntries.Add(new Entry(dp2.X, dp2.Y));

                    if(mCurVisData.AltControlTime.Any(x => x == dp2.X))
                    {
                        mColors.Add(Color.Black);
                    }
                    else
                    {
                        mColors.Add(mColorList[count]);
                    }
                }

                LineDataSet lds = new LineDataSet(mEntries, dp.Key);

                lds.SetColor(mColorList[count], 255);
                lds.SetCircleColors(mColors.ToArray());
                lds.SetDrawCircleHole(true);
                lds.SetCircleColorHole(mColorList[count]);

                mDataSet.SetValue(lds, count);
                count++;
                
            }
        }

        private void Init()
        {
            this.mColors = new List<int>();
            this.mLineChart = (LineChart)FindViewById<LineChart>(Resource.Id.linechart);
            this.mCurVisData = CurrentVisualizationData.Instance;
            this.mDataSet = new ILineDataSet[mCurVisData.Points.Count];
        }



    }
}