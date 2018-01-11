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
*  File: SocketWriter.cs                                                *
*  Created on: 2017-07-28                                               *
*  Author(s): Adrian Klapsch                                            *
*                                                                       *
*  SocketWriter.cs writes data from the controller (smartphone)         *
*  to the raspberry or to the drone.                                    *
*                                                                       *
************************************************************************/

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
using System.IO;
using Android.Util;

namespace BTDronection
{
    public class SocketWriter
    {

        // Constants
        private readonly int PACKET_SIZE = 19;
        private readonly byte START_BYTE = 0x00;

        // Output members
        private DataOutputStream mDataOutputStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BTDronection.SocketWriter"/> class.
        /// </summary>
        /// <param name="outputStream">Output stream</param>
        public SocketWriter(Stream outputStream)
        {
            mDataOutputStream = new DataOutputStream(outputStream);
        }

        /// <summary>
        /// Writes controller data from the smartphone to the Raspberry 
        /// or to the drone.
        /// </summary>
        /// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
        public void Write(params Int16[] args)
        {
            byte[] bytes = ConvertToByte(args);

            try
            {
                mDataOutputStream.Write(bytes);
                mDataOutputStream.Flush();
            }
            catch (Exception ex)
            {
                Log.Debug("BTSocketWriter", "Error while sending data");
            }
        }

        /// <summary>
        /// Writes controller data from the smartphone to the Raspberry 
        /// or to the drone.
        /// </summary>
        /// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
        public void Write(params byte[] args)
        {
            try
            {
                mDataOutputStream.Write(args);
                mDataOutputStream.Flush();
            }
            catch (Exception ex)
            {
                Log.Debug("BTSocketWriter", "Error while sending data");
            }
        }

        /// <summary>
        /// Converts int16 controller parameters to byte stream
        /// </summary>
        /// <param name="args">Controller parameter (throttle, yaw, pitch, roll)</param>
        /// <returns>Byte stream</returns>
        private byte[] ConvertToByte(params Int16[] args)
        {
            byte[] b = new byte[PACKET_SIZE];
            byte speed = (byte)args[0];
            byte heightcontrol = (byte)(ControllerView.Settings.AltitudeControlActivated == true ? 1 : 0);
            int azimuth = Java.Lang.Float.FloatToIntBits(args[1]);
            int pitch = Java.Lang.Float.FloatToIntBits(args[2]);
            int roll = Java.Lang.Float.FloatToIntBits(args[3]);

            int checksum = START_BYTE;
            checksum ^= (heightcontrol << 8 | speed) & 0xFFFF;
            checksum ^= azimuth;
            checksum ^= pitch;
            checksum ^= roll;

            b[0] = START_BYTE;

            b[1] = (byte)(heightcontrol & 0xFF);
            b[2] = (byte)(speed & 0xFF);

            b[3] = (byte)((azimuth >> 24) & 0xFF);
            b[4] = (byte)((azimuth >> 16) & 0xFF);
            b[5] = (byte)((azimuth >> 8) & 0xFF);
            b[6] = (byte)(azimuth & 0xFF);

            b[7] = (byte)((pitch >> 24) & 0xFF);
            b[8] = (byte)((pitch >> 16) & 0xFF);
            b[9] = (byte)((pitch >> 8) & 0xFF);
            b[10] = (byte)(pitch & 0xFF);

            b[11] = (byte)((roll >> 24) & 0xFF);
            b[12] = (byte)((roll >> 16) & 0xFF);
            b[13] = (byte)((roll >> 8) & 0xFF);
            b[14] = (byte)(roll & 0xFF);

            b[15] = (byte)((checksum >> 24) & 0xFF);
            b[16] = (byte)((checksum >> 16) & 0xFF);
            b[17] = (byte)((checksum >> 8) & 0xFF);
            b[18] = (byte)(checksum & 0xFF);
            return b;
        }

        /// <summary>
        /// Closes, if necessary, the output stream.
        /// </summary>
        public void Close()
        {
            if(mDataOutputStream != null)
            {
                mDataOutputStream.Close();
            }
        }
    }
}