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
*  File: ControllerActivity.cs                                          *
*  Created on: 2017-07-19                                               *
*  Author(s): Guertl Sebastian Matthias (IFAT PMM TI COP)               *
*             Klapsch Adrian Vasile (IFAT PMM TI COP)                   *
*                                                                       *
*  ControllerActivity has two functionalities:                          *
*  1) Choose between controller mode before flight.                     *
*  2) Create ControllerView with Joysticks and settings.                *
*                                                                       *
************************************************************************/

using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace BTDronection
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
        private RadioGroup mRgControlMethod;
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
        private Button mBtBack;

        // Widgets controller
        private SeekBar mSbTrimBar;
        private TextView mTvTrimValue;
        private RadioButton mRbYawTrim;
        private RadioButton mRbPitchTrim;
        private RadioButton mRbRollTrim;
        private Button mBtnAltitudeControl;

        // Private members
        private Dictionary<string, ControllerSettings> mPeerSettings;
        private string mSelectedMac;

		// Socket members
        private SocketConnection mSocketConnection;

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

			// Initialize widgets
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderSettings);
            mRgControlMethod = FindViewById<RadioGroup>(Resource.Id.rgControlMode);
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
            mBtBack = FindViewById<Button>(Resource.Id.btnSettingsBack);

			// Create font
			var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            mTvHeader.Typeface = font;
            mRbMode1.Typeface = font;
            mRbMode2.Typeface = font;
            mCbxLoggingActive.Typeface = font;
            mTvMinYaw.Typeface = font;
            mEtMinYaw.Typeface = font;
			mTvMaxYaw.Typeface = font;
			mEtMaxYaw.Typeface = font;
            mTvMinPitch.Typeface = font;
            mEtMinPitch.Typeface = font;
            mTvMaxPitch.Typeface = font;
            mEtMaxPitch.Typeface = font;
            mTvMinRoll.Typeface = font;
            mEtMinRoll.Typeface = font;
            mTvMaxRoll.Typeface = font;
            mEtMaxRoll.Typeface = font;
            mBtStart.Typeface = font;
            mBtBack.Typeface = font;

            mRbMode1.Click += OnMode1Click;
            mIvMode1.Click += OnMode1Click;

            mRbMode2.Click += OnMode2Click;
            mIvMode2.Click += OnMode2Click;

            mBtStart.Click += OnStartController;

            mBtnAltitudeControl.Click += OnAltitudeControlClick;

			// Get singleton instance of socket connection
            mSocketConnection = SocketConnection.Instance;

            mSelectedMac = Intent.GetStringExtra("mac");
            mPeerSettings = ReadPeerSettings();
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
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                string[] trimParts = parts[1].Split(';');
                peerSettings.Add(parts[0], new ControllerSettings
                {
                    AltitudeControlActivated = false,
                    LoggingActivated = mCbxLoggingActive.Checked,
                    Inverted = false,
                    MinYaw = Convert.ToInt32(mEtMinYaw.Text),
                    MaxYaw = Convert.ToInt32(mEtMaxYaw.Text),
                    MinPitch = Convert.ToInt32(mEtMinPitch.Text),
                    MaxPitch = Convert.ToInt32(mEtMaxPitch.Text),
                    MinRoll = Convert.ToInt32(mEtMinRoll.Text),
                    MaxRoll = Convert.ToInt32(mEtMaxRoll.Text),
                    TrimYaw = Convert.ToInt16(trimParts[0]),
                    TrimPitch = Convert.ToInt16(trimParts[1]),
                    TrimRoll = Convert.ToInt16(trimParts[2])
                });
            }
            return peerSettings;
        }

        /// <summary>
        /// Writes log in csv format.
        /// </summary>
        public void WriteLogData()
        {
            if(mSocketConnection.LogData != null)
            {
                DateTime time = DateTime.Now;
                string logName = string.Format("{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                var storageDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + logName);
                storageDir.Mkdirs();
                var writer = new Java.IO.FileWriter(new Java.IO.File(storageDir, "controls.csv"));
                writer.Write(mSocketConnection.LogData);
                mPeerSettings[mSelectedMac] = ControllerView.Settings;
                string dirName = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "settings";
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
        /// Starts the controller.
        /// </summary>
        private void OnStartController(object sender, EventArgs e)
        {
            SetContentView(Resource.Layout.ControllerLayout);

            if (mPeerSettings.Any(kvp => kvp.Key == mSelectedMac) == true)
            {
                ControllerView.Settings.TrimYaw = mPeerSettings[mSelectedMac].TrimYaw;
                ControllerView.Settings.TrimPitch = mPeerSettings[mSelectedMac].TrimPitch;
                ControllerView.Settings.TrimRoll = mPeerSettings[mSelectedMac].TrimRoll;
            }

            // Initialize widgets
            mSbTrimBar = FindViewById<SeekBar>(Resource.Id.sbTrimbar);
            mTvTrimValue = FindViewById<TextView>(Resource.Id.tvTrimValue);
            mRbYawTrim = FindViewById<RadioButton>(Resource.Id.rbYawTrim);
            mRbPitchTrim = FindViewById<RadioButton>(Resource.Id.rbPitchTrim);
            mRbRollTrim = FindViewById<RadioButton>(Resource.Id.rbRollTrim);

            // Create and set font
            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
            mTvTrimValue.Typeface = font;
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
			if (ControllerView.Settings.AltitudeControlActivated)
			{
				ControllerView.Settings.AltitudeControlActivated = ControllerSettings.INACTIVE;
				mBtnAltitudeControl.SetBackgroundColor(Color.ParseColor("#005DA9"));
			}
			else
			{
				ControllerView.Settings.AltitudeControlActivated = ControllerSettings.ACTIVE;
				mBtnAltitudeControl.SetBackgroundColor(Color.ParseColor("#E30034"));
			}
		}

		/// <summary>
		/// Saves log file and close connection when finished.
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();
			WriteLogData();
			mSocketConnection.Cancel();
		}

		/// <summary>
		/// Saves log file and close connection when finished.
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop();
			WriteLogData();
			mSocketConnection.Cancel();
		}
    }
}
