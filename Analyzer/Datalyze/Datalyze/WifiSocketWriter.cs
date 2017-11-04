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
using Java.IO;
using Android.Util;
using System.IO;

namespace Datalyze
{
    public class WifiSocketWriter
    {
        private DataOutputStream mOutputStream;

        public WifiSocketWriter(Stream outputstream)
        {
            mOutputStream = new DataOutputStream(outputstream);
        }

        public void Write(byte[] bytes)
        {
            try
            {
                mOutputStream.Write(bytes);
                mOutputStream.Flush();
            }
            catch(Java.Lang.Exception ex)
            {
                Log.Debug("WifiSocketWriter", "Error while sending data");
            }
        }

        public void Close()
        {
            if(mOutputStream != null)
            {
                mOutputStream.Close();
            }
        }
    }
}