﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Java.Lang;

namespace WiFiDronection
{
    [Activity(Label = "ControllerActivity",
              Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
              MainLauncher = false,
              ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorLandscape
             )]
    public class ControllerActivity : Activity
    {
        // Members
        private TextView mTvHeader;
        private RadioGroup mRgControlMode;
        private RadioButton mRbMode1;
        private RadioButton mRbMode2;
        private Button mBtStart;
        private Button mBtBackToMain;
        private ImageView mIvMode1;
        private ImageView mIvMode2;

        private SeekBar mSbTrimBar;
        private TextView mTvTrimValue;
        private Button mBtnAltitudeControl;
        private RadioButton mRbYawTrim;
        private RadioButton mRbPitchTrim;
        private RadioButton mRbRollTrim;

        private bool mIsConnected;
        private SocketConnection mSocketConnection;
        private SocketReader mSocketReader;
        private string mSelectedBssid;
        private Dictionary<string, ControllerSettings> mPeerSettings;

        // Constants
        private readonly int mMinTrim = -30;

        // Public variables
        public static bool Inverted;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ControllerSettings);

            // Get singleton instance of socketconnection
            mSocketConnection = SocketConnection.Instance;

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
            // Initialize widgets
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderSettings);
            mRgControlMode = FindViewById<RadioGroup>(Resource.Id.rgControlMode);
            mRbMode1 = FindViewById<RadioButton>(Resource.Id.rbMode1);
            mRbMode2 = FindViewById<RadioButton>(Resource.Id.rbMode2);
            mBtStart = FindViewById<Button>(Resource.Id.btStart);
            mBtBackToMain = FindViewById<Button>(Resource.Id.btnSettingsBack);
            mIvMode1 = FindViewById<ImageView>(Resource.Id.ivMode1);
            mIvMode2 = FindViewById<ImageView>(Resource.Id.ivMode2);

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

            mIsConnected = Intent.GetBooleanExtra("isConnected", true);
            mSelectedBssid = Intent.GetStringExtra("mac");
            mPeerSettings = ReadPeerSettings();
        }

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
                peerSettings.Add(parts[0], new ControllerSettings
                {
                    AltitudeControlActivated = false,
                    Inverted = false,
                    TrimYaw = Convert.ToInt16(trimParts[0]),
                    TrimPitch = Convert.ToInt16(trimParts[1]),
                    TrimRoll = Convert.ToInt16(trimParts[2])
                });
            }
            return peerSettings;
        }

        /// <summary>
        /// Onclick event for start button
        /// Creates socket connection and opens controllerview
        /// Start socket reader thread
        /// </summary>
        private void OnStartController(object sender, EventArgs e)
        {
            // Create sokcet connection
            if (mSocketConnection.IsSocketConnected == false)
            {
                mSocketConnection.Start();
            }
            mSocketConnection.isConnected = true;
            // Change GUI to Controller layout with joysticks
            SetContentView(Resource.Layout.ControllerLayout);

            if(mPeerSettings.Any(kvp => kvp.Key == mSelectedBssid) == true)
            {
                ControllerView.Settings.TrimYaw = mPeerSettings[mSelectedBssid].TrimYaw;
                ControllerView.Settings.TrimPitch = mPeerSettings[mSelectedBssid].TrimPitch;
                ControllerView.Settings.TrimRoll = mPeerSettings[mSelectedBssid].TrimRoll;
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
            if(mIsConnected == false)
            {
                mSocketReader = new SocketReader(mSocketConnection.InputStream);
				mSocketReader.Start();
            }
        }

        /// <summary>
        /// Save Log when finished
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            WriteLogData();
            mSocketConnection.isConnected = false;
            mSocketConnection.Interrupt();
            /*mSocketConnection.OnCancel();
            mSocketReader.Close();*/
        }

        /// <summary>
        /// Save Log when finished
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            WriteLogData();
            mSocketConnection.isConnected = false;
            mSocketConnection.Interrupt();
            /*mSocketConnection.OnCancel();
            mSocketReader.Close();*/
        }

        /// <summary>
        /// Write log in csv format
        /// </summary>
        private void WriteLogData()
        {
            if(mSocketConnection.LogData != null)
            {
                DateTime time = DateTime.Now;
                string dirName = string.Format("{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                var storageDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + dirName);
                storageDir.Mkdirs();
                var writer = new Java.IO.FileWriter(new Java.IO.File(storageDir, "controls.csv"));
                writer.Write(mSocketConnection.LogData);
                mPeerSettings[mSelectedBssid] = ControllerView.Settings;
                dirName = MainActivity.ApplicationFolderPath + Java.IO.File.Separator + "settings";
                string settingsString = "";
                foreach(KeyValuePair<string, ControllerSettings> kvp in mPeerSettings)
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
		/// Change control mode
		/// </summary>
		private void OnMode1Click(object sender, EventArgs e)
		{
			Inverted = ControllerSettings.INACTIVE;
            mRbMode1.Checked = true;
		}

        /// <summary>
        /// Change control mode
        /// </summary>
        private void OnMode2Click(object sender, EventArgs e)
        {
            Inverted = ControllerSettings.ACTIVE;
            mRbMode2.Checked = true;
        }

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
            }
		}

        /// <summary>
        /// Go to back to main activity
        /// </summary>
        private void OnBackToMain(object sender, EventArgs e)
        {
            this.Finish();
        }
    }
}