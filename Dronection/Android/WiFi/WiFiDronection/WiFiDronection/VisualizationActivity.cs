﻿/************************************************************************
*																		*
*  Copyright (C) 2017 Infineon Technologies Austria AG.					*
*																		*
*  Licensed under the Apache License, Version 2.0 (the "License");		*
*  you may not use this file except in compliance with the License.		*
*  You may obtain a copy of the License at								*
*																		*
*    http://www.apache.org/licenses/LICENSE-2.0							*
*																		*
*  Unless required by applicable law or agreed to in writing, software	*
*  distributed under the License is distributed on an "AS IS" BASIS,	*
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or		*
*  implied.																*
*  See the License for the specific language governing					*
*  permissions and limitations under the License.						*
*																		*
*																		*
*  File: VisualizationActivity.cs										*
*  Created on: 2017-07-28                                               *
*  Author(s): Englert Christoph (IFAT PMM TI COP)                       *
*																		*
*  VisualizationActivity provides all log files in a listview.			*
*																		*
************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;

namespace WiFiDronection
{
    [Activity(Label = "VisualizationActivity", 
              Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
              ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class VisualizationActivity : Activity
    {
        // Widgets
        private TextView mTvHeaderVisualization;
        private ListView mLvVisualizationData;
        private Button mBtShowChart;

        // Data to visualize
        private CurrentVisualizationData mCurVisData;

        // Customized adapter
        private ListAdapter mAdapter;

        // Name of the log file
        private String mFilename;

        /// <summary>
        /// Creates the activity and sets the filename.
        /// </summary>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.VisualizationLayout);
            this.mFilename = Intent.GetStringExtra("filename");
            Init();
        }

        /// <summary>
        /// Initializes the widgets and the data to be visualized.
        /// </summary>
        private void Init()
        {
            this.mLvVisualizationData = FindViewById<ListView>(Resource.Id.lvData);
            FillRawDataList();
            mLvVisualizationData.Adapter = mAdapter;
            mLvVisualizationData.DividerHeight = 5;
            this.mLvVisualizationData.ItemClick += OnListViewItemClick;


			// Create font
			var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            this.mTvHeaderVisualization = FindViewById<TextView>(Resource.Id.tvHeaderVisualization);
            this.mBtShowChart = FindViewById<Button>(Resource.Id.btnShowChart);

            mTvHeaderVisualization.Typeface = font;
            mBtShowChart.Typeface = font;

            this.mBtShowChart.Click += OnShowChart;

            this.mCurVisData = CurrentVisualizationData.Instance;
            this.mCurVisData.Points = new Dictionary<string, List<DataPoint>>();
            this.mCurVisData.AltControlTime = new List<float>();
        }

        private void OnShowChart(object sender, EventArgs e)
        {
            StartActivity(typeof(ShowVisualizationDataActivity));
        }

        /// <summary>
        /// Fills the raw data list.
        /// </summary>
        private void FillRawDataList()
        {
            var projectDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mFilename);
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

            mAdapter = new ListAdapter(this, fileNames);
        }

        /// <summary>
        /// Handles OnClick event on a list item.
        /// </summary>
        private void OnListViewItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string title = mAdapter[e.Position];
            string path = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mFilename + Java.IO.File.Separator + title + ".csv";
            var reader = new Java.IO.BufferedReader(new Java.IO.FileReader(path));
            string line = "";

            if (title.Equals("controls"))
            {
                //throttle, yaw, pitch, roll
                try
                {
                    mCurVisData.Points.Add("throttle", new List<DataPoint>());
                    mCurVisData.Points.Add("yaw", new List<DataPoint>());
                    mCurVisData.Points.Add("pitch", new List<DataPoint>());
                    mCurVisData.Points.Add("roll", new List<DataPoint>());
                }
                catch (Exception ex)
                {
                    mCurVisData.Points.Remove("throttle");
                    mCurVisData.Points.Remove("yaw");
                    mCurVisData.Points.Remove("pitch");
                    mCurVisData.Points.Remove("roll");
                    e.View.SetBackgroundColor(Color.White);
                    return;
                }
            }
            else
            {
                try
                {
                    mCurVisData.Points.Add(title, new List<DataPoint>());
                }
                catch (Exception ex)
                {
                    mCurVisData.Points.Remove(title);
                    e.View.SetBackgroundColor(Color.White);
                    return;

                }
            }

            while ((line = reader.ReadLine()) != null)
            {
                String[] p = line.Split(',');
                if (title.Equals("controls"))
                {
                    float x = Convert.ToSingle(p[0]);
                    float t = Convert.ToSingle(p[1]);
                    float y = Convert.ToSingle(p[2]);
                    float p2 = Convert.ToSingle(p[3]);
                    float r = Convert.ToSingle(p[4]);
                    int h = Convert.ToInt32(p[5]);
                    mCurVisData.Points["throttle"].Add(new DataPoint(x, t));
                    mCurVisData.Points["yaw"].Add(new DataPoint(x, y));
                    mCurVisData.Points["pitch"].Add(new DataPoint(x, p2));
                    mCurVisData.Points["roll"].Add(new DataPoint(x, r));
                    if (h == 1)
                    {
                        mCurVisData.AltControlTime.Add(x);
                    }
                }
                else if (!title.Equals("settings"))
                {
                    float x = Convert.ToSingle(p[0]);
                    float y = Convert.ToSingle(p[1]);
                    int h = Convert.ToInt32(p[2]);
                    mCurVisData.Points[title].Add(new DataPoint(x, y));
                    if (h == 1)
                    {
                        mCurVisData.AltControlTime.Add(x);
                    }
                }

            }
            reader.Close();

            e.View.SetBackgroundColor(Color.ParseColor("#928285"));
        }


    }
}