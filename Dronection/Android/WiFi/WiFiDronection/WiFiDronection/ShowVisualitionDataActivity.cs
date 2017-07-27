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

namespace WiFiDronection
{
    [Activity(Label = "ShowVisualitionDataActivity",  Theme = "@android:style/Theme.NoTitleBar.Fullscreen", 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorLandscape)]
    public class ShowVisualitionDataActivity : Activity
    {
        private LineChart m_LineChart;
        private List<Entry> m_Entries;
        private CurrentVisualisatonData m_CurVisData;
        private LineDataSet m_DataSet;
        private LineData m_LineData;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ShowVisualisationDataLayout);

            Init();
            AddingPointsToEntries();
            DesignDataSet();

            this.m_LineData = new LineData(m_DataSet);
            this.m_LineChart.Data = m_LineData;
            this.m_LineChart.Invalidate();
        }

        private void AddingPointsToEntries()
        {
            foreach (DataPoint dp in m_CurVisData.Points)
            {
                m_Entries.Add(new Entry(dp.X, dp.Y));
            }      
        }

        public void DesignDataSet()
        {
            this.m_DataSet = new LineDataSet(m_Entries, "Druck");
         //   m_DataSet.SetMode(LineDataSet.Mode)
        }

        private void Init()
        {
            this.m_LineChart = (LineChart) FindViewById<LineChart>(Resource.Id.linechart);
            this.m_Entries = new List<Entry>();
            this.m_CurVisData = CurrentVisualisatonData.Instance;
        }
    }
}