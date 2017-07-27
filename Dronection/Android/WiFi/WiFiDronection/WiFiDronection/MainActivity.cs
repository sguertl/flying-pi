using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.Threading;
using Android.Net.Wifi;
using Android.Runtime;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Views;
using System;

namespace WiFiDronection
{
    [Activity(MainLauncher = true,
        Icon = "@drawable/icon",
        Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class MainActivity : Activity
    {
        // Members
        private TextView mTvHeader;
        private TextView mTvWifiName;
        private TextView mTvWifiMac;
        private TextView mTvFooter;
        private Button mBtnConnect;
        private Button mBtnShowLogs;
        private Button mBtnHelp;
        private TextView mTvHeaderDialog;
        
        private string mSelectedSsid;
        private string mLastConnectedPeer;
        private bool mIsConnected;

        // Public variable 
        // Root path for project folder
        public static string ApplicationFolderPath;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            // Initialize members
            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeader);
            mTvWifiName = FindViewById<TextView>(Resource.Id.tvWifiName);
            mTvWifiMac = FindViewById<TextView>(Resource.Id.tvWifiMac);
            mTvFooter = FindViewById<TextView>(Resource.Id.tvFooter);
            mBtnConnect = FindViewById<Button>(Resource.Id.btnConnect);
            mBtnShowLogs = FindViewById<Button>(Resource.Id.btnShowLogs);
            mBtnHelp = FindViewById<Button>(Resource.Id.btnHelp);

            mTvHeader.Typeface = font;
            mTvWifiName.Typeface = font;
            mTvWifiMac.Typeface = font;
            mTvFooter.Typeface = font;
            mBtnConnect.Typeface = font;
            mBtnShowLogs.Typeface = font;
            mBtnHelp.Typeface = font;

            mBtnConnect.Enabled = false;
            mBtnConnect.Click += OnConnect;

            mBtnShowLogs.Click += OnShowLogFiles;

            mBtnHelp.Click += OnHelp;

            mLastConnectedPeer = "";
            mIsConnected = false;

            // Turn on wifi if turned off
            WifiManager wm = GetSystemService(WifiService).JavaCast<WifiManager>();
            if (wm.IsWifiEnabled == false)
            {
                wm.SetWifiEnabled(true);
            }

            CreateApplicationFolder();

            RefreshWifiList();
        }

        /// <summary>
        /// Scan for wifi devices
        /// </summary>
        private void RefreshWifiList()
        {
            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            wifiManager.StartScan();

            // Start searching thread
            ThreadPool.QueueUserWorkItem(lol =>
            {
                while (true)
                {
                    Thread.Sleep(3000);
                    var wifiList = wifiManager.ScanResults;

                    // Filter devices by Rasp or Pi
                    IEnumerable<ScanResult> results = wifiList.Where(w => w.Ssid.ToUpper().Contains("RASP") || w.Ssid.ToUpper().Contains("PI"));
                    var wifi = results.First();
                    RunOnUiThread(() =>
                    {
                        // Show selected wifi device
                        mSelectedSsid = wifi.Ssid;
                        mTvWifiName.Text = "SSID: " + wifi.Ssid;
                        mTvWifiMac.Text = "MAC: " + wifi.Bssid;
                        mBtnConnect.Enabled = true;
                        mBtnConnect.SetBackgroundColor(Color.ParseColor("#005DA9"));
                    });
                }
            });
        }

        /// <summary>
        /// Onclick event for connect button
        /// </summary>
        private void OnConnect(object sender, EventArgs e)
        {
            // Check if there is already a connection to the wifi device
            if(mLastConnectedPeer != mSelectedSsid)
            {
                // Open Password dialog for building wifi connection
                OnCreateDialog(0).Show();
            }
            else
            {
                // Open controller activity
                Intent intent = new Intent(BaseContext, typeof(ControllerActivity));
                intent.PutExtra("isConnected", mIsConnected);
                StartActivity(intent);
            }
        }

        protected override Dialog OnCreateDialog(int id)
        {
            var wifiDialogView = LayoutInflater.Inflate(Resource.Layout.WifiDialog, null);
            var wifiDialogHeaderView = FindViewById(Resource.Layout.WifiDialogTitle);


            var builder = new AlertDialog.Builder(this);
            //mTvHeaderDialog = FindViewById<TextView>(Resource.Id.tvHeaderDialog);
            //mTvHeaderDialog.Typeface = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");


            builder.SetIcon(Resource.Drawable.ifx_logo_small);
            builder.SetView(wifiDialogView);
            builder.SetTitle("Enter WiFi password");
            builder.SetCustomTitle(wifiDialogHeaderView);
            builder.SetPositiveButton("OK", WpaOkClicked);
            builder.SetNegativeButton("Cancel", CancelClicked);

            return builder.Create();
        }

        /// <summary>
        /// Onclick event for Ok button of password dialog
        /// </summary>
        private void WpaOkClicked(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            // Get password
            var password = (EditText)dialog.FindViewById(Resource.Id.etDialogPassword);

            var conf = new WifiConfiguration();
            conf.Ssid = "\"" + mSelectedSsid + "\"";
            conf.PreSharedKey = "\"" + password.Text + "\"";

            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            // Connect network
            int id = wifiManager.AddNetwork(conf);

            IList<WifiConfiguration> myWifi = wifiManager.ConfiguredNetworks;

            WifiConfiguration wc = myWifi.First(x => x.Ssid.Contains(mSelectedSsid));
            wifiManager.Disconnect();
            wifiManager.EnableNetwork(id, true);
            wifiManager.Reconnect();

            // check if password is correct
            if (wifiManager.IsWifiEnabled)
            {
                mLastConnectedPeer = mSelectedSsid;
                Intent intent = new Intent(BaseContext, typeof(ControllerActivity));
                intent.PutExtra("isConnected", mIsConnected);
                StartActivity(intent);
                mIsConnected = true;
            }
            else
            {
                Toast.MakeText(this, "Could not connect to peer", ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// Onclick event on cancel button of password dialog
        /// </summary>
        private void CancelClicked(object sender, DialogClickEventArgs e)
        {
            // Do nothing
        }

        /// <summary>
        /// Onclick event on show logs button
        /// </summary>
        private void OnShowLogFiles(object sender, EventArgs e)
        {
            // Opens Log activity
            StartActivity(typeof(LogActivity));
        }

        /// <summary>
        /// Onclick event on help button
        /// </summary>
        private void OnHelp(object sender, EventArgs e)
        {
            // Opens Help activity
            StartActivity(typeof(HelpActivity));
        }

        private void CreateApplicationFolder()
        {
            // Creates Application folder on internal mobile storage
            ApplicationFolderPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "Airything");
            var storageDir = new Java.IO.File(ApplicationFolderPath);
            storageDir.Mkdirs();
        }
    }
}