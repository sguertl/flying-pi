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
*  File: ControllerActivity.cs											*
*  Created on: 2017-07-19		                                		*
*  Author(s): Guertl Sebastian Matthias (IFAT PMM TI COP)               *
*             Klapsch Adrian Vasile (IFAT PMM TI COP)                   *
*																		*
*  ControllerActivity has two functionalities:                          *
*  1) Choose between controller mode before flight.  					*
*  2) Create ControllerView with Joysticks and settings.                *
*																		*
************************************************************************/

﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;

namespace WiFiDronection
{
    [Activity(Label = "ControllerActivity",
              Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
              MainLauncher = false,
              ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorLandscape
             )]
    public class ControllerActivity : Activity
    {
        // Widgets controller settings
        private TextView mTvHeader;
        private RadioGroup mRgControlMode;
        private RadioButton mRbMode1;
        private RadioButton mRbMode2;
        private ImageView mIvMode1;
        private ImageView mIvMode2;
		private CheckBox mCbxLoggingActive;
		private TextView mTvMinYaw;
		private EditText mEtMinYaw;
		private TextView mTvMaxYaw;
		private EditText mEtMaxYaw;
		private TextView mTvMinPitch;
		private EditText mEtMinPitch;
		private TextView mTvMaxPitch;
		private EditText mEtMaxPitch;
		private TextView mTvMinRoll;
		private EditText mEtMinRoll;
		private TextView mTvMaxRoll;
		private EditText mEtMaxRoll;
		private Button mBtStart;
		private Button mBtBackToMain;

        // Widgets controller
        private SeekBar mSbTrimBar;
        private TextView mTvTrimValue;
        private RadioButton mRbYawTrim;
        private RadioButton mRbPitchTrim;
        private RadioButton mRbRollTrim;
		private Button mBtnAltitudeControl;

		// Socket members
		private bool mIsConnected;
        private SocketConnection mSocketConnection;
        private SocketReader mSocketReader;
        private string mSelectedBssid;
        private Dictionary<string, ControllerSettings> mPeerSettings;
		private bool mLoggingActive;
		private int mMinYaw;
		private int mMaxYaw;
		private int mMinPitch;
		private int mMaxPitch;
		private int mMinRoll;
		private int mMaxRoll;

        // Constants
        private readonly int mMinTrim = -20;

        // Public variables
        public static bool Inverted;

        /// <summary>
        /// Creates activity and initializes widgets.
        /// </summary>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ControllerSettings);

            // Get singleton instance of socket connection
            mSocketConnection = SocketConnection.Instance;

            // Create font
            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            // Initialize widgets
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderSettings);
            mRgControlMode = FindViewById<RadioGroup>(Resource.Id.rgControlMode);
            mRbMode1 = FindViewById<RadioButton>(Resource.Id.rbMode1);
            mRbMode2 = FindViewById<RadioButton>(Resource.Id.rbMode2);
            mIvMode1 = FindViewById<ImageView>(Resource.Id.ivMode1);
            mIvMode2 = FindViewById<ImageView>(Resource.Id.ivMode2);
			mCbxLoggingActive = FindViewById<CheckBox>(Resource.Id.cbxLoggingActive);
			mTvMinYaw = FindViewById<TextView>(Resource.Id.tvMinYaw);
			mEtMinYaw = FindViewById<EditText>(Resource.Id.etMinYaw);
			mTvMaxYaw = FindViewById<TextView>(Resource.Id.tvMaxYaw);
			mEtMaxYaw = FindViewById<EditText>(Resource.Id.etMaxYaw);
			mTvMinPitch = FindViewById<TextView>(Resource.Id.tvMinPitch);
			mEtMinPitch = FindViewById<EditText>(Resource.Id.etMinPitch);
			mTvMaxPitch = FindViewById<TextView>(Resource.Id.tvMaxPitch);
			mEtMaxPitch = FindViewById<EditText>(Resource.Id.etMaxPitch);
			mTvMinRoll = FindViewById<TextView>(Resource.Id.tvMinRoll);
			mEtMinRoll = FindViewById<EditText>(Resource.Id.etMinRoll);
			mTvMaxRoll = FindViewById<TextView>(Resource.Id.tvMaxRoll);
			mEtMaxRoll = FindViewById<EditText>(Resource.Id.etMaxRoll);
			mBtStart = FindViewById<Button>(Resource.Id.btStart);
			mBtBackToMain = FindViewById<Button>(Resource.Id.btnSettingsBack);

