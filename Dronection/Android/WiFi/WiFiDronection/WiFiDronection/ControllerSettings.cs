namespace WiFiDronection
{
    public class ControllerSettings
    {
        // Constants
        public static readonly bool ACTIVE = true;
        public static readonly bool INACTIVE = false;

        /// <summary>
        /// Flying mode
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
        /// Altitude control
        /// </summary>
        public bool AltitudeControlActivated
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:WiFiDronection.ControllerSettings"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:WiFiDronection.ControllerSettings"/>.</returns>
        public override string ToString()
        {
            return TrimYaw + ";" + TrimPitch + ";" + TrimRoll;
        }
    }
}