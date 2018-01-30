﻿/************************************************************************
*                                                                       *
*  Copyright (C) 2017-2018 Infineon Technologies Austria AG.            *
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
*  File: SocketReader.cs                                                *
*  Created on: 2017-07-26                                               *
*  Author(s): Adrian Klapsch                                            *
*                                                                       *
*  SocketReader reads data which is sent by the Raspberry.              *
*                                                                       *
************************************************************************/

using System;
using System.Linq;

using Java.Lang;
using Java.IO;
using Android.Util;
using System.Collections.Generic;
using System.IO;

namespace WiFiDronection
{
    public class SocketReader
    {
		
        private static readonly string TAG = "SocketReader";

        // Input stream
        private DataInputStream mDataInputStream;
        private RaspberryClose mRaspberryCloseEvent;
        private bool isReading;

        private string mCurrentMsg;

        public string CurrentMsg
        {
            get { return mCurrentMsg; }
            set { mCurrentMsg = value; }
        }

        private Dictionary<string, LogData> mDroneLogs;

        public Dictionary<string, LogData> DroneLogs
        {
            get { return mDroneLogs; }
            set { mDroneLogs = value; }
        }


        // Data reader thread
        public Thread mDataReaderThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WiFiDronection.SocketReader"/> class.
        /// </summary>
        /// <param name="inputStream">Input stream of socket connection</param>
        /// <param name="rpiClose">Delegate for closing the socket connection</param>
        public SocketReader(DataInputStream inputStream, RaspberryClose rpiClose)
        {
            mDataInputStream = inputStream;
            mRaspberryCloseEvent = rpiClose;
            mDroneLogs = new Dictionary<string, LogData>();
            mDataReaderThread = new Thread(OnRead);
            isReading = false;
        }

