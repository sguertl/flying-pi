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
using MikePhil.Charting.Data;
using MikePhil.Charting.Interfaces.Datasets;

namespace WiFiDronection
{
    [Activity(Label = "ShowVisualitionDataActivity",  Theme = "@android:style/Theme.NoTitleBar.Fullscreen", 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorLandscape)]
    public class ShowVisualitionDataActivity : Activity
    {
        private LineChart m_LineChart;
        private List<Entry> m_Entries;
        private CurrentVisualisatonData m_CurVisData;
        private ILineDataSet[] m_DataSet;
        private LineData m_LineData;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ShowVisualisationDataLayout);

            Init();
            AddingPointsToEntries();
           // DesignDataSet();


            this.m_LineData = new LineData(m_DataSet);
            this.m_LineChart.Data = m_LineData;
            this.m_LineChart.Invalidate();
        }

        private void AddingPointsToEntries()
        {
            int count = 0;
            foreach (KeyValuePair<string,List<DataPoint>> dp in m_CurVisData.Points)
            {
                this.m_Entries = new List<Entry>();
                foreach (DataPoint dp2 in dp.Value )
                {
                    m_Entries.Add(new Entry(dp2.X, dp2.Y));
                }
                LineDataSet lds = new LineDataSet(m_Entries, dp.Key);
                m_DataSet.SetValue(lds,count);
                count++;
            }      
        }

        public void DesignDataSet()
        {
       
         //   m_DataSet.SetMode(LineDataSet.Mode)
        }

        private void Init()
        {
            this.m_LineChart = (LineChart) FindViewById<LineChart>(Resource.Id.linechart);
            this.m_CurVisData = CurrentVisualisatonData.Instance;
            this.m_DataSet = new ILineDataSet[m_CurVisData.Points.Count];
        }
    }
}