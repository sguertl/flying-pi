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

namespace WiFiDronection
{
    [Activity(Label = "ControllerActivity",
              Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen",
              MainLauncher = false,
              ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorLandscape
             )]
    public class ControllerActivity : Activity
    {
        private RadioGroup m_RgControlMethod;
        private RadioButton m_RbThrottleLeft;
        private RadioButton m_RbThrottleRight;
        private TextView m_TvDescription;
        private Button m_BtStart;
        private Button m_BtShowLog;

        private SeekBar mSbTrimBar;
        private TextView mTvTrimValue;
        private RadioButton mRbYawTrim;
        private RadioButton mRbPitchTrim;
        private RadioButton mRbRollTrim;

        //private IntentFilter m_Filter; // Used to filter events when searching
        //private CallReciver m_Receiver;

        private bool m_Inverted;
        private int m_YawTrim;

        private readonly String TEXT_LEFT = "The left joystick will be used to regulate throttle and rudder. The right joystick will be used to regulate elevator and aileron.";
        private readonly String TEXT_RIGHT = "The left joystick will be used to regulate elevator and rudder. The right joystick will be used to regulate the throttle and aileron.";

        private string mStorageDirPath;

        private SocketConnection mSocketConnection;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ControllerSettings);

            mSocketConnection = SocketConnection.Instance;

            m_RgControlMethod = FindViewById<RadioGroup>(Resource.Id.rgControlMethod);
            m_RbThrottleLeft = FindViewById<RadioButton>(Resource.Id.rbThrottleLeft);
            m_RbThrottleRight = FindViewById<RadioButton>(Resource.Id.rbThrottleRight);
            m_TvDescription = FindViewById<TextView>(Resource.Id.tvDescription);
            m_BtStart = FindViewById<Button>(Resource.Id.btStart);
            m_BtShowLog = FindViewById<Button>(Resource.Id.btShowLog);

            m_RbThrottleLeft.Click += OnThrottleLeftClick;
            m_RbThrottleRight.Click += OnThrottleRightClick;

            m_BtStart.SetBackgroundColor(Android.Graphics.Color.DeepSkyBlue);
            m_BtStart.SetTextColor(Android.Graphics.Color.White);
            m_BtShowLog.SetBackgroundColor(Android.Graphics.Color.DeepSkyBlue);
            m_BtShowLog.SetTextColor(Android.Graphics.Color.White);

            m_BtStart.Click += OnStartController;

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
            SetContentView(new ControllerView(this, mSocketConnection));
            View.Inflate(this, Resource.Layout.ControllerLayout, null);
            Make();
        }

        private void Make()
        {
            mSbTrimBar = FindViewById<SeekBar>(Resource.Id.sbTrimbar);
            mTvTrimValue = FindViewById<TextView>(Resource.Id.tvTrimValue);
            mRbYawTrim = FindViewById<RadioButton>(Resource.Id.rbYawTrim);
            mRbPitchTrim = FindViewById<RadioButton>(Resource.Id.rbPitchTrim);
            mRbRollTrim = FindViewById<RadioButton>(Resource.Id.rbRollTrim);

            mSbTrimBar.ProgressChanged += delegate
            {
                if (mRbYawTrim.Checked == true)
                {
                    ControllerView.Settings.TrimYaw = mSbTrimBar.Progress;
                }
                else if (mRbPitchTrim.Checked == true)
                {
                    ControllerView.Settings.TrimPitch = mSbTrimBar.Progress;
                }
                else
                {
                    ControllerView.Settings.TrimRoll = mSbTrimBar.Progress;
                }
                mTvTrimValue.Text = mSbTrimBar.Progress.ToString();
            };

            mRbYawTrim.Click += delegate
            {
                mTvTrimValue.Text = ControllerView.Settings.TrimYaw.ToString();
                mSbTrimBar.Progress = ControllerView.Settings.TrimYaw;
            };

            mRbPitchTrim.Click += delegate
            {
                mTvTrimValue.Text = ControllerView.Settings.TrimPitch.ToString();
                mSbTrimBar.Progress = ControllerView.Settings.TrimPitch;
            };

            mRbRollTrim.Click += delegate
            {
                mTvTrimValue.Text = ControllerView.Settings.TrimRoll.ToString();
                mSbTrimBar.Progress = ControllerView.Settings.TrimRoll;
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mSocketConnection.onCancel();
        }

        protected override void OnStop()
        {
            base.OnStop();
            mSocketConnection.onCancel();
        }

        private void OnThrottleRightClick(object sender, EventArgs e)
        {
            m_Inverted = ControllerSettings.ACTIVE;
            m_TvDescription.Text = TEXT_RIGHT;
        }

        private void OnThrottleLeftClick(object sender, EventArgs e)
        {
            m_Inverted = ControllerSettings.INACTIVE;
            m_TvDescription.Text = TEXT_LEFT;
        }

        private void OnRgClick(object sender, EventArgs e)
        {
            if (m_RbThrottleLeft.Selected)
            {
                m_Inverted = ControllerSettings.INACTIVE;
                m_TvDescription.Text = TEXT_LEFT;

            }
            if (m_RbThrottleRight.Selected)
            {
                m_Inverted = ControllerSettings.ACTIVE;
                m_TvDescription.Text = TEXT_RIGHT;
            }
        }
    }
}