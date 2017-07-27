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

namespace WiFiDronection
{
    [Activity(Label = "VisualisationActivity", Theme = "@android:style/Theme.NoTitleBar.Fullscreen")]
    public class VisualisationActivity : Activity
    {
        private ListView m_lvVisualisationData;
        private CurrentVisualisatonData m_CurVisData;

        private String[] m_Names = new String[] { "Druck" };

        private ArrayAdapter<String> m_Adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.VisualisationLayout);
            string path = Intent.GetStringExtra("filename");
            Init();
        }

        private void Init()
        {
            this.m_Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, m_Names);

            this.m_lvVisualisationData = FindViewById<ListView>(Resource.Id.lvData);
            m_lvVisualisationData.Adapter = m_Adapter;
            //  m_lvVisualisationData.SetBackgroundColor(Android.Graphics.Color.WhiteSmoke);
            m_lvVisualisationData.DividerHeight = 14;
            this.m_lvVisualisationData.ItemClick += OnListViewItemClick;

            this.m_CurVisData = CurrentVisualisatonData.Instance;


            Random rand = new Random();

            for (int x = 1; x <= 100; x++)
            {
                m_CurVisData.Points.Add(new DataPoint(x, rand.Next(-50,50)));
            }

        }

        private void OnListViewItemClick(object sender, EventArgs e)
        {
            StartActivity(typeof(ShowVisualitionDataActivity));
        }
    }
}