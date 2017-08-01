/************************************************************************
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
*  File: RawDataActivity.cs														*
*  Created on: 2017-8-1				*
*  Author(s): Guertl Sebastian Matthias (IFAT PMM TI COP)											*
*																		*
*  <Summary>															*
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
    [Activity(Label = "RawDataActivity", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen")]
    public class RawDataActivity : Activity
    {
        // Widgets
        private TextView mTvHeader;
        private ListView mLvRawData;
        private TextView mTvDisplayRawData;
        private Button mBtBack;

        // Customized list adapter
        private ListAdapter mAdapter;
        // Selected file
        private string mSelectedFile;

        /// <summary>
        /// Creates the activity and initializes widgets.
        /// </summary>
        /// <param name="savedInstanceState">Saved instance state.</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RawDataLayout);

            // Initialize widgets
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderRawData);
            mLvRawData = FindViewById<ListView>(Resource.Id.lvRawData);
            mLvRawData.ItemClick += OnListItemClick;
            mTvDisplayRawData = FindViewById<TextView>(Resource.Id.tvDisplayRawData);
            mBtBack = FindViewById<Button>(Resource.Id.btnBackRawData);
            mBtBack.Click += OnBack;

            Typeface font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
            mTvHeader.Typeface = font;
            mBtBack.Typeface = font;

            // Get selected filename from 
            mSelectedFile = Intent.GetStringExtra("filename");

            FillRawDataList();
        }

        /// <summary>
        /// Fills list with the control types.
        /// </summary>
        private void FillRawDataList()
        {
            var projectDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mSelectedFile);
            List<string> fileNames = new List<string>();
            string[] fileArray = projectDir.List();
            if (fileArray != null)
            {
                fileNames = fileArray.ToList();
            }
            mAdapter = new ListAdapter(this, fileNames);
            mLvRawData.Adapter = mAdapter;
        }

        /// <summary>
        /// Handles OnClick event for list item.
        /// Reads the raw data from a .csv file and diplays it on textview.
        /// </summary>
        private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string fileName = e.View.FindViewById<TextView>(Resource.Id.tvListItem).Text;
            string path = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mSelectedFile + Java.IO.File.Separator + fileName;
            var reader = new Java.IO.BufferedReader(new Java.IO.FileReader(path));
            string line = "";
            string finalText = "";
            while((line = reader.ReadLine()) != null)
            {
                line = line.Replace(',', ' ');
                finalText += line + "\n";
            }
            mTvDisplayRawData.Text = finalText;
            reader.Close();
        }

        /// <summary>
        /// Handles OnClick event for Back button
        /// </summary>
        private void OnBack(object sender, EventArgs e)
        {
            Finish();
        }
    }
}