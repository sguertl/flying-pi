﻿/************************************************************************
*																		*
*  Copyright (C) 2017 Infineon Technologies Austria AG.					*
*																		*
*  Licensed under the Apache License, Version 2.0 (the "License");		*
*  you may not use this file except in compliance with the License.		*
*  You may obtain a copy of the License at								*
*																		*
*    http://www.apache.org/licenses/LICENSE-2.0							*
*																		*
*  Unless required by applicable law or agreed to in writing, software	*
*  distributed under the License is distributed on an "AS IS" BASIS,	*
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or		*
*  implied.																*
*  See the License for the specific language governing					*
*  permissions and limitations under the License.						*
*																		*
*																		*
*  File: ControllerSettings.cs											*
*  Created on: 2017-07-19                                   			*
*  Author(s): Guertl Sebastian Matthias (IFAT PMM TI COP)				*
*																		*
*  ControllerSettings stores various settings which are important		*
*  when piloting the drone.                                             *
*																		*
************************************************************************/

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