﻿/************************************************************************
*                                                                       *
*  Copyright (C) 2017-2018 Infineon Technologies Austria AG.            *
*                                                                       *
*  Licensed under the Apache License, Version 2.0 (the "License");      *
*  you may not use this file except in compliance with the License.		*
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
*  File: ControllerActivity.cs                                          *
*  Created on: 2017-07-19                                               *
*  Author(s): Sebastian Guertl                                          *
*             Adrian Klapsch                                            *
*                                                                       *
*  ControllerActivity has two functionalities:                          *
*  1) Choose between controller mode before flight.                     *
*  2) Create ControllerView with Joysticks and settings.                *
*                                                                       *
************************************************************************/

﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using Android.Util;

namespace WiFiDronection
{
    public delegate void RaspberryClose();

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
        private Button mBtLoggingOptions;
        private LinearLayout mLlLoggingOptions;
        private CheckBox mCbxBattery;
        private CheckBox mCbxRadarData;
        private CheckBox mCbxCollisionStatus;
        private CheckBox mCbxControlsMobile;
        private CheckBox mCbxControlsDrone;
        private CheckBox mCbxDebug1;
        private CheckBox mCbxDebug2;
        private CheckBox mCbxDebug3;
        private CheckBox mCbxDebug4;
        private Button mBtExpandTrimOptions;
        private LinearLayout mLlMinMaxYaw;
		private TextView mTvMinYaw;
		private EditText mEtMinYaw;
		private TextView mTvMaxYaw;
		private EditText mEtMaxYaw;
        private LinearLayout mLlMinMaxPitch;
		private TextView mTvMinPitch;
		private EditText mEtMinPitch;
		private TextView mTvMaxPitch;
		private EditText mEtMaxPitch;
        private LinearLayout mLlMinMaxRoll;
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
        private SocketConnection mSocketConnection;
        private string mSelectedBssid;
        private Dictionary<string, ControllerSettings> mPeerSettings;
		private bool mLoggingActive;
        private bool mLogBatteryActive;
        private bool mLogRadarActive;
        private bool mLogCollisionStatusActive;
        private bool mLogControlsMobileActive;
        private bool mLogControlsDroneActive;
        private bool mLogDebug1Active;
        private bool mLogDebug2Active;
        private bool mLogDebug3Active;
        private bool mLogDebug4Active;
        private int mMinYaw;
		private int mMaxYaw;
		private int mMinPitch;
		private int mMaxPitch;
		private int mMinRoll;
		private int mMaxRoll;
        private ControllerView mControllerView;

        // Constants
        private readonly int mMinTrim = -20;
        private readonly byte INIT_COMMUNICATION = 0xA;
        private readonly byte END_COMMUNICATION = 0x63;

        // Public variables
        public static bool Inverted;

        /// <summary>
        /// Creates activity and initializes, modifies and handles events for
        /// all widgets. Calls ReadPeerSettings() to read the settings from 
        /// a file and set them for the flight.
        /// </summary>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ControllerSettings);

            // Get singleton instance of socket connection
            mSocketConnection = new SocketConnection(CloseOnRPIReset);

