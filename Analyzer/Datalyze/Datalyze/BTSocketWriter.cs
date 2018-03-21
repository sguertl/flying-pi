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
   public class BTSocketWriter
    {

        private DataOutputStream mDataOutputStream;

        public BTSocketWriter(DataOutputStream dataoutputstream)
        {
            mDataOutputStream = dataoutputstream;
        }

        public void Write(params byte[] bytes)
        {
            try
            {
                    mDataOutputStream.Write(bytes);
                    mDataOutputStream.Flush();             
            }catch(Exception ex)
            {
                Log.Debug("BTSocketWriter", "Error while sending data");
            }
        }

        public void Close()
        {
            try
            {
                if (mDataOutputStream != null)
                {
                    mDataOutputStream.Close();
                }
            }catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}