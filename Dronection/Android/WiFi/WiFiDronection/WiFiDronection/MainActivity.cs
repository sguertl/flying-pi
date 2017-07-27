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
        private TextView mTvHeader;
        private TextView mTvWifiName;
        private TextView mTvWifiMac;
        private TextView mTvFooter;
        private Button mBtnConnect;
        private Button mBtnShowLogs;
        private Button mBtnHelp;

        private TextView mTvHeaderDialog;

        private ArrayAdapter<Peer> mAdapter;
        private List<Peer> mPeerList;
        private string mSelectedSsid;
        private string mLastConnectedPeer;
        private bool mIsConnected;

        public static string ApplicationFolderPath;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

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

            mPeerList = new List<Peer>();

            mLastConnectedPeer = "";
            mIsConnected = false;

            WifiManager wm = GetSystemService(WifiService).JavaCast<WifiManager>();
            if (wm.IsWifiEnabled == false)
            {
                wm.SetWifiEnabled(true);
            }

            CreateApplicationFolder();

            RefreshWifiList();
        }

        private void RefreshWifiList()
        {
            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            wifiManager.StartScan();

            ThreadPool.QueueUserWorkItem(lol =>
            {
                while (true)
                {
                    Thread.Sleep(3000);
                    var wifiList = wifiManager.ScanResults;

                    if (mAdapter == null)
                    {
                        mAdapter = new ArrayAdapter<Peer>(this, Android.Resource.Layout.SimpleListItem1, Android.Resource.Id.Text1);
                    }

                    IEnumerable<ScanResult> results = wifiList.Where(w => w.Ssid.ToUpper().Contains("RASP") || w.Ssid.ToUpper().Contains("PI"));

                    foreach (var wifi in results)
                    {
                        var wifi1 = wifi;
                        RunOnUiThread(() =>
                        {
                            if (mPeerList.Any(p => p.SSID == wifi1.Ssid) == false)
                            {
                                Peer p = new Peer { SSID = wifi1.Ssid, BSSID = wifi1.Bssid, Encryption = wifi1.Capabilities };

                                mSelectedSsid = p.SSID;
                                mTvWifiName.Text = "SSID: " + p.SSID;
                                mTvWifiMac.Text = "MAC: " + p.BSSID;
                                mBtnConnect.Enabled = true;
                                mBtnConnect.SetBackgroundColor(Color.ParseColor("#005DA9"));

                                mAdapter.Add(p);
                                mPeerList.Add(p);
                            }
                        });
                    }

                    RunOnUiThread(() => mAdapter.NotifyDataSetChanged());
                }
            });
        }

        private void OnConnect(object sender, EventArgs e)
        {
            if(mLastConnectedPeer != mSelectedSsid)
            {
                OnCreateDialog(0).Show();
            }
            else
            {
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

        private void WpaOkClicked(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            var password = (EditText)dialog.FindViewById(Resource.Id.etDialogPassword);

            var conf = new WifiConfiguration();
            conf.Ssid = "\"" + mSelectedSsid + "\"";
            conf.PreSharedKey = "\"" + password.Text + "\"";

            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();

            int id = wifiManager.AddNetwork(conf);

            IList<WifiConfiguration> myWifi = wifiManager.ConfiguredNetworks;

            WifiConfiguration wc = myWifi.First(x => x.Ssid.Contains(mSelectedSsid));
            wifiManager.Disconnect();
            wifiManager.EnableNetwork(id, true);
            wifiManager.Reconnect();

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

        private void CancelClicked(object sender, DialogClickEventArgs e)
        {
            // Do nothing
        }

        private void OnShowLogFiles(object sender, EventArgs e)
        {
            StartActivity(typeof(LogActivity));
        }

        private void OnHelp(object sender, EventArgs e)
        {
            StartActivity(typeof(HelpActivity));
        }

        private void CreateApplicationFolder()
        {
            ApplicationFolderPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "Airything");
            var storageDir = new Java.IO.File(ApplicationFolderPath);
            storageDir.Mkdirs();
        }
    }
}