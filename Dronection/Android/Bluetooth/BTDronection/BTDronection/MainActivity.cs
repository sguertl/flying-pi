using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Util;
using Android.Graphics;

namespace BTDronection
{
    [Activity(Label = "BTDronection", MainLauncher = true, Icon = "@drawable/icon", 
        Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        // Members
        private BluetoothAdapter m_BtAdapter;
        private Button mBtShowDevices;
        private LinearLayout m_Linear;
        private TextView mTvHeader;
        private Button mBtShowLog;
        private Button mBtHelp;
        private TextView mTvFooter;

        public static string ApplicationFolderPath;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            
            Init();

            // Checking if bluetooth is supported
            if (m_BtAdapter == null)
            {
                Toast.MakeText(ApplicationContext, "Bluetooth is not supported", 0).Show();

                // Displaying an alert to inform the user that bluetooth is not supported
                AlertDialog alert = new AlertDialog.Builder(this).Create();
                alert.SetTitle("Bluetooth not supported");
                alert.SetMessage("Bluetooth is not supported!");
                alert.SetButton("Ok", (s, ev) => { Finish(); });
                alert.Show();
            }
            else
            {
                // Checking if bluetooth is enabled
                if (!m_BtAdapter.IsEnabled) { TurnBTOn(); }
            }

            CreateApplicationFolder();
        }

        /// <summary>
        /// Initializing and modifies objects
        /// </summary>
        public void Init()
        {
            // Initializing objects
            m_BtAdapter = BluetoothAdapter.DefaultAdapter;
            mBtShowDevices = FindViewById<Button>(Resource.Id.btShowDevices);
            m_Linear = FindViewById<LinearLayout>(Resource.Id.linear);
            mBtShowLog = FindViewById<Button>(Resource.Id.btLog);
            mBtHelp = FindViewById<Button>(Resource.Id.btHelp);
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeader);
            mTvFooter = FindViewById<TextView>(Resource.Id.tvFooter);
            
            // Setting activity background
            m_Linear.SetBackgroundColor(Android.Graphics.Color.White);

            mBtShowLog.Click += delegate
            {
                StartActivity(typeof(LogActivity));
            };

            mBtHelp.Click += delegate
            {
                StartActivity(typeof(HelpActivity));
            };

            // Handling paired devices button click
            mBtShowDevices.Click += delegate
            {
                if (m_BtAdapter.IsEnabled)
                {
                    StartActivity(typeof(PairedDevices));
                }
                else
                {
                    Toast.MakeText(this, "Bluetooth has to be turned on", ToastLength.Short).Show();
                }
            };

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");
            mTvHeader.Typeface = font;
            mBtShowDevices.Typeface = font;
            mBtHelp.Typeface = font;
            mTvFooter.Typeface = font;
            mBtShowLog.Typeface = font;
        }

        /// <summary>
        /// Enables bluetooth on the device
        /// </summary>
        public void TurnBTOn()
        {
            Intent intent = new Intent(BluetoothAdapter.ActionRequestEnable);
            StartActivityForResult(intent, 1);
        }

        private void CreateApplicationFolder()
        {
            // Creates Application folder on internal mobile storage
            ApplicationFolderPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "airything");
            ApplicationFolderPath += Java.IO.File.Separator + "bluetooth";
            var storageDir = new Java.IO.File(ApplicationFolderPath + Java.IO.File.Separator + "settings");
            storageDir.Mkdirs();
            var settingsFile = new Java.IO.File(ApplicationFolderPath + Java.IO.File.Separator + Java.IO.File.Separator + "settings.csv");
            settingsFile.CreateNewFile();
        }
    }
}

