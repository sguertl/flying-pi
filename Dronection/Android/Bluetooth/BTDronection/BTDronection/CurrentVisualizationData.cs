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
*  File: CurrentVisualizationData.cs                                    *
*  Created on: 2017-07-28                                               *
*  Author(s): Englert Christoph (IFAT PMM TI COP)                       *
*                                                                       *
*  CurrentVisualizationData is a singleton class to store data from     *
*  a file that is going to be displayed in a graph.                     *
*                                                                       *
************************************************************************/

using System.Collections.Generic;

namespace BTDronection
{
	public class CurrentVisualizationData
	{
		// List of data points
		private Dictionary<string, List<DataPoint>> mPoints;
		public Dictionary<string, List<DataPoint>> Points
		{
			get { return mPoints; }
			set { mPoints = value; }
		}

		// Altitude control points
		private List<float> mAltControlTime;
		public List<float> AltControlTime
		{
			get { return mAltControlTime; }
			set { mAltControlTime = value; }
		}

		/// <summary>
		/// Singleton pattern
		/// </summary>
		private static CurrentVisualizationData instance = null;
		private static readonly object padlock = new object();

		/// <summary>
		/// Private constructor
		/// </summary>
		private CurrentVisualizationData()
		{
			this.mPoints = new Dictionary<string, List<DataPoint>>();
			this.AltControlTime = new List<float>();
		}

		/// <summary>
		/// Returns the instance.
		/// </summary>
		/// <value>Instance of CurrentVisualizationData</value>
		public static CurrentVisualizationData Instance
		{
			get
			{
				lock (padlock)
				{
					if (instance == null)
					{
						instance = new CurrentVisualizationData();
					}
					return instance;
				}
			}
		}
	}
}