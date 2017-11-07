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
using Android.Bluetooth;

namespace Datalyze
{
    public class HelpClass
    {
        private static HelpClass instance = null;
        private static readonly object padlock = new object();

        private BluetoothDevice mBluetoohtDevice;

        public BluetoothDevice BluetoohtDevice
        {
            get { return mBluetoohtDevice; }
            set { mBluetoohtDevice = value; }
        }


        private HelpClass()
        {

        } 

        public static HelpClass Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new HelpClass();
                    }
                    return instance;
                }
            }
        }


    }
}