/************************************************************************
*                                                                       *
*  Copyright (C) 2017 Infineon Technologies Austria AG.                 *
*                                                                       *
*  Licensed under the Apache License, Version 2.0 (the "License");      *
*  you may not use this file except in compliance with the License.     *
*  You may obtain a copy of the License at                              *
*                                                                       *
*    http://www.apache.org/licenses/LICENSE-2.0                         *
*                                                                       *
*  Unless required by applicable law or agreed to in writing, software  *
*  distributed under the License is distributed on an "AS IS" BASIS,    *
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or      *
*  implied.                                                             *
*  See the License for the specific language governing                  *
*  permissions and limitations under the License.                       *
*                                                                       *
*                                                                       *
*  File: ControllerSettings.cs                                          *
*  Created on: 2017-07-19                                               *
*  Author(s): Guertl Sebastian Matthias (IFAT PMM TI COP)               *
*                                                                       *
*  ControllerSettings stores various settings which are important       *
*  when piloting the drone.                                             *
*                                                                       *
************************************************************************/

namespace BTDronection
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
        /// Gets or sets the minimum yaw.
        /// </summary>
        /// <value>The minimum yaw.</value>
        public int MinYaw
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the max yaw.
        /// </summary>
        /// <value>The max yaw.</value>
		public int MaxYaw
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the minimum pitch.
        /// </summary>
        /// <value>The minimum pitch.</value>
        public int MinPitch
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the max pitch.
        /// </summary>
        /// <value>The max pitch.</value>
        public int MaxPitch
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum roll.
        /// </summary>
        /// <value>The minimum roll.</value>
		public int MinRoll
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the max roll.
        /// </summary>
        /// <value>The max roll.</value>
		public int MaxRoll
		{
			get;
			set;
		}


		/// <summary>
		/// Trim of yaw parameter [-20;20]
		/// </summary>
		public int TrimYaw
        {
            get;
            set;
        }

		/// <summary>
		/// Trim of pitch parameter [-20;20]
		/// </summary>
		public int TrimPitch
        {
            get;
            set;
        }

		/// <summary>
		/// Trim of roll paramter [-20;20]
		/// </summary>
		public int TrimRoll
        {
            get;
            set;
        }

		/// <summary>
        /// Gets or sets a value indicating whether <see cref="T:BTDronection.ControllerSettings"/> altitude control activated.
        /// </summary>
        /// <value><c>true</c> if altitude control activated; otherwise, <c>false</c>.</value>
		public bool AltitudeControlActivated
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:BTDronection.ControllerSettings"/> logging activated.
        /// </summary>
        /// <value><c>true</c> if logging activated; otherwise, <c>false</c>.</value>
        public bool LoggingActivated
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
