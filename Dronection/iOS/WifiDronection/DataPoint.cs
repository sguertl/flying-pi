using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiDronection
{
   public class DataPoint
    {
        // X coordinate
        private float mX;

        public float X
        {
            get { return mX; }
            set { mX = value; }
        }

        // Y coordinate
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
