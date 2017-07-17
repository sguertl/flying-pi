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
    public class WifiDirectActionListener : Java.Lang.Object, WifiP2pManager.IActionListener
    {
        private Context mContext;
        private string mFailure;
        private Action mAction;

        public WifiDirectActionListener(Context context, string failure, Action action)
        {
            mContext = context;
            mFailure = failure;
            mAction = action;
        }

        public void OnFailure(WifiP2pFailureReason reason)
        {
            Toast.MakeText(mContext, mFailure + " Failed: " + reason, ToastLength.Short).Show();
        }

        public void OnSuccess()
        {
            Toast.MakeText(mContext, mFailure + " Discovery initiated", ToastLength.Short).Show();
            mAction.Invoke();
        }
    }
}