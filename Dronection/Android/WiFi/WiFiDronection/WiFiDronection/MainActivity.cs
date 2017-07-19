using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.Threading;
using Android.Net.Wifi;
using Android.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace WiFiDronection
{
    [Activity(Label = "WiFiDronection", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private ListView mLvPeer;
        private ArrayAdapter<Peer> mAdapter;

        private string mSelectedSsid;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            mLvPeer = FindViewById<ListView>(Resource.Id.lvPeers);
            mLvPeer.ItemClick += OnListViewItemClick;

            //var aj = GetSystemService(WifiService).JavaCast<WifiManager>();
            //aj.Disconnect();

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
                        RunOnUiThread(() => mLvPeer.Adapter = mAdapter);
                    }

                    if (mAdapter.Count > 0)
                    {
                        RunOnUiThread(() => mAdapter.Clear());
                    }

                    //!filter nach neuem und anzeige rpi
                    //IEnumerable<ScanResult> results = wifiList.GroupBy(sr => sr.Ssid).Select(group => group.First());
                    foreach (var wifi in wifiList)//results)
                    {
                        var wifi1 = wifi;
                        RunOnUiThread(() =>
                        {
                            mAdapter.Add(new Peer { SSID = wifi1.Ssid, BSSID = wifi1.Bssid, Encryption = wifi1.Capabilities });
                        });
                    }

                    RunOnUiThread(() => mAdapter.NotifyDataSetChanged());
                }
            });
        }

        private void OnListViewItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            var wifiItem = mAdapter.GetItem(itemClickEventArgs.Position);
            mSelectedSsid = wifiItem.SSID;
            OnCreateDialog(0).Show();
        }

        protected override Dialog OnCreateDialog(int id)
        {
            var customView = LayoutInflater.Inflate(Resource.Layout.WifiDialog, null);
            var builder = new AlertDialog.Builder(this);

            builder.SetIcon(Android.Resource.Drawable.IcMenuPreferences);
            builder.SetView(customView);
            builder.SetTitle("Set Wifi password");
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
            wifiManager.EnableNetwork(wc.NetworkId, true);
            wifiManager.Reconnect();

            if (wifiManager.IsWifiEnabled)
            {
                //StartActivity(typeof(DataTransferActivity));
                // Go to next activity
            }
            else
            {
                Toast.MakeText(this, "Could not connect to peer", ToastLength.Short).Show();
            }
        }

        private void CancelClicked(object sender, DialogClickEventArgs e)
        {
            //
        }
    }
}

