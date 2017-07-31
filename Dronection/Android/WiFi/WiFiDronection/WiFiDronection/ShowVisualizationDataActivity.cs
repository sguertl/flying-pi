﻿using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Data;
using MikePhil.Charting.Interfaces.Datasets;
using Android.Graphics;

namespace WiFiDronection
{
    [Activity(Label = "ShowVisualizationDataActivity",  Theme = "@android:style/Theme.NoTitleBar.Fullscreen", 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorLandscape)]
    public class ShowVisualizationDataActivity : Activity
    {
        // Line chart members
        private LineChart mLineChart;
        private List<Entry> mEntries;
        private CurrentVisualizationData mCurVisData;
        private ILineDataSet[] mDataSet;
        private LineData mLineData;

        // Colors
        private Color[] mColorList = { Color.Red, Color.Green, Color.Blue, Color.Brown};
        private List<int> mColors;

        /// <summary>
        /// Creates the activity and displays the line chart.
        /// </summary>
        /// <param name="savedInstanceState">Saved instance state.</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ShowVisualizationDataLayout);

            Init();
            AddPointsToEntries();

            this.mLineData = new LineData(mDataSet);
            this.mLineChart.Data = mLineData;

            /*foreach (float hc in mCurVisData.HighContTime)
            {
                LimitLine ll = new LimitLine(hc, "");
                ll.LineColor = new Color(255, 0, 0, 40);
                ll.LineWidth = 30f;
                this.mLineChart.XAxis.AddLimitLine(ll);
            }*/

            this.mLineChart.XAxis.SetDrawLimitLinesBehindData(true);
            this.mLineChart.Invalidate();
        }

        /// <summary>
        /// Adds the points to entries.
        /// </summary>
        private void AddPointsToEntries()
        {
            int count = 0;
            foreach (KeyValuePair<string,List<DataPoint>> dp in mCurVisData.Points)
            {
                this.mEntries = new List<Entry>();
                mColors = new List<int>();

                foreach (DataPoint dp2 in dp.Value )
                {
                    mEntries.Add(new Entry(dp2.X, dp2.Y));
                    if (mCurVisData.AltControlTime.Any(x => x == dp2.X))
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
                // lds.SetColors(mColors.ToArray());
                lds.SetCircleColors(mColors.ToArray());
                // lds.SetCircleColor(mColorList[count]);
                lds.SetDrawCircleHole(true);
                lds.SetCircleColorHole(mColorList[count]);

                mDataSet.SetValue(lds,count);
                count++;
            }      
        }

        /// <summary>
        /// Initializes the line chart.
        /// </summary>
        private void Init()
        {
            this.mColors = new List<int>();
            this.mLineChart = (LineChart) FindViewById<LineChart>(Resource.Id.linechart);
            this.mCurVisData = CurrentVisualizationData.Instance;
            this.mDataSet = new ILineDataSet[mCurVisData.Points.Count];
        }
    }
}