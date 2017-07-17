using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Net.Wifi.P2p;
using Android.Content;
using System;
using System.Collections.Generic;

namespace Android_WifiDirect_Intro
{
    [Activity(Label = "Android_WifiDirect_Intro", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, WifiP2pManager.IPeerListListener
    {
        private ListView mLvPeers;
        private ArrayAdapter<PeerItem> mAdapter;
        private WifiP2pManager mManager;
        private WifiP2pManager.Channel mChannel;
        private WifiDirectBroadcastReceiver mReceiver;
        private IntentFilter mIntentFilter;
        private List<WifiP2pDevice> mPeers;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            mLvPeers = FindViewById<ListView>(Resource.Id.lvPeers);

            mManager = (WifiP2pManager)GetSystemService(WifiP2pService);
            mChannel = mManager.Initialize(this, MainLooper, null);
            mReceiver = new WifiDirectBroadcastReceiver(mManager, mChannel, this);

            mIntentFilter = new IntentFilter();
            mIntentFilter.AddAction(WifiP2pManager.WifiP2pStateChangedAction);
            mIntentFilter.AddAction(WifiP2pManager.WifiP2pPeersChangedAction);
            mIntentFilter.AddAction(WifiP2pManager.WifiP2pConnectionChangedAction);
            mIntentFilter.AddAction(WifiP2pManager.WifiP2pThisDeviceChangedAction);

            mPeers = new List<WifiP2pDevice>();
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver(mReceiver, mIntentFilter);
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnregisterReceiver(mReceiver);
        }

        public void OnListAllPeers(View view)
        {
            mManager.DiscoverPeers(mChannel, new WifiDirectActionListener(this, "Discovery", () => { }));
        }

        public void OnPeersAvailable(WifiP2pDeviceList peers)
        {
            foreach(var peer in peers.DeviceList)
            {
                mPeers.Add(peer);
                
            }

            mLvPeers.Adapter = mAdapter;

            if(mPeers.Count == 0)
            {
                Toast.MakeText(this, "No devices found", ToastLength.Short).Show();
            }
        }
    }
}

