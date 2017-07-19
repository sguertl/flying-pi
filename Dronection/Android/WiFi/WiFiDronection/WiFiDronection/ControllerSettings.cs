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
    public class ControllerSettings
    {
        public static readonly bool ACTIVE = true;
        public static readonly bool INACTIVE = false;

        public bool Inverted
        {
            get;
            set;
        }

        public int TrimYaw
        {
            get;
            set;
        }

        public int TrimPitch
        {
            get;
            set;
        }

        public int TrimRoll
        {
            get;
            set;
        }

        public bool HeightControlActivated
        {
            get;
            set;
        }

        public ControllerSettings()
        {

        }
    }
}