using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Telephony;
using Android.Runtime;
using Android.Net.Sip;
using Android;
using System.IO;
using Android.Graphics;

namespace BTDronection
{
    [Activity(Label = "ControllerActivity",
              Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
              MainLauncher = false,
              ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorLandscape
             )]
    public class ControllerActivity : Activity
    {
        private TextView mTvHeader;
        private RadioGroup mRgControlMethod;
        private RadioButton mRbThrottleLeft;
        private RadioButton mRbThrottleRight;
        private ImageView mIvMode;
        private Button mBtStart;
        private Button mBtShowLog;

        private SeekBar mSbTrimBar;
        private TextView mTvTrimValue;
        private RadioButton mRbYawTrim;
        private RadioButton mRbPitchTrim;
        private RadioButton mRbRollTrim;

        //private IntentFilter mFilter; // Used to filter events when searching
        //private CallReciver mReceiver;

        public static bool mInverted;
        private int mYawTrim;
        private readonly int mMinTrim = -30;

        private string mStorageDirPath;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ControllerSettings);
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderSettings);
            mRgControlMethod = FindViewById<RadioGroup>(Resource.Id.rgControlMethod);
            mRbThrottleLeft = FindViewById<RadioButton>(Resource.Id.rbThrottleLeft);
            mRbThrottleRight = FindViewById<RadioButton>(Resource.Id.rbThrottleRight);
            mIvMode = FindViewById<ImageView>(Resource.Id.ivMode);
            mBtStart = FindViewById<Button>(Resource.Id.btStart);
            mBtShowLog = FindViewById<Button>(Resource.Id.btShowLog);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            mTvHeader.Typeface = font;
            mRbThrottleLeft.Typeface = font;
            mRbThrottleRight.Typeface = font;
            mBtStart.Typeface = font;
            mBtShowLog.Typeface = font;

            mRbThrottleLeft.Click += OnThrottleLeftClick;
            mRbThrottleRight.Click += OnThrottleRightClick;

            mBtStart.Click += OnStartController;

            //mFilter = new IntentFilter();

            // mReceiver = new CallReciver();

            // mFilter.AddAction("android.intent.action.PHONE_STATE");
            // mFilter.AddAction("INCOMING_CALL");
            // mFilter.AddAction(SipSession.State.IncomingCall.ToString());
            // Registering events and forwarding them to the broadcast object
            // RegisterReceiver(mReceiver, mFilter);

            mStorageDirPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "Airything");
            var storageDir = new Java.IO.File(mStorageDirPath);
            storageDir.Mkdirs();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DateTime time = DateTime.Now;
            string logName = string.Format("{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}_log", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
            var storageDir = new Java.IO.File(MainActivity.ApplicationFolderPath + Java.IO.File.Separator + logName);
            storageDir.Mkdirs();
            var writer = new Java.IO.FileWriter(new Java.IO.File(storageDir, "Controlls.csv"));
            writer.Write(DataTransfer.DEBUG);
            writer.Close();
            ConnectedThread.Cancel();
            writer.Close();
        }

        private void OnStartController(object sender, EventArgs e)
        {
            SetContentView(Resource.Layout.ControllerLayout);

            mSbTrimBar = FindViewById<SeekBar>(Resource.Id.sbTrimbar);
            mTvTrimValue = FindViewById<TextView>(Resource.Id.tvTrimValue);
            mRbYawTrim = FindViewById<RadioButton>(Resource.Id.rbYawTrim);
            mRbPitchTrim = FindViewById<RadioButton>(Resource.Id.rbPitchTrim);
            mRbRollTrim = FindViewById<RadioButton>(Resource.Id.rbRollTrim);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
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
        }

        private void OnThrottleRightClick(object sender, EventArgs e)
        {
            mInverted = ControllerSettings.ACTIVE;
            mIvMode.SetImageResource(Resource.Drawable.mode2);
        }

        private void OnThrottleLeftClick(object sender, EventArgs e)
        {
            mInverted = ControllerSettings.INACTIVE;
            mIvMode.SetImageResource(Resource.Drawable.mode1);
        }

        private void OnRgClick(object sender, EventArgs e)
        {
            if (mRbThrottleLeft.Selected)
            {
                mInverted = ControllerSettings.INACTIVE;

            }
            if (mRbThrottleRight.Selected)
            {
                mInverted = ControllerSettings.ACTIVE;
            }
        }
    }
}
