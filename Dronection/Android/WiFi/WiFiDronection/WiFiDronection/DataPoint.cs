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
   public class DataPoint
    {

        private float m_X;
        private float m_Y;

        public float Y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        public float X
        {
            get { return m_X; }
            set { m_X = value; }
        }

        public DataPoint(float x, float y)
        {
            this.m_X = x;
            this.m_Y = y;
        }

        public override string ToString()
        {
            return String.Format("[{0}/{1}]", m_X, m_Y);
        }
    }
}