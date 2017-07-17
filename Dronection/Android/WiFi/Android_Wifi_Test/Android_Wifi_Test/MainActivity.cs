using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net.Wifi;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Android.Net.Wifi.P2p;
using System.Collections;

namespace Android_Wifi_Test
{
    [Activity(Label = "Android_Wifi_Test", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private ListView mListView;
        private ArrayAdapter<WifiEntry> mAdapter;
        private string mSelectedSsid;

        private const int WpaDialog = 0;
        private const int WepDialog = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.Main);

            mListView = FindViewById<ListView>(Resource.Id.listView1);
            RefreshWifiList();

            mListView.ItemClick += ListViewOnItemClick;
        }

        private void ListViewOnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            var wifiItem = mAdapter.GetItem(itemClickEventArgs.Position);
            mSelectedSsid = wifiItem.SSID;

            if (wifiItem.Encryption.Contains("WPA"))
            {
                OnCreateDialog(WpaDialog).Show();

            }
        }

        private void RefreshWifiList()
        {
            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            wifiManager.StartScan();

            ThreadPool.QueueUserWorkItem(lol =>
            {
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    var wifiList = wifiManager.ScanResults;

                    if (null == mAdapter)
                    {
                        mAdapter = new ArrayAdapter<WifiEntry>(this, Android.Resource.Layout.SimpleListItem1, Android.Resource.Id.Text1);
                        RunOnUiThread(() => mListView.Adapter = mAdapter);
                    }

                    if (mAdapter.Count > 0)
                    {
                        RunOnUiThread(() => mAdapter.Clear());
                    }
                    IEnumerable<ScanResult> results = wifiList.GroupBy(sr => sr.Ssid).Select(group => group.First());
                    foreach (var wifi in results)
                    {
                        var wifi1 = wifi;
                        RunOnUiThread(() => 
                        {
                            mAdapter.Add(new WifiEntry { SSID = wifi1.Ssid, BSSID = wifi1.Bssid, Encryption = wifi1.Capabilities });
                        });
                    }

                    RunOnUiThread(() => mAdapter.NotifyDataSetChanged());
                }
            });
        }

        protected override Dialog OnCreateDialog(int id)
        {
            var customView = LayoutInflater.Inflate(Resource.Layout.wifi_dialog, null);
            var builder = new AlertDialog.Builder(this);
            builder.SetIcon(Android.Resource.Drawable.IcMenuPreferences);
            builder.SetView(customView);
            builder.SetTitle("Set Wifi password");

            switch (id)
            {
                case WpaDialog:
                {
                        builder.SetPositiveButton("OK", WpaOkClicked);
                        builder.SetNegativeButton("Cancel", CancelClicked);
                        return builder.Create();
                }
                case WepDialog:
                {
                        builder.SetPositiveButton("OK", WepOkClicked);
                        builder.SetNegativeButton("Cancel", CancelClicked);
                        return builder.Create();
                }
            }
            return base.OnCreateDialog(id);
        }

        private void WpaOkClicked(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            var password = (EditText)dialog.FindViewById(Resource.Id.password);

            var conf = new WifiConfiguration();
            conf.Ssid = "\"" + mSelectedSsid + "\"";
            conf.PreSharedKey = "\"" + password + "\"";

            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            wifiManager.AddNetwork(conf);
            foreach(var network in wifiManager.ConfiguredNetworks.Where(n => n.Ssid.Contains(mSelectedSsid)))
            {
                wifiManager.Disconnect();
                wifiManager.EnableNetwork(network.NetworkId, true);
                wifiManager.Reconnect();
            }
        }

        private void WepOkClicked(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            var password = (EditText)dialog.FindViewById(Resource.Id.password);

            var conf = new WifiConfiguration();
            conf.Ssid = "\"" + mSelectedSsid + "\"";
            conf.WepKeys[0] = "\"" + password + "\"";
            conf.WepTxKeyIndex = 0;
            conf.AllowedKeyManagement.Set((int)KeyManagementType.None);
            conf.AllowedPairwiseCiphers.Set((int)GroupCipherType.Wep40);

            var wifiManager = GetSystemService(WifiService).JavaCast<WifiManager>();
            wifiManager.AddNetwork(conf);
            WifiP2pDevice wifi = new WifiP2pDevice();
           
            foreach(var network in wifiManager.ConfiguredNetworks.Where(n => n.Ssid.Contains(mSelectedSsid)))
            {
                wifiManager.Disconnect();
                wifiManager.EnableNetwork(network.NetworkId, true);
                wifiManager.Reconnect();
            }
        }

        private void CancelClicked(object sender, DialogClickEventArgs e)
        {
            //
        }
    }

    public class MyComparer : IEqualityComparer<ScanResult>
    {
        public bool Equals(ScanResult x, ScanResult y)
        {
            return x.Ssid == y.Ssid;
        }

        public int GetHashCode(ScanResult obj)
        {
            return obj.GetHashCode();
        }
    }
}

