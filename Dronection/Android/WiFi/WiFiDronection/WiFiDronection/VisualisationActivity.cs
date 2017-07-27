﻿using System;
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

        private ListAdapter m_Adapter;

        private String m_Filename;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.VisualisationLayout);
            this.m_Filename = Intent.GetStringExtra("filename");
            Init();
        }

        private void Init()
        {
            this.m_lvVisualisationData = FindViewById<ListView>(Resource.Id.lvData);
            FillRawDataList();
            m_lvVisualisationData.Adapter = m_Adapter;
            //  m_lvVisualisationData.SetBackgroundColor(Android.Graphics.Color.WhiteSmoke);
            m_lvVisualisationData.DividerHeight = 14;
            this.m_lvVisualisationData.ItemClick += OnListViewItemClick;

            this.m_CurVisData = CurrentVisualisatonData.Instance;
        }

        private void FillRawDataList()
        {
            var projectDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + m_Filename);
            List<string> fileNames = new List<string>();
            string[] fileArray = projectDir.List();

            for (int i = 0; i < fileArray.Length; i++)
            {
                fileArray[i] = fileArray[i].Replace(".csv", "");
            }
            if (fileArray != null)
            {
                fileNames = fileArray.ToList();
            }

            m_Adapter = new ListAdapter(this, fileNames);
        }

        private void OnListViewItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string title = m_Adapter[e.Position];
            string path = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + m_Filename + Java.IO.File.Separator + title+".csv";
            var reader = new Java.IO.BufferedReader(new Java.IO.FileReader(path));
            string line = "";

            if (title.Equals("Controlls"))
            {
                m_CurVisData.Points.Add(title, new List<DataPoint>());
                m_CurVisData.Points.Add(title, new List<DataPoint>());
                m_CurVisData.Points.Add(title, new List<DataPoint>());
                m_CurVisData.Points.Add(title, new List<DataPoint>());
            }
            else
            {
                m_CurVisData.Points.Add(title, new List<DataPoint>()); 
            }

            while ((line = reader.ReadLine()) != null)
            {
                String[] p = line.Split(',');
                if (title.Equals("Controlls"))
                {

                }
                else
                {
                    float x = Convert.ToSingle(p[0]);
                    float y = Convert.ToSingle(p[1]);
                    int h = Convert.ToInt32(p[2]);
                    m_CurVisData.Points.First(d => d.Key.Equals(title)).Value.Add(new DataPoint(x,y));
                    if (h == 0){
                        m_CurVisData.HighContTime.Add(x);
                    }
                }
  
            }
            reader.Close();
            StartActivity(typeof(ShowVisualitionDataActivity));
        }


    }
}