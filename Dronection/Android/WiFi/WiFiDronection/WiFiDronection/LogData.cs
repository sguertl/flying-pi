/************************************************************************
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
*  File: LogData.cs                                                     *
*  Created on: 2017-08-01                                               *
*  Author(s): Adrian Klapsch                                            *
*                                                                       *
*  LogData stores a string for CSV file.                                *
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

namespace WiFiDronection
{
    public class LogData
    {
        // Members
        private string mName;
        private string mCsvString;
        private int mBytes;

        // Properties
        public int Bytes
        {
            get { return mBytes; }
            set { mBytes = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WiFiDronection.LogData"/> class.
        /// </summary>
        /// <param name="name">Name of log</param>
        /// <param name="bytes">Bytes of log</param>
        public LogData(string name, int bytes)
        {
            mName = name;
            mBytes = bytes;

            mCsvString = "";
        }

        /// <summary>
        /// Add the specified text.
        /// </summary>
        /// <returns>CSV String</returns>
        /// <param name="text">Text to be added to CSV</param>
        public void Add(string text)
        {
            mCsvString += text + "\n";
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:WiFiDronection.LogData"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:WiFiDronection.LogData"/>.</returns>
        public override string ToString()
        {
            return mCsvString;
        }
    }
}