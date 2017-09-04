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

namespace WiFiDronection
{
    public class LogData
    {
        private string mName;
        private string mCsvString;
        private int mBytes;

        public int Bytes
        {
            get { return mBytes; }
            set { mBytes = value; }
        }

        public LogData(string name, int bytes)
        {
            mName = name;
            mBytes = bytes;

            mCsvString = "";
        }

        public void Add(string text)
        {
            mCsvString += text + "\n";
        }

        public override string ToString()
        {
            return mCsvString;
        }
    }
}