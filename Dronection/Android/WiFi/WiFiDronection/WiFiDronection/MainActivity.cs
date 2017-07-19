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

namespace WiFiDronection
{
    [Activity(MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Holo.Light")]
    public class MainActivity : Activity
    {
        private TextView mTvHeader;
        private ListView mLvPeer;

        private ArrayAdapter<Peer> mAdapter;
        private List<Peer> mPeerList;
        private string mSelectedSsid;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            //RequestWindowFeature(WindowFeatures.NoTitle);
            Window.AddFlags(WindowManagerFlags.Fullscreen);

            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeader);
            var font = Typeface.CreateFromAsset(Assets, "OpenSans-Light.ttf");
            mTvHeader.Typeface = font;

            mLvPeer = FindViewById<ListView>(Resource.Id.lvPeers);
            mLvPeer.ItemClick += OnListViewItemClick;

            mPeerList = new List<Peer>();

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
                    
                    IEnumerable<ScanResult> results = wifiList.Where(w => w.Ssid.ToUpper().Contains("RPI") || w.Ssid.ToUpper().Contains("RASPBERRY"));

                    foreach (var wifi in results)
                    {
                        var wifi1 = wifi;
                        RunOnUiThread(() =>
                        {
                            if(mPeerList.Any(p => p.SSID == wifi1.Ssid) == false)
                            {
                                Peer p = new Peer { SSID = wifi1.Ssid, BSSID = wifi1.Bssid, Encryption = wifi1.Capabilities };
                                mAdapter.Add(p);
                                mPeerList.Add(p);
                            }
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
            wifiManager.EnableNetwork(id, true);
            wifiManager.Reconnect();

            if (wifiManager.IsWifiEnabled)
            {
                StartActivity(typeof(ControllerActivity));
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

