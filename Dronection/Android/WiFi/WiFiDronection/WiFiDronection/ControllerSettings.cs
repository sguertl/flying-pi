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
        // Constants
        public static readonly bool ACTIVE = true;
        public static readonly bool INACTIVE = false;

        /// <summary>
        /// Are the joysticks inverted
        /// </summary>
        public bool Inverted
        {
            get;
            set;
        }

        /// <summary>
        /// Trim of yaw parameter [-30;30]
        /// </summary>
        public int TrimYaw
        {
            get;
            set;
        }

        /// <summary>
        /// Trim of pitch parameter [-30;30]
        /// </summary>
        public int TrimPitch
        {
            get;
            set;
        }

        /// <summary>
        /// Trim of roll paramter [-30;30]
        /// </summary>
        public int TrimRoll
        {
            get;
            set;
        }

        /// <summary>
        /// Is Height control of the copter active
        /// </summary>
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