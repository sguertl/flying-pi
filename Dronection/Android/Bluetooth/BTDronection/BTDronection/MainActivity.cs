/************************************************************************
*                                                                       *
*  Copyright (C) 2017 Infineon Technologies Austria AG.                 *
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
*  File: MainActivity.cs                                                *
*  Created on: 2017-07-19                                               *
*  Author(s): Guertl Sebastian Matthias (IFAT PMM TI COP)               *
*             Klapsch Adrian Vasile (IFAT PMM TI COP)                   *
*             Englert Christoph (IFAT PMM TI COP)                       *
*                                                                       *
*  MainActivity asks the user to activate bluetooth.                    *
*                                                                       *
************************************************************************/

using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Graphics;

namespace BTDronection
{
    [Activity(Label = "BTDronection", MainLauncher = true, Icon = "@drawable/icon", 
        Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {

		// Root path for project folder
		public static string ApplicationFolderPath;

		// Widgets
        private Button mBtShowDevices;
        private LinearLayout mLinearLayout;
        private TextView mTvHeader;
        private Button mBtShowLog;
        private Button mBtHelp;
        private TextView mTvFooter;

        // Bluetooth members
        private BluetoothAdapter mBtAdapter;

		/// <summary>
		/// Entry point for application.
		/// </summary>
		protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            
            Init();

            // Check if bluetooth is supported
            if (mBtAdapter == null)
            {
                Toast.MakeText(ApplicationContext, "Bluetooth is not supported", 0).Show();

                // Display an alert to inform the user that bluetooth is not supported
                AlertDialog alert = new AlertDialog.Builder(this).Create();
                alert.SetTitle("Bluetooth not supported");
                alert.SetMessage("Bluetooth is not supported!");
                alert.SetButton("Ok", (s, ev) => { Finish(); });
                alert.Show();
            }
            else
            {
                // Check if bluetooth is enabled
                if (!mBtAdapter.IsEnabled) 
                { 
                    TurnBTOn(); 
                }
            }

            CreateApplicationFolder();
        }

        /// <summary>
        /// Initizalies and modifies objects
        /// </summary>
        public void Init()
        {
			// Initialize widgets
			mBtAdapter = BluetoothAdapter.DefaultAdapter;
            mBtShowDevices = FindViewById<Button>(Resource.Id.btnShowDevices);
            mLinearLayout = FindViewById<LinearLayout>(Resource.Id.linear);
            mBtShowLog = FindViewById<Button>(Resource.Id.btnShowLogs);
            mBtHelp = FindViewById<Button>(Resource.Id.btnHelp);
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeader);
            mTvFooter = FindViewById<TextView>(Resource.Id.tvFooter);
            
            // Set activity background
            mLinearLayout.SetBackgroundColor(Android.Graphics.Color.White);

            mBtShowLog.Click += delegate
            {
                StartActivity(typeof(LogActivity));
            };

            mBtHelp.Click += delegate
            {
                StartActivity(typeof(HelpActivity));
            };

            // Handle paired devices button click
            mBtShowDevices.Click += delegate
            {
                if (mBtAdapter.IsEnabled)
                {
                    StartActivity(typeof(PairedDevicesActivity));
                }
                else
                {
                    Toast.MakeText(this, "Bluetooth has to be turned on", ToastLength.Short).Show();
                }
            };

            // Create and set font
            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
            mTvHeader.Typeface = font;
            mBtShowDevices.Typeface = font;
            mBtHelp.Typeface = font;
            mTvFooter.Typeface = font;
            mBtShowLog.Typeface = font;
        }

        /// <summary>
        /// Enables bluetooth on the device.
        /// </summary>
        public void TurnBTOn()
        {
            Intent intent = new Intent(BluetoothAdapter.ActionRequestEnable);
            StartActivityForResult(intent, 1);
        }

		/// <summary>
		/// Creates the application folder for internal mobile storage.
		/// </summary>
		private void CreateApplicationFolder()
        {
            ApplicationFolderPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "airything");
            ApplicationFolderPath += Java.IO.File.Separator + "bluetooth";
            var storageDir = new Java.IO.File(ApplicationFolderPath + Java.IO.File.Separator + "settings");
            storageDir.Mkdirs();
            var settingsFile = new Java.IO.File(ApplicationFolderPath + Java.IO.File.Separator + Java.IO.File.Separator + "settings.csv");
            settingsFile.CreateNewFile();
        }

		/// <summary>
		/// Closes socket connection.
		/// </summary>
		protected override void OnDestroy()
        {
            base.OnDestroy();
            SocketConnection sc = SocketConnection.Instance;
            sc.Cancel();
        }

		/// <summary>
		/// Closes socket connection.
		/// </summary>
		protected override void OnStop()
        {
            base.OnStop();
            SocketConnection sc = SocketConnection.Instance;
            sc.Cancel();
        }

    }
}

