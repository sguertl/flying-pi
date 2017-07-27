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
        private Button mBtStart;
        private Button mBtBackToMain;
        private ImageView mIvMode;

        private SeekBar mSbTrimBar;
        private TextView mTvTrimValue;
        private RadioButton mRbYawTrim;
        private RadioButton mRbPitchTrim;
        private RadioButton mRbRollTrim;

        //private IntentFilter m_Filter; // Used to filter events when searching
        //private CallReciver m_Receiver;

        public static bool Inverted;
        private int mYawTrim;
        private readonly int mMinTrim = -30;
        private bool mIsConnected;

        private SocketConnection mSocketConnection;
        private SocketReader mSocketReader;

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

            //m_Filter = new IntentFilter();

            // m_Receiver = new CallReciver();

            // m_Filter.AddAction("android.intent.action.PHONE_STATE");
            // m_Filter.AddAction("INCOMING_CALL");
            // m_Filter.AddAction(SipSession.State.IncomingCall.ToString());
            // Registering events and forwarding them to the broadcast object
            // RegisterReceiver(m_Receiver, m_Filter);
        }

        private void OnStartController(object sender, EventArgs e)
        {
            if (mSocketConnection.IsSocketConnected == false)
            {
                mSocketConnection.Start();
            }
            mSocketConnection.isConnected = true;
            SetContentView(Resource.Layout.ControllerLayout);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

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

            if(mIsConnected == false)
            {
                mSocketReader = new SocketReader(mSocketConnection.InputStream);
                mSocketReader.Start();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            WriteLogData();
            mSocketConnection.isConnected = false;
            /*mSocketConnection.OnCancel();
            mSocketReader.Close();*/
        }
        public delegate void Clean();
        protected override void OnStop()
        {
            base.OnStop();
            WriteLogData();
            mSocketConnection.isConnected = false;
            /*mSocketConnection.OnCancel();
            mSocketReader.Close();*/
        }

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

        private void OnThrottleRightClick(object sender, EventArgs e)
        {
            Inverted = ControllerSettings.ACTIVE;
            mIvMode.SetImageResource(Resource.Drawable.mode2);
        }

        private void OnThrottleLeftClick(object sender, EventArgs e)
        {
            Inverted = ControllerSettings.INACTIVE;
            mIvMode.SetImageResource(Resource.Drawable.mode1);
        }

        private void OnRgClick(object sender, EventArgs e)
        {
            if (mRbThrottleLeft.Selected)
            {
                Inverted = ControllerSettings.INACTIVE;

            }
            if (mRbThrottleRight.Selected)
            {
                Inverted = ControllerSettings.ACTIVE;
            }
        }
        private void OnBackToMain(object sender, EventArgs e)
        {
            this.Finish();
        }


    }
}