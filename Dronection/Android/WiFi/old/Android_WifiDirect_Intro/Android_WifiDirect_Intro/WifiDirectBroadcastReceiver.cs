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
using Android.Net.Wifi.P2p;

namespace Android_WifiDirect_Intro
{
    public class WifiDirectBroadcastReceiver : BroadcastReceiver
    {
        private WifiP2pManager mManager;
        private WifiP2pManager.Channel mChannel;
        private MainActivity mActivity;

        public WifiDirectBroadcastReceiver(WifiP2pManager manager, WifiP2pManager.Channel channel, MainActivity activity)
        {
            mManager = manager;
            mChannel = channel;
            mActivity = activity;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;

            if(WifiP2pManager.WifiP2pStateChangedAction == action)
            {
                // prüfen ob wifi aktiviert ist
                int state = intent.GetIntExtra(WifiP2pManager.ExtraWifiState, -1);
                if(state == (int) WifiP2pState.Enabled)
                {
                    Toast.MakeText(context, "Wifi Direct is enabled", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(context, "Wifi Direct is not enabled", ToastLength.Short).Show();
                }
            }
            else if(WifiP2pManager.WifiP2pPeersChangedAction == action)
            {
                // wird bei Wifip2pmanager.requestpeers() aufgerufen
                if(mManager != null)
                {
                    mManager.RequestPeers(mChannel, mActivity);
                }
            }
            else if(WifiP2pManager.WifiP2pConnectionChangedAction == action)
            {
                // antwort bei connect oder disconnect
            }
            else if(WifiP2pManager.WifiP2pThisDeviceChangedAction == action)
            {
                // antwort bei connection änderungen
            }
        }
    }
}