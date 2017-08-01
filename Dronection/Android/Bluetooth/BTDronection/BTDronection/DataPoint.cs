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

namespace BTDronection
{
    public class DataPoint
    {

        private float mX;

        public float X
        {
            get { return mX; }
            set { mX = value; }
        }

        private float mY;

        public float Y
        {
            get { return mY; }
            set { mY = value; }
        }


        public DataPoint(float x, float y)
        {
            this.mX = x;
            this.mY = y;
        }

        public override string ToString()
        {
            return String.Format("[{0}/{1}]", mX, mY);
        }

    }
}