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
        private bool mLoggingActive;
        private int mMinYaw;
        private int mMaxYaw;
        private int mMinPitch;
        private int mMaxPitch;
        private int mMinRoll;
        private int mMaxRoll;
        private Flight mFlight;

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
           
			mCbxLoggingActive.Click += (sender, e) => mLoggingActive = mCbxLoggingActive.Checked;

			// Get singleton instance of socket connection
			mSocketConnection = SocketConnection.Instance;

            mSelectedMac = Intent.GetStringExtra("mac");
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
        /// Reads the settings file for a specific peer.
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
                    mLoggingActive = trimParts[3].Equals("true");
                    mMinYaw = Convert.ToInt32(trimParts[4] == "0" ? "-15" : trimParts[4]);
                    mMaxYaw = Convert.ToInt32(trimParts[5] == "0" ? "15" : trimParts[5]);
                    mMinPitch = Convert.ToInt32(trimParts[6] == "0" ? "-20" : trimParts[6]);
                    mMaxPitch = Convert.ToInt32(trimParts[7] == "0" ? "20" : trimParts[7]);
                    mMinRoll = Convert.ToInt32(trimParts[8] == "0" ? "-20" : trimParts[8]);
                    mMaxRoll = Convert.ToInt32(trimParts[9] == "0" ? "20" : trimParts[9]);
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
        /// Writes log in csv format.
        /// </summary>
        public void WriteLogData()
        {
            if(mSocketConnection.LogData != null)
            {
                RemoveFolder();
                string dirName = "";
                if (mLoggingActive)
                {
                    DateTime time = DateTime.Now;
                    dirName = string.Format("{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                    var storageDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + dirName);
                    var logWriter = new Java.IO.FileWriter(new Java.IO.File(storageDir, "controls.csv"));
                    storageDir.Mkdirs();
                    logWriter.Write(mSocketConnection.LogData);
                    logWriter.Close();
                }

                mPeerSettings[mSelectedMac] = ControllerView.Settings;
                dirName = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "settings";
                string settingsString = "";
                foreach (KeyValuePair<string, ControllerSettings> kvp in mPeerSettings)
                {
                    settingsString += kvp.Key + "," + kvp.Value.TrimYaw + ";" + kvp.Value.TrimPitch + ";" + kvp.Value.TrimRoll + ";" 
                                         + (mLoggingActive ? "true" : "false")  + ";" + mMinYaw + ";" + mMaxYaw + ";" + mMinPitch + ";" 
                                         + mMaxPitch + ";" + mMinRoll + ";" + mMaxRoll + "\n";
                }

                var settingsWriter = new Java.IO.FileWriter(new Java.IO.File(dirName, "settings.csv"));
                settingsWriter.Write(settingsString);
                settingsWriter.Close();
            }
        }

        /// <summary>
        /// Starts the controller.
        /// </summary>
        private void OnStartController(object sender, EventArgs e)
        {
            mFlight = Flight.Instance;

            mMinYaw = Convert.ToInt32(mEtMinYaw.Text);
			mMaxYaw = Convert.ToInt32(mEtMaxYaw.Text);

			mMinPitch = Convert.ToInt32(mEtMinPitch.Text);
			mMaxPitch = Convert.ToInt32(mEtMaxPitch.Text);

			mMinRoll = Convert.ToInt32(mEtMinRoll.Text);
			mMaxRoll = Convert.ToInt32(mEtMaxRoll.Text);

			SetContentView(Resource.Layout.ControllerLayout);

            if (mPeerSettings.Any(kvp => kvp.Key == mSelectedMac) == true)
            {
                ControllerView.Settings.TrimYaw = mPeerSettings[mSelectedMac].TrimYaw;
                ControllerView.Settings.TrimPitch = mPeerSettings[mSelectedMac].TrimPitch;
                ControllerView.Settings.TrimRoll = mPeerSettings[mSelectedMac].TrimRoll;
                ControllerView.Settings.LoggingActivated = mLoggingActive;
                ControllerView.Settings.MinYaw = mMinYaw;
                ControllerView.Settings.MaxYaw = mMaxYaw;
                ControllerView.Settings.MinPitch = mMinPitch;
                ControllerView.Settings.MaxPitch = mMaxPitch;
                ControllerView.Settings.MinRoll = mMinRoll;
                ControllerView.Settings.MaxRoll = mMaxRoll;
            }

            // Initialize widgets
            mSbTrimBar = FindViewById<SeekBar>(Resource.Id.sbTrimbar);
            mTvTrimValue = FindViewById<TextView>(Resource.Id.tvTrimValue);
            mRbYawTrim = FindViewById<RadioButton>(Resource.Id.rbYawTrim);
            mRbPitchTrim = FindViewById<RadioButton>(Resource.Id.rbPitchTrim);
            mRbRollTrim = FindViewById<RadioButton>(Resource.Id.rbRollTrim);
            mBtnAltitudeControl = FindViewById<Button>(Resource.Id.btnAltitudeControl);

            // Create and set font
            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
            mTvTrimValue.Typeface = font;
            mRbYawTrim.Typeface = font;
            mRbPitchTrim.Typeface = font;
            mRbRollTrim.Typeface = font;
            mBtnAltitudeControl.Typeface = font;

            mSbTrimBar.Progress = ControllerView.Settings.TrimYaw - mMinTrim;
            mTvTrimValue.Text = ControllerView.Settings.TrimYaw.ToString();

            mBtnAltitudeControl.Click += OnAltitudeControlClick;

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
		/// Removes old files.
		/// </summary>
		private void RemoveFolder()
		{
			var root = new Java.IO.File (MainActivity.ApplicationFolderPath);
			List<string> fileNames = root.List().ToList();
			if(fileNames.Count > 16) 
			{
				var delFolder = new Java.IO.File (MainActivity.ApplicationFolderPath + Java.IO.File.Separator + fileNames [fileNames.Count - 2]);
				foreach (string delChild in delFolder.List()) 
				{
					new Java.IO.File (delFolder.AbsolutePath, delChild).Delete();
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
            if (ControllerView.Settings.AltitudeControlActivated)
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
                    mFlight.CV.UpdateOvals(mFlight.CV.mRightJS.CenterX, mFlight.CV.mRightJS.CenterY);
                }
                else
                {
                    mFlight.CV.UpdateOvals(mFlight.CV.mLeftJS.CenterX, mFlight.CV.mLeftJS.CenterY);
                }

                mFlight.CV.Invalidate();
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

            if(mFlight.CV.WriteTimer != null)
            {
                mFlight.CV.WriteTimer.Close();
                mFlight.CV.WriteTimer = null;
            }
		}

		/// <summary>
		/// Saves log file and close connection when finished.
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop();
			WriteLogData();
			mSocketConnection.Cancel();

            if (mFlight.CV.WriteTimer != null)
            {
                mFlight.CV.WriteTimer.Close();
                mFlight.CV.WriteTimer = null;
            }
        }
    }
}
