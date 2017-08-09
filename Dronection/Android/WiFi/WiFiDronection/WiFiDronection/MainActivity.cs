﻿﻿/************************************************************************
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
*  File: MainActivity.cs												*
*  Created on: 2017-07-19                               				*
*  Author(s): Guertl Sebastian Matthias (IFAT PMM TI COP)				*
*             Klapsch Adrian Vasile (IFAT PMM TI COP)                   *
*             Englert Christoph (IFAT PMM TI COP)                       *
*																		*
*  MainActivity is responsible for searching a proper wifi connection   *
*  and connecting to it.		    				                	*
*																		*
************************************************************************/

﻿using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.Threading;
using Android.Net.Wifi;
using Android.Runtime;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Views;
using System;

namespace WiFiDronection
{
    [Activity(MainLauncher = true,
        Icon = "@drawable/icon",
        Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class MainActivity : Activity
    {

		// Root path for project folder
		public static string ApplicationFolderPath;

        // Widgets
        private TextView mTvHeader;
        private TextView mTvWifiName;
        private TextView mTvWifiMac;
        private TextView mTvFooter;
        private Button mBtnConnect;
        private Button mBtnShowLogs;
        private Button mBtnHelp;

        // Wifi connection members
        private string mSelectedSsid;
        private string mSelectedBssid;
        private string mLastConnectedPeer;
       // private bool mIsConnected;


        /// <summary>
        /// Entry point for application. Initializes all widgets and sets the
        /// font to them. Turns on wifi it is turned off. 
        /// Calls CreateApplicationFolder() and RefreshWifiList(). 
        /// </summary>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            // Create font
            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            // Initialize widgets
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeader);
            mTvWifiName = FindViewById<TextView>(Resource.Id.tvWifiName);
            mTvWifiMac = FindViewById<TextView>(Resource.Id.tvWifiMac);
            mTvFooter = FindViewById<TextView>(Resource.Id.tvFooter);
            mBtnConnect = FindViewById<Button>(Resource.Id.btnConnect);
            mBtnShowLogs = FindViewById<Button>(Resource.Id.btnShowLogs);
            mBtnHelp = FindViewById<Button>(Resource.Id.btnHelp);

            // Set font to widgets
            mTvHeader.Typeface = font;
            mTvWifiName.Typeface = font;
            mTvWifiMac.Typeface = font;
            mTvFooter.Typeface = font;
            mBtnConnect.Typeface = font;
            mBtnShowLogs.Typeface = font;
            mBtnHelp.Typeface = font;

            // Disable Connect button while searching for connection
            mBtnConnect.Enabled = false;

            // Add events to buttons
            mBtnConnect.Click += OnConnect;
            mBtnShowLogs.Click += OnShowLogFiles;
            mBtnHelp.Click += OnHelp;

            mLastConnectedPeer = "";
            // mIsConnected = false;

            // Turn on wifi if it is turned off
            WifiManager wm = GetSystemService(WifiService).JavaCast<WifiManager>();
            if (wm.IsWifiEnabled == false)
            {
                wm.SetWifiEnabled(true);
            }

            CreateApplicationFolder();
            RefreshWifiList();
        }

        /// <summary>
        /// Scans surroundings for suitable wifi devices and wireless networks.
        /// </summary>
        private void RefreshWifiList()
        {
            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            wifiManager.StartScan();

            // Start searching thread
            ThreadPool.QueueUserWorkItem(x =>
            {
                while (true)
                {
                    Thread.Sleep(3000);
                    var wifiList = wifiManager.ScanResults;

                    if (wifiList != null && wifiList.Count > 0)
                    {
                        // Filter devices by Rasp or Pi
                        IEnumerable<ScanResult> results = wifiList.Where(w => w.Ssid.ToUpper().Contains("RASP") || w.Ssid.ToUpper().Contains("PI"));
                        try
                        {
                            var wifi = results.First();
                            RunOnUiThread(() =>
                            {
                                // Show selected wifi device
                                mSelectedSsid = wifi.Ssid;
                                mSelectedBssid = wifi.Bssid;
                                mTvWifiName.Text = "SSID: " + wifi.Ssid;
                                mTvWifiMac.Text = "MAC: " + wifi.Bssid;
                                mBtnConnect.Enabled = true;
                                mBtnConnect.Text = "Connect";
                                mBtnConnect.SetBackgroundColor(Color.ParseColor("#005DA9"));
                            });
                        }
                        catch(InvalidOperationException ex)
                        {
                            RunOnUiThread(() =>
                            {
                                mBtnConnect.Text = "Can't find WiFi connection";
                            });
                        }
                    }
                }
            });
        }

		/// <summary>
		/// Handles OnClick event of Connect button.
		/// Calls OnCreateDialog() for entering the password if it is the first
		/// time the user connects to the network. Otherwise it starts 
		/// ControllerActivity without asking for the password.
		/// </summary>
		private void OnConnect(object sender, EventArgs e)
		{
			// Check if there is already a connection to the wifi device
			if (mLastConnectedPeer != mSelectedSsid)
			{
				// Open Password dialog for building a wifi connection
				OnCreateDialog(0).Show();
			}
			else
			{
				// Open controller activity
				Intent intent = new Intent(BaseContext, typeof(ControllerActivity));
				// intent.PutExtra("isConnected", mIsConnected);
				intent.PutExtra("mac", mSelectedBssid);
				StartActivity(intent);
			}
		}

        /// <summary>
        /// Creates a dialog for entering the password.
        /// </summary>
        /// <returns>Dialog to enter the password</returns>
        protected override Dialog OnCreateDialog(int id)
        {
            var wifiDialogView = LayoutInflater.Inflate(Resource.Layout.WifiDialog, null);

            var builder = new AlertDialog.Builder(this);
            builder.SetIcon(Resource.Drawable.ifx_logo_small);
            builder.SetView(wifiDialogView);
            builder.SetTitle("Enter WiFi password");
            builder.SetPositiveButton("OK", WpaOkClicked);
            builder.SetNegativeButton("Cancel", (sender, e) => {});

            return builder.Create();
        }

        /// <summary>
        /// Handles OnClick event for OK-button of password dialog.
        /// Tries to connect to the network and start ControllerActivity.
        /// </summary>
        private void WpaOkClicked(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;

            // Get entered password
            var password = (EditText)dialog.FindViewById(Resource.Id.etDialogPassword);

            var conf = new WifiConfiguration();
            conf.Ssid = "\"" + mSelectedSsid + "\"";
            conf.PreSharedKey = "\"" + password.Text + "\"";

            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            // Connect network
            int id = wifiManager.AddNetwork(conf);

            IList<WifiConfiguration> myWifi = wifiManager.ConfiguredNetworks;

            wifiManager.Disconnect();
            wifiManager.EnableNetwork(id, true);
            wifiManager.Reconnect();

            // Check if password is correct
            if (wifiManager.IsWifiEnabled)
            {
                mLastConnectedPeer = mSelectedSsid;
                Intent intent = new Intent(BaseContext, typeof(ControllerActivity));
                // intent.PutExtra("isConnected", mIsConnected);
                intent.PutExtra("mac", mSelectedBssid);
                StartActivity(intent);
                //mIsConnected = true;
            }
            else
            {
                Toast.MakeText(this, "Could not connect to peer", ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// Handles OnClick event on Show Logs-button.
        /// Starts LogActivity.
        /// </summary>
        private void OnShowLogFiles(object sender, EventArgs e)
        {
            StartActivity(typeof(LogActivity));
        }

        /// <summary>
        /// Handles OnClick event on Help-button.
        /// Starts HelpActivity.
        /// </summary>
        private void OnHelp(object sender, EventArgs e)
        {
            StartActivity(typeof(HelpActivity));
        }

        /// <summary>
        /// Creates the application folder for internal mobile storage.
        /// </summary>
        private void CreateApplicationFolder()
        {
            ApplicationFolderPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "Airything");
            ApplicationFolderPath += Java.IO.File.Separator + "wifi";
            var storageDir = new Java.IO.File(ApplicationFolderPath + Java.IO.File.Separator + "settings");
            storageDir.Mkdirs();
            var settingsFile = new Java.IO.File(ApplicationFolderPath + Java.IO.File.Separator + "settings" + Java.IO.File.Separator + "settings.csv");
            settingsFile.CreateNewFile();
        }

        /// <summary>
        /// Closes socket connection and application.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            SocketConnection sc = SocketConnection.Instance;
            sc.OnCancel();
        }

        /// <summary>
        /// Closes socket connection and application.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            SocketConnection sc = SocketConnection.Instance;
            sc.OnCancel();
        }
    }
}