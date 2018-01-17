﻿/************************************************************************
*                                                                       *
*  Copyright (C) 2017-2018 Infineon Technologies Austria AG.            *
*                                                                       *
*  Licensed under the Apache License, Version 2.0 (the "License");      *
*  you may not use this file except in compliance with the License.     *
*  You may obtain a copy of the License at                              *
*                                                                       *
*    http://www.apache.org/licenses/LICENSE-2.0                         *
*                                                                       *
*  Unless required by applicable law or agreed to in writing, software  *
*  distributed under the License is distributed on an "AS IS" BASIS,    *
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or      *
*  implied.                                                             *
*  See the License for the specific language governing                  *
*  permissions and limitations under the License.                       *
*                                                                       *
*                                                                       *
*  File: LogActivity.cs                                                 *
*  Created on: 2017-07-27                                               *
*  Author(s): Adrian Klapsch                                            *
*                                                                       *
*  LogActivity displays all log files in a listview. A click on an      *
*  item shows the options:                                              *
*  1) Raw Data                                                          *
*  2) Visualize                                                         *
*  3) Delete                                                            *
*                                                                       *
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace WiFiDronection
{
    [Activity(Label = "LogActivity", 
              Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
              ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class LogActivity : Activity
    {
        // Widgets
        private TextView mTvHeader;
        private ListView mLvFiles;
        private TextView mTvEmpty;
        private Button mBtBack;

        // Customized list adapter
        private ListAdapter mAdapter;
        // Selected list item
        private string mSelectedItem;

        /// <summary>
        /// Creates the activity and initializes the widgets.
        /// Calls FillFilesList().
        /// </summary>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Log);

            // Initialize widgets
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderLog);
            mLvFiles = FindViewById<ListView>(Resource.Id.lvFiles);
            mLvFiles.ItemClick += OnShowListItemContextMenu;
			mTvEmpty = FindViewById<TextView>(Resource.Id.tvEmpty);
			mBtBack = FindViewById<Button>(Resource.Id.btnBackLog);

            // Set context menu on listview
            RegisterForContextMenu(mLvFiles);

            mBtBack.Click += OnBackToMain;

			// Create and set font
            Typeface font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
            mTvHeader.Typeface = font;
            mTvEmpty.Typeface = font;
            mBtBack.Typeface = font;
            
            FillFilesList();
        }

        /// <summary>
        /// Displays message if list is empty.
        /// </summary>
        public override void OnContentChanged()
        {
            base.OnContentChanged();
            View empty = FindViewById<View>(Resource.Id.tvEmpty);
            FindViewById<ListView>(Resource.Id.lvFiles).EmptyView = empty;
        }

        /// <summary>
        /// Reads files in project folder and displays them on the list.
        /// </summary>
        private void FillFilesList()
        {
            // Get all file names in project folder
            var projectDir = new Java.IO.File(MainActivity.ApplicationFolderPath);
            List<string> fileNames = new List<string>();
            string[] fileArray = projectDir.List();
            if(fileArray != null)
            {
                fileNames = fileArray.ToList();
                // Sort files by date
                fileNames.Remove("settings");
                fileNames.Sort(new FileComparer());
            }
            // Display on list
            mAdapter = new ListAdapter(this, fileNames);
            mLvFiles.Adapter = mAdapter;
        }

        /// <summary>
        /// Shows context menu after click on list item.
        /// </summary>
        private void OnShowListItemContextMenu(object sender, AdapterView.ItemClickEventArgs e)
        {
            mSelectedItem = e.View.FindViewById<TextView>(Resource.Id.tvListItem).Text;
            mLvFiles.ShowContextMenu();
        }

        /// <summary>
        /// Creates a context menu with three options.
        /// </summary>
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);
            menu.SetHeaderIcon(Resource.Drawable.ifx_logo_small);
            menu.SetHeaderTitle("Options");
            menu.Add(0, v.Id, 0, "Raw data");
            menu.Add(0, v.Id, 0, "Visualize");
            menu.Add(0, v.Id, 0, "Delete");
        }

        /// <summary>
        /// Handles OnClick event for context menu.
        /// Calls ShowRawData() if raw data is selected.
        /// Calls ShowGraph() if visualize is selcted.
        /// Calls DeleteFolder() is delete is selected.
        /// </summary>
        public override bool OnContextItemSelected(IMenuItem item)
        {
            string title = item.ToString().ToLower().Trim();
            switch (title)
            {
                case "raw data": ShowRawData(); break;
                case "visualize": ShowGraph(); break;
                case "delete": DeleteFolder(); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Displays the raw data activity.
        /// </summary>
        private void ShowRawData()
        {
            Intent intent = new Intent(BaseContext, typeof(RawDataActivity));
            intent.PutExtra("filename", mSelectedItem);
            StartActivity(intent);
        }

        /// <summary>
        /// Displays the graphical data visualization activity.
        /// </summary>
        private void ShowGraph()
        {
            Intent intent = new Intent(BaseContext, typeof(VisualizationActivity));
            intent.PutExtra("filename", mSelectedItem);
            StartActivity(intent);
        }

        /// <summary>
        /// Deletes the selected folder from mobile storage.
        /// </summary>
        private void DeleteFolder()
        {
            string dir = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + mSelectedItem;
            var storageDir = new Java.IO.File(dir);
            string[] children = storageDir.List();
            for(int i = 0; i < children.Length; i++)
            {
                new Java.IO.File(dir, children[i]).Delete();
            }
            storageDir.Delete();
            mAdapter.DeleteElement(mSelectedItem);
            mLvFiles.Adapter = mAdapter;
        }

        /// <summary>
        /// Handles OnClick event for Back button.
        /// Goes back to main activity.
        /// </summary>
        private void OnBackToMain(object sender, EventArgs e)
        {
            Finish();
        }
    }

    /// <summary>
    /// Comparer for sorting the list by date.
    /// </summary>
    public class FileComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return y.CompareTo(x);
        }
    }
}