        /// <summary>
        /// Reads data from Raspberry in a thread.
        /// </summary>
        private void OnRead()
        {
            int bytes = 0;
            byte[] buffer = new byte[41];
            int count = 0;
            isReading = true;
            while (true)
            {
                try
                {
                    bytes = mDataInputStream.Read(buffer);
                    //Log.Debug("???", buffer[1].ToString());
                    string msg = "";
                    if(buffer[0] != 1 && buffer[0] != 99)
                    { 
                        bool isRight = ControlChecksum(buffer);
                        if(isRight == true)
                        {
                            string line = "";
                            if(mDroneLogs.Keys.Contains("ControlsDrone") == true)
                            {
                                byte altitudeControl = buffer[1];
                                byte speed = buffer[2];
                                int rawRudder = (buffer[6] & 0xFF) | ((buffer[5] & 0xFF) << 8) | ((buffer[4] & 0xFF) << 16) | ((buffer[3] & 0xFF) << 24);
                                float rudder = Float.IntBitsToFloat(rawRudder);
                                int rawAileron = (buffer[10] & 0xFF) | ((buffer[9] & 0xFF) << 8) | ((buffer[8] & 0xFF) << 16) | ((buffer[7] & 0xFF) << 24);
                                float aileron = Float.IntBitsToFloat(rawAileron);
                                int rawElevator = (buffer[14] & 0xFF) | ((buffer[13] & 0xFF) << 8) | ((buffer[12] & 0xFF) << 16) | ((buffer[11] & 0xFF) << 24);
                                float elevator = Float.IntBitsToFloat(rawElevator);

                                line = count + ";" + altitudeControl + ";" + speed + ";" + rudder + ";" + aileron + ";" + elevator;
                                mDroneLogs["ControlsDrone"].Add(line);
                            }

                            if(mDroneLogs.Keys.Contains("CollisionStatus") == true)
                            {
                                byte collisionStatus = buffer[35];
                                line = count + ";" + collisionStatus.ToString();
                                mDroneLogs["CollisionStatus"].Add(line);
                            }

                            if(mDroneLogs.Keys.Contains("Radar") == true)
                            {
                                int rawRadar = (buffer[18] & 0xFF) | ((buffer[17] & 0xFF) << 8) | ((buffer[16] & 0xFF) << 16) | ((buffer[15] & 0xFF) << 24);
                                float radar = Float.IntBitsToFloat(rawRadar);
                                line = count + ";" + radar.ToString();
                                Log.Debug("!!!", line);
                                mDroneLogs["Radar"].Add(radar.ToString());
                            }

                            if(mDroneLogs.Keys.Contains("Debug1") == true)
                            {
                                int rawDebug = (buffer[22] & 0xFF) | ((buffer[21] & 0xFF) << 8) | ((buffer[20] & 0xFF) << 16) | ((buffer[19] & 0xFF) << 24);
                                float debug = Float.IntBitsToFloat(rawDebug);
                                line = count + ";" + debug.ToString();
                                mDroneLogs["Debug1"].Add(line);
                            }

                            if (mDroneLogs.Keys.Contains("Debug2") == true)
                            {
                                int rawDebug = (buffer[26] & 0xFF) | ((buffer[25] & 0xFF) << 8) | ((buffer[24] & 0xFF) << 16) | ((buffer[23] & 0xFF) << 24);
                                float debug = Float.IntBitsToFloat(rawDebug);
                                line = count + ";" + debug.ToString();
                                mDroneLogs["Debug2"].Add(line);
                            }

                            if (mDroneLogs.Keys.Contains("Debug3") == true)
                            {
                                int rawDebug = (buffer[30] & 0xFF) | ((buffer[29] & 0xFF) << 8) | ((buffer[28] & 0xFF) << 16) | ((buffer[27] & 0xFF) << 24);
                                float debug = Float.IntBitsToFloat(rawDebug);
                                line = count + ";" + debug.ToString();
                                mDroneLogs["Debug3"].Add(line);
                            }

                            if (mDroneLogs.Keys.Contains("Debug4") == true)
                            {
                                int rawDebug = (buffer[34] & 0xFF) | ((buffer[33] & 0xFF) << 8) | ((buffer[32] & 0xFF) << 16) | ((buffer[31] & 0xFF) << 24);
                                float debug = Float.IntBitsToFloat(rawDebug);
                                line = count + ";" + debug.ToString();
                                mDroneLogs["Debug4"].Add(line);
                            }
                        }
                        count += 10;
                    }
                    else
                    {
                        msg = buffer[1].ToString();
                        mCurrentMsg = msg;
                        if(msg == "99")
                        {
                            break;
                        }
                    }
                }
                catch (Java.IO.IOException ex)
                {
                    Log.Debug(TAG, "Error reading (" + ex.Message + ")1");
                    break;
                }
                catch (NullReferenceException ex)
                {
                    Log.Debug(TAG, "No socket connection (" + ex.Message + ")2");
                    // throw new NullReferenceException();
                    break;
                }
                catch(Java.Lang.StringIndexOutOfBoundsException ex)
                {
                    Log.Debug(TAG, "Connection was interrupted by RPI");
                    break;
                }
            }
            isReading = false;
            mRaspberryCloseEvent();
        }

        private bool ControlChecksum(byte[] buffer)
        {
            int calculatedChecksum = buffer[0];
            calculatedChecksum ^= ((buffer[1] << 8 | buffer[2]) & 0xFFFF);
            for(int i = 3; i < 35; i += 4)
            {
                calculatedChecksum ^= (buffer[i] << 24 | buffer[i + 1] << 16 | buffer[i + 2] << 8 | buffer[i + 3]);
            }

            calculatedChecksum ^= buffer[35];
            calculatedChecksum ^= buffer[36];

            int incomingChecksum = (buffer[37] << 24 | buffer[38] << 16 | buffer[39] << 8 | buffer[40]);
            return calculatedChecksum == incomingChecksum;
        }

        /// <summary>
        /// Creates and starts the thread. 
        /// </summary>
        public void StartListening()
        {
            mDataReaderThread.Start();
        }

        public void StopListening()
        {
            if (mDataReaderThread != null)
            {
                if (isReading == true)
                {
                    mDataReaderThread.Join();
                }
                mDataReaderThread = null;
            }
        }

        /// <summary>
        /// Closes everything related to the socket connection.
        /// </summary>
        public void Close()
        {
            try
            {
                /*if (mDataReaderThread != null)
                {
                    if(isReading == true)
                    {
                        mDataReaderThread.Join();
                    }
                    mDataReaderThread = null;
                }*/

                if (mDataInputStream != null)
                {
                    mDataInputStream.Close();
                }
            }
			catch(Java.Lang.Exception ex)
            {
                Log.Debug(TAG, ex.Message);
            }
        }
    }
}