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

namespace Android_Wifi_Test
{
    public class WifiEntry
    {
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public string Encryption { get; set; }

        public override string ToString()
        {
            return string.Format("SSID: {0}\nBSSID: {1}\nEncryption: {2}", SSID, BSSID, Encryption);
        }
    }
}