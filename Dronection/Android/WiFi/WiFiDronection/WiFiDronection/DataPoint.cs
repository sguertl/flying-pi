/************************************************************************
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
*  File: DataPoint.cs														*
*  Created on: 2017-8-1				*
*  Author(s): Guertl Sebastian Matthias (IFAT PMM TI COP)											*
*																		*
*  <Summary>															*
*																		*
************************************************************************/
using System;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WiFiDronection.DataPoint"/> class.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public DataPoint(float x, float y)
        {
            this.mX = x;
            this.mY = y;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:WiFiDronection.DataPoint"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:WiFiDronection.DataPoint"/>.</returns>
        public override string ToString()
        {
            return String.Format("[{0}/{1}]", mX, mY);
        }
    }
}