            mTvHeader.Typeface = font;
            mRbMode1.Typeface = font;
            mRbMode2.Typeface = font;
            mBtStart.Typeface = font;
            mBtBackToMain.Typeface = font;

            mRbMode1.Click += OnMode1Click;
            mIvMode1.Click += OnMode1Click;

            mRbMode2.Click += OnMode2Click;
            mIvMode2.Click += OnMode2Click;

            mBtStart.Click += OnStartController;
            mBtBackToMain.Click += OnBackToMain;

			mCbxLoggingActive.Click += (sender, e) => mLoggingActive = mCbxLoggingActive.Checked;

			mSelectedBssid = Intent.GetStringExtra("mac");
            mPeerSettings = ReadPeerSettings();

			mCbxLoggingActive.Checked = mLoggingActive;

			mEtMinYaw.Text = mMinYaw.ToString();
			mEtMaxYaw.Text = mMaxYaw.ToString();
			mEtMinPitch.Text = mMinPitch.ToString();
			mEtMaxPitch.Text = mMaxPitch.ToString();
			mEtMinRoll.Text = mMinRoll.ToString();
			mEtMaxRoll.Text = mMaxRoll.ToString();
        }

        /// <summary>
        /// Reads the settings file for a specific peer
        /// </summary>
        /// <returns>Peer with settings</returns>
        private Dictionary<string, ControllerSettings> ReadPeerSettings()
        {
            string fileName = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "settings" + Java.IO.File.Separator + "settings.csv";
            var reader = new Java.IO.BufferedReader(new Java.IO.FileReader(fileName));
            Dictionary<string, ControllerSettings> peerSettings = new Dictionary<string, ControllerSettings>();
            string line = "";
            while((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                string[] trimParts = parts[1].Split(';');
				try
				{
					mLoggingActive = trimParts[3].Equals("true");
					mMinYaw = Convert.ToInt32(trimParts[4]);
					mMaxYaw = Convert.ToInt32(trimParts[5]);
					mMinPitch = Convert.ToInt32(trimParts[6]);
					mMaxPitch = Convert.ToInt32(trimParts[7]);
					mMinRoll = Convert.ToInt32(trimParts[8]);
					mMaxRoll = Convert.ToInt32(trimParts[9]);
				}
				catch (IndexOutOfRangeException ex)
				{
					Toast.MakeText(this, "Can't load settings", ToastLength.Short).Show();
				}
                peerSettings.Add(parts[0], new ControllerSettings
                {
					AltitudeControlActivated = false,
					Inverted = false,
					TrimYaw = Convert.ToInt16(trimParts[0]),
					TrimPitch = Convert.ToInt16(trimParts[1]),
					TrimRoll = Convert.ToInt16(trimParts[2]),
					LoggingActivated = mLoggingActive,
					MinYaw = mMinYaw,
					MaxYaw = mMaxYaw,
					MinPitch = mMinPitch,
					MaxPitch = mMaxPitch,
					MinRoll = mMinRoll,
					MaxRoll = mMaxRoll
                });
            }
            return peerSettings;
        }

        /// <summary>
        /// Handles OnClick event for Start button.
        /// Creates socket connection and opens ControllerView.
        /// Starts socket reader thread.
        /// </summary>
        private void OnStartController(object sender, EventArgs e)
        {
            // Create socket connection
            if(mSocketConnection.WifiSocket.IsConnected == false)
            {
                mSocketConnection.OnStartConnection();
            }

            if(mSocketConnection.WifiSocket.IsConnected == false)
            {
                StartActivity(typeof(MainActivity));
                return;
            }

			mMinYaw = Convert.ToInt32(mEtMinYaw.Text);
			mMaxYaw = Convert.ToInt32(mEtMaxYaw.Text);

			mMinPitch = Convert.ToInt32(mEtMinPitch.Text);
			mMaxPitch = Convert.ToInt32(mEtMaxPitch.Text);

			mMinRoll = Convert.ToInt32(mEtMinRoll.Text);
			mMaxRoll = Convert.ToInt32(mEtMaxRoll.Text);

            // Change to Controller with joysticks
            SetContentView(Resource.Layout.ControllerLayout);

            if(mPeerSettings.Any(kvp => kvp.Key == mSelectedBssid) == true)
            {
                ControllerView.Settings.TrimYaw = mPeerSettings[mSelectedBssid].TrimYaw;
                ControllerView.Settings.TrimPitch = mPeerSettings[mSelectedBssid].TrimPitch;
                ControllerView.Settings.TrimRoll = mPeerSettings[mSelectedBssid].TrimRoll;
				ControllerView.Settings.LoggingActivated = mLoggingActive;
				ControllerView.Settings.MinYaw = mMinYaw;
				ControllerView.Settings.MaxYaw = mMaxYaw;
				ControllerView.Settings.MinPitch = mMinPitch;
				ControllerView.Settings.MaxPitch = mMaxPitch;
				ControllerView.Settings.MinRoll = mMinRoll;
				ControllerView.Settings.MaxRoll = mMaxRoll;
            }

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            // Initialize widgets of ControllerLayout
            mSbTrimBar = FindViewById<SeekBar>(Resource.Id.sbTrimbar);
            mTvTrimValue = FindViewById<TextView>(Resource.Id.tvTrimValue);
            mBtnAltitudeControl = FindViewById<Button>(Resource.Id.btnAltitudeControl);
            mRbYawTrim = FindViewById<RadioButton>(Resource.Id.rbYawTrim);
            mRbPitchTrim = FindViewById<RadioButton>(Resource.Id.rbPitchTrim);
            mRbRollTrim = FindViewById<RadioButton>(Resource.Id.rbRollTrim);

            mTvTrimValue.Typeface = font;
            mBtnAltitudeControl.Typeface = font;
            mRbYawTrim.Typeface = font;
            mRbPitchTrim.Typeface = font;
            mRbRollTrim.Typeface = font;

			mSbTrimBar.Progress = ControllerView.Settings.TrimYaw - mMinTrim;
			mTvTrimValue.Text = ControllerView.Settings.TrimYaw.ToString();

			mSbTrimBar.ProgressChanged += delegate
            {
                if (mRbYawTrim.Checked == true)
                {
                    ControllerView.Settings.TrimYaw = mSbTrimBar.Progress + mMinTrim;
                }
                else if (mRbPitchTrim.Checked == true)
                {
                    ControllerView.Settings.TrimPitch = mSbTrimBar.Progress + mMinTrim;
                }
                else
                {
                    ControllerView.Settings.TrimRoll = mSbTrimBar.Progress + mMinTrim;
                }
                mTvTrimValue.Text = (mSbTrimBar.Progress + mMinTrim).ToString();
            };

			mBtnAltitudeControl.Click += OnAltitudeControlClick;

			mRbYawTrim.Click += delegate
            {
                mTvTrimValue.Text = ControllerView.Settings.TrimYaw.ToString();
                mSbTrimBar.Progress = ControllerView.Settings.TrimYaw - mMinTrim;
            };

            mRbPitchTrim.Click += delegate
            {
                mTvTrimValue.Text = ControllerView.Settings.TrimPitch.ToString();
                mSbTrimBar.Progress = ControllerView.Settings.TrimPitch - mMinTrim;
            };

            mRbRollTrim.Click += delegate
            {
                mTvTrimValue.Text = ControllerView.Settings.TrimRoll.ToString();
                mSbTrimBar.Progress = ControllerView.Settings.TrimRoll - mMinTrim;
            };

            // Start reading from Raspberry
            if(mSocketConnection.WifiSocket.IsConnected == true)
            {
                mSocketReader = new SocketReader(mSocketConnection.InputStream);
                mSocketReader.OnStart();
            }
        }

		/// <summary>
		/// Writes log in csv format.
		/// </summary>
		private void WriteLogData()
		{
			if (mSocketConnection.LogData != null)
			{
				RemoveFolder();
				DateTime time = DateTime.Now;
				string dirName = string.Format("{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
				var storageDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + dirName);
				var writer = new Java.IO.FileWriter(new Java.IO.File(storageDir, "controls.csv"));
                if(mLoggingActive)
                {
					storageDir.Mkdirs();
					writer.Write(mSocketConnection.LogData);
				}
                mPeerSettings[mSelectedBssid] = ControllerView.Settings;
				dirName = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "settings";
				string settingsString = "";
				foreach (KeyValuePair<string, ControllerSettings> kvp in mPeerSettings)
				{
					settingsString += kvp.Key + "," + kvp.Value.TrimYaw + ";" + kvp.Value.TrimPitch + ";" + kvp.Value.TrimRoll + "\n";
				}
				writer.Close();
				writer = new Java.IO.FileWriter(new Java.IO.File(dirName, "settings.csv"));
				writer.Write(settingsString);
				writer.Close();
			}
		}

        /// <summary>
        /// Removes old files.
        /// </summary>
        private void RemoveFolder()
        {
            var root = new Java.IO.File(MainActivity.ApplicationFolderPath);
            List<string> fileNames = root.List().ToList();
            if(fileNames.Count > 16)
            {
                var delFolder = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + fileNames[fileNames.Count - 2]);
                foreach(string delChild in delFolder.List())
                {
                    new Java.IO.File(delFolder.AbsolutePath, delChild).Delete();
                }
                delFolder.Delete();
            }
        }

		/// <summary>
		/// Changes control mode to Mode 1.
		/// </summary>
		private void OnMode1Click(object sender, EventArgs e)
		{
			Inverted = ControllerSettings.INACTIVE;
            mRbMode1.Checked = true;
		}

        /// <summary>
        /// Changes control mode to Mode 2.
        /// </summary>
        private void OnMode2Click(object sender, EventArgs e)
        {
            Inverted = ControllerSettings.ACTIVE;
            mRbMode2.Checked = true;
        }

        /// <summary>
        /// Handles OnClick event on Altitude Control button.
        /// Activates or deactivates altitude control.
        /// </summary>
		private void OnAltitudeControlClick(object sender, EventArgs e)
		{
            Flight tk = Flight.Instance;
            if(ControllerView.Settings.AltitudeControlActivated)
            {
                ControllerView.Settings.AltitudeControlActivated = ControllerSettings.INACTIVE;
				mBtnAltitudeControl.SetBackgroundColor(Color.ParseColor("#005DA9"));
            }
            else 
            {
                ControllerView.Settings.AltitudeControlActivated = ControllerSettings.ACTIVE;
                mBtnAltitudeControl.SetBackgroundColor(Color.ParseColor("#E30034"));
                tk.CV.UpdateOvals(tk.CV.mLeftJS.CenterX, tk.CV.mLeftJS.CenterY);
                tk.CV.Invalidate();
            }
		}

		/// <summary>
		/// Saves log file and close connection when finished.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();
			WriteLogData();
            if (mSocketConnection != null)
            {
                mSocketConnection.OnCancel();
            }
            if (mSocketReader != null)
            {
                mSocketReader.Close();
            }
		}

		/// <summary>
		/// Saves log file and close connection when finished.
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop();
			WriteLogData();
            if (mSocketConnection != null)
            {
                mSocketConnection.OnCancel();
            }
            if (mSocketReader != null)
            {
                mSocketReader.Close();
            }
        }


        /// <summary>
        /// Goes to back to main activity.
        /// </summary>
        private void OnBackToMain(object sender, EventArgs e)
        {
            this.Finish();
        }
    }
}