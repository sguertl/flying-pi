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
using System.Threading;
using Android.Net.Wifi;

namespace Datalyze
{
    [Activity(Label = "WifiConnectionActivity", 
        Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class WifiConnectionActivity : Activity
    {
        private TextView mTvWifiName;
        private TextView mTvWifiMac;
        private Button mBtnConnect;
        
        private string mSelectedSsid;
        private string mSelectedBssid;
        private string mLastConnectedPeer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WifiConnectionLayout);

            // Create font
            //var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            // Initialize widgets
            mTvWifiName = FindViewById<TextView>(Resource.Id.tvWifiName);
            mTvWifiMac = FindViewById<TextView>(Resource.Id.tvWifiMac);
            mBtnConnect = FindViewById<Button>(Resource.Id.btnConnect);

            // Set font to widgets
            //mTvWifiName.Typeface = font;
            //mTvWifiMac.Typeface = font;
            //mBtnConnect.Typeface = font;

            // Disable Connect button while searching for connection
            //mBtnConnect.Enabled = false;

            // Add events to buttons
            //mBtnConnect.Click += OnConnect;

            //RefreshWifiList();
        }

        private void RefreshWifiList()
        {
            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            wifiManager.StartScan();

            // Start searching thread
            /*ThreadPool.QueueUserWorkItem(x =>
            {
                while (true)
                {
                    Thread.Sleep(3000);
                    var wifiList = wifiManager.ScanResults;

                    if (wifiList != null && wifiList.Count > 0)
                    {
                        // Filter devices by Rasp or Pi
                        //IEnumerable<ScanResult> results = wifiList.Where(w => w.Ssid.ToUpper().Contains("RASP") || w.Ssid.ToUpper().Contains("PI"));
                        try
                        {
                            var wifi = wifiList.First();
                            RunOnUiThread(() =>
                            {
                                // Show selected wifi device
                                mSelectedSsid = wifi.Ssid;
                                mSelectedBssid = wifi.Bssid;
                                mTvWifiName.Text = "SSID: " + wifi.Ssid;
                                mTvWifiMac.Text = "MAC: " + wifi.Bssid;
                                mBtnConnect.Enabled = true;
                                mBtnConnect.Text = "Connect";
                                mBtnConnect.SetBackgroundColor(Color.ParseColor("#005DA9"));
                            });
                        }
                        catch (InvalidOperationException ex)
                        {
                            RunOnUiThread(() =>
                            {
                                mBtnConnect.Text = "Can't find WiFi connection";
                            });
                        }
                    }
                }
            });*/
        }

        private void OnConnect(object sender, EventArgs e)
        {
            // Check if there is already a connection to the wifi device
            if (mLastConnectedPeer != mSelectedSsid)
            {
                // Open Password dialog for building a wifi connection
                OnCreateDialog(0).Show();
            }
            else
            {
                // Open controller activity
                //Intent intent = new Intent(BaseContext, typeof(ControllerActivity));
                // intent.PutExtra("isConnected", mIsConnected);
                //intent.PutExtra("mac", mSelectedBssid);
                //StartActivity(intent);
            }
        }

        protected override Dialog OnCreateDialog(int id)
        {
            var wifiDialogView = LayoutInflater.Inflate(Resource.Layout.WifiDialog, null);

            var builder = new AlertDialog.Builder(this);
            builder.SetIcon(Resource.Drawable.ifx_logo_small);
            builder.SetView(wifiDialogView);
            builder.SetTitle("Enter WiFi password");
            builder.SetPositiveButton("OK", WpaOkClicked);
            builder.SetNegativeButton("Cancel", (sender, e) => { });

            return builder.Create();
        }

        private void WpaOkClicked(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;

            // Get entered password
            var password = (EditText)dialog.FindViewById(Resource.Id.etDialogPassword);

            var conf = new WifiConfiguration();
            conf.Ssid = "\"" + mSelectedSsid + "\"";
            conf.PreSharedKey = "\"" + password.Text + "\"";

            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            // Connect network
            int id = wifiManager.AddNetwork(conf);

            if (id != -1)
            {
                wifiManager.Disconnect();
                wifiManager.EnableNetwork(id, true);
                wifiManager.Reconnect();
                mLastConnectedPeer = mSelectedSsid;
                //Intent intent = new Intent(BaseContext, typeof(ControllerActivity));
                // intent.PutExtra("isConnected", mIsConnected);
                //intent.PutExtra("mac", mSelectedBssid);
                //StartActivity(intent);
                //mIsConnected = true;
            }
            else
            {
                Toast.MakeText(this, "Could not connect to peer", ToastLength.Short).Show();
            }
        }
    }
}