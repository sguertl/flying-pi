/************************************************************************
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
*  File: PairedDevicesActivity.cs                                       *
*  Created on: 2017-07-21                                               *
*  Author(s): Christoph Englert                                         *
*                                                                       *
*  PairedDevicesActivity has a list of all paired devices and can       *
*  search for new bluetooth devices.                                    *
*                                                                       *
************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Bluetooth;
using Android.Graphics;
using Android.Runtime;
using Android.Util;

namespace BTDronection
{
    [Activity(Label = "PairedDevices", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PairedDevicesActivity : Activity
    {
        // Widgets
        private ListView mPeerListView;
        private ListAdapter mAdapter;
        private LinearLayout mLinearLayout;
        private ProgressDialog mProgressDialog;
        private Button mBtSearchDevices;
        private TextView mTvExplanation;
        private TextView mTvHeader;

        // Bluetooth members
		private BluetoothAdapter mBtAdapter;
		private List<BluetoothDevice> mPairedDevices;
		private List<String> mPeers;

        /// <summary>
        /// Creates activity.
        /// </summary>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PairedLayout);
  
            Init();

            // Display paired devices
            GetPairedDevices();
        }


        private void GetPairedDevices()
        {
            // Display all paired devices on a ListView
            foreach (BluetoothDevice device in mPairedDevices)
            {
                if(mPeers.Contains(device.Name + "\n" + device.Address) == false)
                {
                    mPeers.Add(device.Name + "\n" + device.Address);
                }
            }

            //mAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mPeers);
            mAdapter = new ListAdapter(this, mPeers);
            mPeerListView.Adapter = mAdapter;
        }

        /// <summary>
        /// Initializes and modifies objects.
        /// </summary>
        public void Init()
        {
            // Initializing objects
            mPeerListView = FindViewById<ListView>(Resource.Id.listView);
            mLinearLayout = FindViewById<LinearLayout>(Resource.Id.linear2);
            mBtAdapter = BluetoothAdapter.DefaultAdapter;
            mPairedDevices = mBtAdapter.BondedDevices.Where(bd => bd.Name.ToUpper().Contains("RASPBERRY") || bd.Name.ToUpper().Contains("RPI") || bd.Name.ToUpper().Contains("XMC")).ToList();
            mPeers = new List<String>();

            mProgressDialog = new ProgressDialog(this);
            mProgressDialog.SetMessage("Connecting with device");
            mProgressDialog.SetCancelable(false);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            mBtSearchDevices = FindViewById<Button>(Resource.Id.btSearchDevices);
            mBtSearchDevices.Click += OnSearchDevices;
            mBtSearchDevices.Typeface = font;

            mTvExplanation = FindViewById<TextView>(Resource.Id.tvExplanation);
            mTvExplanation.Typeface = font;

            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeader);
            mTvHeader.Typeface = font;

            // Set activity background
            mLinearLayout.SetBackgroundColor(Color.White);

            // Set background color of the ListView
            mPeerListView.DividerHeight = 14;

            // Add handler when clicking on a ListViewItem
            mPeerListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => { OnItemClick(sender, e); };
        }

        /// <summary>
        /// Handles OnItemClick event on Paired Devices item.
        /// Tries to establish a connection.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            TextView view = (TextView)e.View.FindViewById<TextView>(Resource.Id.tvListItem);
            string address = view.Text.Split('\n')[1];
            BluetoothDevice bluetoothDevice = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(address);
            Intent intent = new Intent(BaseContext, typeof(ControllerActivity));
            intent.PutExtra("device", bluetoothDevice);
            StartActivityForResult(intent, 10);
        }

        /// <summary>
        /// Handles OnClick event on Search devices button.
        /// Opens bluetooth settings.
        /// </summary>
        private void OnSearchDevices(object sender, EventArgs e)
        {
            Intent bluetoothSettings = new Intent();
            bluetoothSettings.SetAction(Android.Provider.Settings.ActionBluetoothSettings);
            StartActivityForResult(bluetoothSettings, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if(requestCode == 10)
            {
                Toast.MakeText(ApplicationContext, "Could not connect to device...", ToastLength.Short);
            }
            mPairedDevices = mBtAdapter.BondedDevices.Where(bd => bd.Name.ToUpper().Contains("RASPBERRY") || bd.Name.ToUpper().Contains("RPI") || bd.Name.ToUpper().Contains("XMC")).ToList();
            GetPairedDevices();
        }
    }
}