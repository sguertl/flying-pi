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
*  File: DataPoint.cs                                                   *
*  Created on: 2017-08-04                                               *
*  Author(s): Englert Christoph (IFAT PMM TI COP)                       *
*                                                                       *
*  Flight contains an instance of ControllerView.                       *
*                                                                       *
************************************************************************/

namespace WiFiDronection
{
	public class Flight
	{

        /// <summary>
        /// Singleton members
        /// </summary>
		private static Flight instance = null;
		private static readonly object padlock = new object();

        /// <summary>
        /// Instance of ControllerView.
        /// </summary>
		private ControllerView mCV;
		public ControllerView CV
		{
			get { return mCV; }
			set { mCV = value; }
		}

        /// <summary>
        /// Private singleton constructor
        /// </summary>
		private Flight()
		{

		}

        /// <summary>
        /// Returns an instance of Flight.
        /// </summary>
        /// <value>Instance of Flight</value>
		public static Flight Instance
		{
			get
			{
				lock (padlock)
				{
					if (instance == null)
					{
						instance = new Flight();
					}
					return instance;
				}
			}
		}
	}
}