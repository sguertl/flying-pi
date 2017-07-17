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

namespace Android_WifiDirect_Intro
{
    public class PeerItem
    {
        public string Name { get; set; }
        public string Address { get; set; }

        public override string ToString()
        {
            return string.Format("{0}\n{1}", Name, Address);
        }
    }
}