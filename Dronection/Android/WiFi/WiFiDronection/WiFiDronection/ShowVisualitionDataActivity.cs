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
using Android.Graphics;
using Java.Util;
using Java.Lang;
using MikePhil.Charting.Components;

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

        private Color[] m_ColorList = { Color.Red, Color.Green, Color.Blue, Color.Brown};
        private List<int> m_Colors;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ShowVisualisationDataLayout);

            Init();
            AddingPointsToEntries();

            this.m_LineData = new LineData(m_DataSet);
            this.m_LineChart.Data = m_LineData;

            foreach (float hc in m_CurVisData.HighContTime)
            {
                LimitLine ll = new LimitLine(hc, "");
                ll.LineColor = new Color(255, 0, 0, 40);
                ll.LineWidth = 30f;
                this.m_LineChart.XAxis.AddLimitLine(ll);
            }

            this.m_LineChart.XAxis.SetDrawLimitLinesBehindData(true);
            this.m_LineChart.Invalidate();
        }

        private void AddingPointsToEntries()
        {
            int count = 0;
            foreach (KeyValuePair<string,List<DataPoint>> dp in m_CurVisData.Points)
            {
                this.m_Entries = new List<Entry>();
                m_Colors = new List<int>();

                foreach (DataPoint dp2 in dp.Value )
                {
                    m_Entries.Add(new Entry(dp2.X, dp2.Y));
                    if (m_CurVisData.HighContTime.Any(x => x == dp2.X))
                    {
                        m_Colors.Add(Color.Black);
                    }
                    else
                    {
                        m_Colors.Add(m_ColorList[count]);
                    }
                        
                }

                LineDataSet lds = new LineDataSet(m_Entries, dp.Key);

                lds.SetColor(m_ColorList[count], 255);
                // lds.SetColors(m_Colors.ToArray());
                lds.SetCircleColors(m_Colors.ToArray());
                // lds.SetCircleColor(m_ColorList[count]);
                lds.SetDrawCircleHole(true);
                lds.SetCircleColorHole(m_ColorList[count]);

                m_DataSet.SetValue(lds,count);
                count++;
            }      
        }

        private void Init()
        {
            this.m_Colors = new List<int>();
            this.m_LineChart = (LineChart) FindViewById<LineChart>(Resource.Id.linechart);
            this.m_CurVisData = CurrentVisualisatonData.Instance;
            this.m_DataSet = new ILineDataSet[m_CurVisData.Points.Count];
        }
    }
}