            // Create font
            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            // Initialize widgets
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderSettings);
            mRgControlMode = FindViewById<RadioGroup>(Resource.Id.rgControlMode);
            mRbMode1 = FindViewById<RadioButton>(Resource.Id.rbMode1);
            mRbMode2 = FindViewById<RadioButton>(Resource.Id.rbMode2);
            mIvMode1 = FindViewById<ImageView>(Resource.Id.ivMode1);
            mIvMode2 = FindViewById<ImageView>(Resource.Id.ivMode2);
            mBtLoggingOptions = FindViewById<Button>(Resource.Id.btnLoggingOptions);
            mLlLoggingOptions = FindViewById<LinearLayout>(Resource.Id.layoutLoggingOptions);
            mCbxBattery = FindViewById<CheckBox>(Resource.Id.cbxLogBattery);
            mCbxRadarData = FindViewById<CheckBox>(Resource.Id.cbxLogRadardata);
            mCbxCollisionStatus = FindViewById<CheckBox>(Resource.Id.cbxLogCollisionStatus);
            mCbxControlsMobile = FindViewById<CheckBox>(Resource.Id.cbxLogControlsMobile);
            mCbxControlsDrone = FindViewById<CheckBox>(Resource.Id.cbxLogControlsDrone);
            mCbxDebug1 = FindViewById<CheckBox>(Resource.Id.cbxLogDebug1);
            mCbxDebug2 = FindViewById<CheckBox>(Resource.Id.cbxLogDebug2);
            mCbxDebug3 = FindViewById<CheckBox>(Resource.Id.cbxLogDebug3);
            mCbxDebug4 = FindViewById<CheckBox>(Resource.Id.cbxLogDebug4);
            mBtExpandTrimOptions = FindViewById<Button>(Resource.Id.btnExpandTrimOptions);
            mLlMinMaxYaw = FindViewById<LinearLayout>(Resource.Id.layoutMinMaxYaw);
			mTvMinYaw = FindViewById<TextView>(Resource.Id.tvMinYaw);
			mEtMinYaw = FindViewById<EditText>(Resource.Id.etMinYaw);
			mTvMaxYaw = FindViewById<TextView>(Resource.Id.tvMaxYaw);
			mEtMaxYaw = FindViewById<EditText>(Resource.Id.etMaxYaw);
            mLlMinMaxPitch = FindViewById<LinearLayout>(Resource.Id.layoutMinMaxPitch);
			mTvMinPitch = FindViewById<TextView>(Resource.Id.tvMinPitch);
			mEtMinPitch = FindViewById<EditText>(Resource.Id.etMinPitch);
			mTvMaxPitch = FindViewById<TextView>(Resource.Id.tvMaxPitch);
			mEtMaxPitch = FindViewById<EditText>(Resource.Id.etMaxPitch);
            mLlMinMaxRoll = FindViewById<LinearLayout>(Resource.Id.layoutMinMaxRoll);
			mTvMinRoll = FindViewById<TextView>(Resource.Id.tvMinRoll);
			mEtMinRoll = FindViewById<EditText>(Resource.Id.etMinRoll);
			mTvMaxRoll = FindViewById<TextView>(Resource.Id.tvMaxRoll);
			mEtMaxRoll = FindViewById<EditText>(Resource.Id.etMaxRoll);
			mBtStart = FindViewById<Button>(Resource.Id.btStart);
			mBtBackToMain = FindViewById<Button>(Resource.Id.btnSettingsBack);

            // Set font to widgets
            mTvHeader.Typeface = font;
            mRbMode1.Typeface = font;
            mRbMode2.Typeface = font;
            mBtLoggingOptions.Typeface = font;
            mBtExpandTrimOptions.Typeface = font;
            mBtStart.Typeface = font;
            mBtBackToMain.Typeface = font;

            // Handle events for all widgets
            mRbMode1.Click += OnMode1Click;
            mIvMode1.Click += OnMode1Click;

            mRbMode2.Click += OnMode2Click;
            mIvMode2.Click += OnMode2Click;

            mBtLoggingOptions.Click += OnShowLoggingOptions;
            mBtExpandTrimOptions.Click += OnExpandTrimOptions;

            mBtStart.Click += OnStartController;
            mBtBackToMain.Click += OnBackToMain;

			mSelectedBssid = Intent.GetStringExtra("mac");
            mPeerSettings = ReadPeerSettings();

			mEtMinYaw.Text = mMinYaw.ToString();
			mEtMaxYaw.Text = mMaxYaw.ToString();
			mEtMinPitch.Text = mMinPitch.ToString();
			mEtMaxPitch.Text = mMaxPitch.ToString();
			mEtMinRoll.Text = mMinRoll.ToString();
			mEtMaxRoll.Text = mMaxRoll.ToString();

            mSocketConnection.StartConnection();
        }

        /// <summary>
        /// Reads the settings file for a specific peer.
        /// If there is no settings file, default values are used.
        /// </summary>
        /// <returns>Peer with settings</returns>
        private Dictionary<string, ControllerSettings> ReadPeerSettings()
        {
            string fileName = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "settings" + Java.IO.File.Separator + "settings.csv";
            var reader = new Java.IO.BufferedReader(new Java.IO.FileReader(fileName));
            Dictionary<string, ControllerSettings> peerSettings = new Dictionary<string, ControllerSettings>();
            string line = "";
            bool isFirstTouch = true;
            while ((line = reader.ReadLine()) != null)
            {
                isFirstTouch = false;
                string[] parts = line.Split(',');
                string[] trimParts = parts[1].Split(';');
                try
                {
                    mLoggingActive = trimParts[3] == "True";
                    mLogBatteryActive = trimParts[4] == "True";
                    mLogRadarActive = trimParts[5] == "True";
                    mLogCollisionStatusActive = trimParts[6] == "True";
                    mLogControlsMobileActive = trimParts[7] == "True";
                    mLogControlsDroneActive = trimParts[8] == "True";
                    mLogDebug1Active = trimParts[9] == "True";
                    mLogDebug2Active = trimParts[10] == "True";
                    mLogDebug3Active = trimParts[11] == "True";
                    mLogDebug4Active = trimParts[12] == "True";
                    mMinYaw = Convert.ToInt32(trimParts[13] == "0" ? "-15" : trimParts[13]);
                    mMaxYaw = Convert.ToInt32(trimParts[14] == "0" ? "15" : trimParts[14]);
                    mMinPitch = Convert.ToInt32(trimParts[15] == "0" ? "-20" : trimParts[15]);
                    mMaxPitch = Convert.ToInt32(trimParts[16] == "0" ? "20" : trimParts[16]);
                    mMinRoll = Convert.ToInt32(trimParts[17] == "0" ? "-20" : trimParts[17]);
                    mMaxRoll = Convert.ToInt32(trimParts[18] == "0" ? "20" : trimParts[18]);
                }
                catch (IndexOutOfRangeException ex)
                {
                    mMinYaw = -15;
                    mMaxYaw = 15;
                    mMinPitch = -20;
                    mMaxPitch = 20;
                    mMinRoll = -20;
                    mMaxRoll = 20;
                }

                if(mLoggingActive == true)
                {
                    mLlLoggingOptions.Visibility = Android.Views.ViewStates.Visible;
                    mBtLoggingOptions.Text = "Logging turned on";
                    mCbxBattery.Checked = mLogBatteryActive;
                    mCbxRadarData.Checked = mLogRadarActive;
                    mCbxCollisionStatus.Checked = mLogCollisionStatusActive;
                    mCbxControlsMobile.Checked = mLogControlsMobileActive;
                    mCbxControlsDrone.Checked = mLogControlsDroneActive;
                    mCbxDebug1.Checked = mLogDebug1Active;
                    mCbxDebug2.Checked = mLogDebug2Active;
                    mCbxDebug3.Checked = mLogDebug3Active;
                    mCbxDebug4.Checked = mLogDebug4Active;
                }

                peerSettings.Add(parts[0], new ControllerSettings
                {
                    AltitudeControlActivated = false,
                    Inverted = false,
                    TrimYaw = Convert.ToInt16(trimParts[0]),
                    TrimPitch = Convert.ToInt16(trimParts[1]),
                    TrimRoll = Convert.ToInt16(trimParts[2]),
                    LoggingActivated = mLogControlsMobileActive,
                    MinYaw = mMinYaw,
                    MaxYaw = mMaxYaw,
                    MinPitch = mMinPitch,
                    MaxPitch = mMaxPitch,
                    MinRoll = mMinRoll,
                    MaxRoll = mMaxRoll
                });
            }
            if (isFirstTouch == true)
            {
                mMinYaw = -15;
                mMaxYaw = 15;
                mMinPitch = -20;
                mMaxPitch = 20;
                mMinRoll = -20;
                mMaxRoll = 20;
            }
            return peerSettings;
        }

        /// <summary>
        /// Handles OnClick event for Start button.
        /// Creates socket connection and opens ControllerView.
        /// Initializes, modifies and handles events for all widgets.
        /// Starts socket reader thread.
        /// </summary>
        private void OnStartController(object sender, EventArgs e)
        {
            // Create socket connection
            while(mSocketConnection.IsConnectingFinished == false)
            {
                Java.Lang.Thread.Sleep(100);
            }

            if(mSocketConnection.IsConnected == false)
            {
                StartActivity(typeof(MainActivity));
                return;
            }
            byte[] logInit = new byte[19];
            logInit[0] = INIT_COMMUNICATION;
            logInit[1] = (byte)(mLoggingActive == true ? 1 : 0);
            logInit[2] = (byte)(mLogBatteryActive == true ? 1 : 0);
            logInit[3] = (byte)(mLogRadarActive == true ? 1 : 0);
            logInit[4] = (byte)(mLogCollisionStatusActive == true ? 1 : 0);
            // For testing
            logInit[5] = (byte)(mLogControlsDroneActive == true ? 1 : 0);
            //
            logInit[6] = (byte)(mLogDebug1Active == true ? 1 : 0);
            logInit[7] = (byte)(mLogDebug2Active == true ? 1 : 0);
            logInit[8] = (byte)(mLogDebug3Active == true ? 1 : 0);
            logInit[9] = (byte)(mLogDebug4Active == true ? 1 : 0);

            mSocketConnection.WriteLog(logInit);

            // Start reading from Raspberry
            mSocketConnection.StartListening();

            // Read min and max values
            mMinYaw = Convert.ToInt32(mEtMinYaw.Text);
			mMaxYaw = Convert.ToInt32(mEtMaxYaw.Text);
			mMinPitch = Convert.ToInt32(mEtMinPitch.Text);
			mMaxPitch = Convert.ToInt32(mEtMaxPitch.Text);
			mMinRoll = Convert.ToInt32(mEtMinRoll.Text);
			mMaxRoll = Convert.ToInt32(mEtMaxRoll.Text);

            // Read log data
            mLogBatteryActive = mCbxBattery.Checked;
            if(mLogBatteryActive == true)
            {
                mSocketConnection.DroneLogs.Add("Battery", new LogData("Battery", 1));
            }
            mLogRadarActive = mCbxRadarData.Checked;
            if(mLogRadarActive == true)
            {
                mSocketConnection.DroneLogs.Add("Radar", new LogData("Radar", 1));
            }
            mLogCollisionStatusActive = mCbxCollisionStatus.Checked;
            if(mLogCollisionStatusActive == true)
            {
                mSocketConnection.DroneLogs.Add("CollisionStatus", new LogData("CollisionStatus", 1));
            }
            mLogControlsMobileActive = mCbxControlsMobile.Checked;

            // For testing
            mLogControlsDroneActive = mCbxControlsDrone.Checked;
            if (mLogControlsDroneActive == true)
            {
                mSocketConnection.DroneLogs.Add("ControlsDrone", new LogData("ControlsDrone", 1));
            }
            //
            mLogDebug1Active = mCbxDebug1.Checked;
            if(mLogDebug1Active == true)
            {
                mSocketConnection.DroneLogs.Add("Debug1", new LogData("Debug1", 1));
            }
            mLogDebug2Active = mCbxDebug2.Checked;
            if (mLogDebug2Active == true)
            {
                mSocketConnection.DroneLogs.Add("Debug2", new LogData("Debug2", 1));
            }
            mLogDebug3Active = mCbxDebug3.Checked;
            if (mLogDebug3Active == true)
            {
                mSocketConnection.DroneLogs.Add("Debug3", new LogData("Debug3", 1));
            }
            mLogDebug4Active = mCbxDebug4.Checked;
            if (mLogDebug4Active == true)
            {
                mSocketConnection.DroneLogs.Add("Debug4", new LogData("Debug4", 1));
            }

            // Change to Controller with joysticks
            SetContentView(Resource.Layout.ControllerLayout);

            // Set values for controller settings
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

            // Create font
            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            // Initialize widgets of ControllerLayout
            mControllerView = FindViewById<ControllerView>(Resource.Id.JoystickView);
            mSbTrimBar = FindViewById<SeekBar>(Resource.Id.sbTrimbar);
            mTvTrimValue = FindViewById<TextView>(Resource.Id.tvTrimValue);
            mBtnAltitudeControl = FindViewById<Button>(Resource.Id.btnAltitudeControl);
            mRbYawTrim = FindViewById<RadioButton>(Resource.Id.rbYawTrim);
            mRbPitchTrim = FindViewById<RadioButton>(Resource.Id.rbPitchTrim);
            mRbRollTrim = FindViewById<RadioButton>(Resource.Id.rbRollTrim);

            mControllerView.SetSocketConnection(mSocketConnection);

            // Set font to widgets
            mTvTrimValue.Typeface = font;
            mBtnAltitudeControl.Typeface = font;
            mRbYawTrim.Typeface = font;
            mRbPitchTrim.Typeface = font;
            mRbRollTrim.Typeface = font;

			mBtnAltitudeControl.Click += OnAltitudeControlClick;

			mSbTrimBar.Progress = ControllerView.Settings.TrimYaw - mMinTrim;
			mTvTrimValue.Text = ControllerView.Settings.TrimYaw.ToString();

            // Change value of trim
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

            // Change seekbar to yaw trim
			mRbYawTrim.Click += delegate
            {
                mTvTrimValue.Text = ControllerView.Settings.TrimYaw.ToString();
                mSbTrimBar.Progress = ControllerView.Settings.TrimYaw - mMinTrim;
            };

            // Change seekbar to pitch trim
            mRbPitchTrim.Click += delegate
            {
                mTvTrimValue.Text = ControllerView.Settings.TrimPitch.ToString();
                mSbTrimBar.Progress = ControllerView.Settings.TrimPitch - mMinTrim;
            };

            // Change seekbar to roll trim
            mRbRollTrim.Click += delegate
            {
                mTvTrimValue.Text = ControllerView.Settings.TrimRoll.ToString();
                mSbTrimBar.Progress = ControllerView.Settings.TrimRoll - mMinTrim;
            };
        }

		/// <summary>
		/// Writes log in csv format to mobile storage.
        /// Saves user options
		/// </summary>
		private void WriteLogData()
		{
			if (mSocketConnection.LogData != null)
			{
				RemoveFolder();
                string dirName = "";
                if (mLoggingActive)
                {
                    DateTime time = DateTime.Now;
				    dirName = string.Format("{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
				    var storageDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + dirName);
                    storageDir.Mkdirs();
				    var logWriter = new Java.IO.FileWriter(new Java.IO.File(storageDir, "controls.csv"));
					logWriter.Write(mSocketConnection.LogData);
                    logWriter.Close();
                    foreach(KeyValuePair<string, LogData> kvp in mSocketConnection.DroneLogs)
                    {
                        var writer = new Java.IO.FileWriter(new Java.IO.File(storageDir, kvp.Key + ".csv"));
                        writer.Write(kvp.Value.ToString());
                        writer.Close();
                    }
                }
                mPeerSettings[mSelectedBssid] = ControllerView.Settings;
				dirName = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "settings";
				string settingsString = "";
				foreach (KeyValuePair<string, ControllerSettings> kvp in mPeerSettings)
				{
					settingsString += kvp.Key + "," + kvp.Value.TrimYaw + ";" + kvp.Value.TrimPitch + ";" + kvp.Value.TrimRoll + ";"
                                         + mLoggingActive + ";" + mLogBatteryActive + ";" + mLogRadarActive + ";" + mLogCollisionStatusActive + ";"
                                         + mLogControlsMobileActive + ";" + mLogControlsDroneActive + ";" + mLogDebug1Active + ";"
                                         + mLogDebug2Active + ";" + mLogDebug3Active + ";" + mLogDebug4Active + ";" + mMinYaw + ";" + mMaxYaw + ";"
                                         + mMinPitch + ";" + mMaxPitch + ";" + mMinRoll + ";" + mMaxRoll + "\n"; ;
				}
				
				var settingsWriter = new Java.IO.FileWriter(new Java.IO.File(dirName, "settings.csv"));
				settingsWriter.Write(settingsString);
				settingsWriter.Close();
			}
		}

        /// <summary>
        /// Removes old files from application storage.
        /// </summary>
        private void RemoveFolder()
        {
            var root = new Java.IO.File(MainActivity.ApplicationFolderPath);
            List<string> fileNames = root.List().ToList();
            fileNames.Sort();
            if(fileNames.Count > 16)
            {
                var delFolder = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + fileNames[0]);
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
            if(ControllerView.Settings.AltitudeControlActivated)
            {
                ControllerView.Settings.AltitudeControlActivated = ControllerSettings.INACTIVE;
				mBtnAltitudeControl.SetBackgroundColor(Color.ParseColor("#005DA9"));
            }
            else 
            {
                ControllerView.Settings.AltitudeControlActivated = ControllerSettings.ACTIVE;
                mBtnAltitudeControl.SetBackgroundColor(Color.ParseColor("#E30034"));
                if (ControllerView.Settings.Inverted)
                {
                    mControllerView.UpdateOvals(mControllerView.mRightJS.CenterX, mControllerView.mRightJS.CenterY);
                }
                else
                {
                    mControllerView.UpdateOvals(mControllerView.mLeftJS.CenterX, mControllerView.mLeftJS.CenterY);
                }
                mControllerView.Invalidate();
            }
		}

		/// <summary>
		/// Saves log file and close connection when finished.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();

            if (mSocketConnection != null)
            {
                byte[] endframe = new byte[19];
                endframe[0] = 1;
                endframe[1] = END_COMMUNICATION;
                mSocketConnection.WriteLog(endframe);
            }

			WriteLogData();
            if (mSocketConnection != null)
            {
                mSocketConnection.Close();
            }
        }

		/// <summary>
		/// Saves log file and close connection when finished.
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop();

            if (mSocketConnection != null)
            {
                byte[] endframe = new byte[19];
                endframe[0] = 1;
                endframe[1] = END_COMMUNICATION;
                mSocketConnection.WriteLog(endframe);
            }

			WriteLogData();
            if (mSocketConnection != null)
            {
                mSocketConnection.Close();
            }
        }

        /// <summary>
        /// Goes to back to main activity.
        /// </summary>
        private void OnBackToMain(object sender, EventArgs e)
        {
            this.Finish();
        }

        /// <summary>
        /// Closes the socket connection and goes back to main activity.
        /// </summary>
		public void CloseOnRPIReset()
		{
			mSocketConnection.Close();
            //mSocketConnection.CloseReader();
			StartActivity(typeof(MainActivity));
		}

        private void OnExpandTrimOptions(object sender, EventArgs e)
        {
            if(mLlMinMaxPitch.Visibility == Android.Views.ViewStates.Gone)
            {
                mLlMinMaxPitch.Visibility = Android.Views.ViewStates.Visible;
                mLlMinMaxRoll.Visibility = Android.Views.ViewStates.Visible;
                mLlMinMaxYaw.Visibility = Android.Views.ViewStates.Visible;
                //mBtExpandTrimOptions.Text = "Trim Options v";
            }
            else
            {
                mLlMinMaxPitch.Visibility = Android.Views.ViewStates.Gone;
                mLlMinMaxRoll.Visibility = Android.Views.ViewStates.Gone;
                mLlMinMaxYaw.Visibility = Android.Views.ViewStates.Gone;
                //mBtExpandTrimOptions.Text = "Trim Options >";
            }
        }

        private void OnShowLoggingOptions(object sender, EventArgs e)
        {
            if(mLlLoggingOptions.Visibility == Android.Views.ViewStates.Gone)
            {
                mBtLoggingOptions.Text = "Logging turned on";
                mLlLoggingOptions.Visibility = Android.Views.ViewStates.Visible;
                mLoggingActive = true;
            }
            else
            {
                mBtLoggingOptions.Text = "Logging turned off";
                mLlLoggingOptions.Visibility = Android.Views.ViewStates.Gone;
                mLoggingActive = false;
            }
        }
    }
}