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
        private TextView mTvHeader;
        private RadioGroup mRgControlMethod;
        private RadioButton mRbThrottleLeft;
        private RadioButton mRbThrottleRight;
        private TextView mTvDescription;
        private Button mBtStart;
        private Button mBtShowLog;

        private SeekBar mSbTrimBar;
        private TextView mTvTrimValue;
        private RadioButton mRbYawTrim;
        private RadioButton mRbPitchTrim;
        private RadioButton mRbRollTrim;

        //private IntentFilter m_Filter; // Used to filter events when searching
        //private CallReciver m_Receiver;

        public static bool Inverted;
        private int mYawTrim;
        private readonly int mMinTrim = -50;

        private readonly String TEXT_LEFT = "The left joystick will be used to regulate throttle and yaw. The right joystick will be used to regulate pitch and roll.";
        private readonly String TEXT_RIGHT = "The left joystick will be used to regulate pitch and yaw. The right joystick will be used to regulate the throttle and roll.";

        private string mStorageDirPath;

        private SocketConnection mSocketConnection;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ControllerSettings);

            mSocketConnection = SocketConnection.Instance;

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeaderSettings);
            mRgControlMethod = FindViewById<RadioGroup>(Resource.Id.rgControlMethod);
            mRbThrottleLeft = FindViewById<RadioButton>(Resource.Id.rbThrottleLeft);
            mRbThrottleRight = FindViewById<RadioButton>(Resource.Id.rbThrottleRight);
            mTvDescription = FindViewById<TextView>(Resource.Id.tvDescription);
            mBtStart = FindViewById<Button>(Resource.Id.btStart);
            mBtShowLog = FindViewById<Button>(Resource.Id.btShowLog);

            mTvHeader.Typeface = font;
            mRbThrottleLeft.Typeface = font;
            mRbThrottleRight.Typeface = font;
            mTvDescription.Typeface = font;
            mBtStart.Typeface = font;
            mBtShowLog.Typeface = font;

            mRbThrottleLeft.Click += OnThrottleLeftClick;
            mRbThrottleRight.Click += OnThrottleRightClick;

            mBtStart.Click += OnStartController;

            //m_Filter = new IntentFilter();

            // m_Receiver = new CallReciver();

            // m_Filter.AddAction("android.intent.action.PHONE_STATE");
            // m_Filter.AddAction("INCOMING_CALL");
            // m_Filter.AddAction(SipSession.State.IncomingCall.ToString());
            // Registering events and forwarding them to the broadcast object
            // RegisterReceiver(m_Receiver, m_Filter);

            /*mStorageDirPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "Airything");
            var storageDir = new Java.IO.File(mStorageDirPath);
            storageDir.Mkdirs();*/
        }

        private void OnStartController(object sender, EventArgs e)
        {
            mSocketConnection.Start();
            SetContentView(Resource.Layout.ControllerLayout);

            mSbTrimBar = FindViewById<SeekBar>(Resource.Id.sbTrimbar);
            mTvTrimValue = FindViewById<TextView>(Resource.Id.tvTrimValue);
            mRbYawTrim = FindViewById<RadioButton>(Resource.Id.rbYawTrim);
            mRbPitchTrim = FindViewById<RadioButton>(Resource.Id.rbPitchTrim);
            mRbRollTrim = FindViewById<RadioButton>(Resource.Id.rbRollTrim);

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mSocketConnection.OnCancel();
        }

        protected override void OnStop()
        {
            base.OnStop();
            mSocketConnection.OnCancel();
        }

        private void OnThrottleRightClick(object sender, EventArgs e)
        {
            Inverted = ControllerSettings.ACTIVE;
            mTvDescription.Text = TEXT_RIGHT;
        }

        private void OnThrottleLeftClick(object sender, EventArgs e)
        {
            Inverted = ControllerSettings.INACTIVE;
            mTvDescription.Text = TEXT_LEFT;
        }

        private void OnRgClick(object sender, EventArgs e)
        {
            if (mRbThrottleLeft.Selected)
            {
                Inverted = ControllerSettings.INACTIVE;
                mTvDescription.Text = TEXT_LEFT;

            }
            if (mRbThrottleRight.Selected)
            {
                Inverted = ControllerSettings.ACTIVE;
                mTvDescription.Text = TEXT_RIGHT;
            }
        }
    }
}