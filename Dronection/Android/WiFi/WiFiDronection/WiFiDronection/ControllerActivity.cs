using System;
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
        private RadioGroup mRgControlMethod;
        private RadioButton mRbThrottleLeft;
        private RadioButton mRbThrottleRight;
        private Button mBtStart;
        private Button mBtBackToMain;
        private ImageView mIvMode;

        private SeekBar mSbTrimBar;
        private TextView mTvTrimValue;
        private RadioButton mRbYawTrim;
        private RadioButton mRbPitchTrim;
        private RadioButton mRbRollTrim;

        private int mYawTrim;
        private bool mIsConnected;
        private SocketConnection mSocketConnection;
        private SocketReader mSocketReader;

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
            mRgControlMethod = FindViewById<RadioGroup>(Resource.Id.rgControlMethod);
            mRbThrottleLeft = FindViewById<RadioButton>(Resource.Id.rbThrottleLeft);
            mRbThrottleRight = FindViewById<RadioButton>(Resource.Id.rbThrottleRight);
            mBtStart = FindViewById<Button>(Resource.Id.btStart);
            mBtBackToMain = FindViewById<Button>(Resource.Id.btBackToMain);
            mIvMode = FindViewById<ImageView>(Resource.Id.ivMode);

            mTvHeader.Typeface = font;
            mRbThrottleLeft.Typeface = font;
            mRbThrottleRight.Typeface = font;
            mBtStart.Typeface = font;
            mBtBackToMain.Typeface = font;

            mRbThrottleLeft.Click += OnThrottleLeftClick;
            mRbThrottleRight.Click += OnThrottleRightClick;

            mBtStart.Click += OnStartController;
            mBtBackToMain.Click += OnBackToMain;

            mIsConnected = Intent.GetBooleanExtra("isConnected", true);
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

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            // Initialize wigets of ControllerLayout
            mSbTrimBar = FindViewById<SeekBar>(Resource.Id.sbTrimbar);
            mTvTrimValue = FindViewById<TextView>(Resource.Id.tvTrimValue);
            mRbYawTrim = FindViewById<RadioButton>(Resource.Id.rbYawTrim);
            mRbPitchTrim = FindViewById<RadioButton>(Resource.Id.rbPitchTrim);
            mRbRollTrim = FindViewById<RadioButton>(Resource.Id.rbRollTrim);

            mTvTrimValue.Typeface = font;
            mRbYawTrim.Typeface = font;
            mRbPitchTrim.Typeface = font;
            mRbRollTrim.Typeface = font;

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
                var writer = new Java.IO.FileWriter(new Java.IO.File(storageDir, "Controlls.csv"));
                writer.Write(mSocketConnection.LogData);
                writer.Close();
            }
        }

        /// <summary>
        /// Change control mode
        /// </summary>
        private void OnThrottleRightClick(object sender, EventArgs e)
        {
            Inverted = ControllerSettings.ACTIVE;
            mIvMode.SetImageResource(Resource.Drawable.mode2);
        }

        /// <summary>
        /// Change control mode
        /// </summary>
        private void OnThrottleLeftClick(object sender, EventArgs e)
        {
            Inverted = ControllerSettings.INACTIVE;
            mIvMode.SetImageResource(Resource.Drawable.mode1);
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