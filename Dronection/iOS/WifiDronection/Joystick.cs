﻿﻿/************************************************************************
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
*  File: Joystick.cs                                                    *
*  Created on: 2017-07-19                                               *
*  Author(s): Guertl Sebastian Matthias (IFAT PMM TI COP)               *
*                                                                       *
*  Joystick provides the functionality of a hardware joystick           *
*  on a RC.                                                             *
*                                                                       *
************************************************************************/

using System;

namespace WiFiDronection
{
	public class Joystick
	{
		// Constant value to convert 1 rad in degrees
		private const double RAD = 1 / (2 * Math.PI) * 360;

		// Constant direction values
		public static readonly int CENTER = 0;
		public static readonly int BOTTOM = 1;
		public static readonly int BOTTOM_RIGHT = 2;
		public static readonly int RIGHT = 3;
		public static readonly int TOP_RIGHT = 4;
		public static readonly int TOP = 5;
		public static readonly int TOP_LEFT = 6;
		public static readonly int LEFT = 7;
		public static readonly int BOTTOM_LEFT = 8;
		public static readonly int LEFT_STICK = 0;
		public static readonly int RIGHT_STICK = 1;

		// Diameter of the joystick
		public readonly float StickDiameter;
		// Diameter of the displacement
		public readonly float DisplacementDiameter;

		// Radius of the joystick
		public static float StickRadius;
		// Radius of the displacement
		public static float DisplacementRadius;


		// Multiplier for joystick sensitivity (the higher the value, the more sensitive)
		// Mutliplier for Elevator and Aileron
		private float mMult = 0.15f;
		// Multiplier for Rudder
		private float mMultRudder = 0.4f;
		// Multiplier for throttle
		private float mMultThrottle = 1f;

		// Current x-position of joystick
		private float mXPosition;
		// Current y-position of joystick
		private float mYPosition;
		// Side of stick
		private readonly bool mLeftStick;
		// Control mode
		private readonly bool mInverted; // Control mode

		// Center x of joystick
		private float mCenterX;
		public float CenterX { get { return mCenterX; } private set { mCenterX = value; } }

		// Center y of joystick
		private float mCenterY;
		public float CenterY { get { return mCenterY; } private set { mCenterY = value; } }

		// Current power (= displacement) of the joystick; maximum is 1 (= 100 %)
		private int mPower;
		public int Power { get { return GetPower(); } private set { mPower = value; } }

		// Current angle of the joystick
		private float mAngle;
		public float Angle { get { return GetAngle(); } private set { mAngle = value; } }

		// Current direction of the joystick
		private int mDirection;
		public int Direction { get { return GetDirection(); } private set { mDirection = value; } }

		// Vector length
		private float mAbs;
		public float Abs { get { return GetAbs(); } private set { mAbs = value; } }

		// Throttle value of the stick
		private Int16 mThrottle;
		public Int16 Throttle { get { return GetThrottleValue(); } set { mThrottle = value; } }

		// Rudder value of the stick
		private Int16 mRudder;
		public Int16 Rudder { get { return GetRudderValue(); } private set { mRudder = value; } }

		// Elevator value of the stick
		private Int16 mElevator;
		public Int16 Elevator { get { return GetElevatorValue(); } private set { mElevator = value; } }

		// Aileron value of the stick
		private Int16 mAileron;
		public Int16 Aileron { get { return GetAileronValue(); } private set { mAileron = value; } }

		/// <summary>
		/// Creates new Joystick object and sets stick diameter, 
		/// displacement diameter and center of the joystick.
		/// </summary>
		/// <param name="width">Width of screen</param>
		/// <param name="height">Height of screen</param>
		/// <param name="isLeftStick">Left or right positioned joystick</param>
		/// <param name="invertedControl">Mode of controller</param>
		public Joystick(float width, float height, bool isLeftStick, bool invertedControl)
		{
			StickDiameter = (width / 8 + width / 2) / 2 - width / 5;
			DisplacementDiameter = StickDiameter * 2.25f;

			StickRadius = StickDiameter / 2;
			DisplacementRadius = DisplacementDiameter / 2;

            mCenterY = height / 8 + height / 2;

			if (!invertedControl)
			{
				if (isLeftStick)
				{
					mCenterX = width / 5 + StickRadius / 2;
					SetPosition(mCenterX, mCenterY + DisplacementRadius);
				}
				else
				{
					mCenterX = width - width / 5 - StickRadius / 2;
					SetPosition(mCenterX, mCenterY);
				}
			}
			else
			{
				if (isLeftStick)
				{
					mCenterX = width / 5 + StickRadius / 2;
					SetPosition(mCenterX, mCenterY);
				}
				else
				{
					mCenterX = width - width / 5 - StickRadius / 2;
					SetPosition(mCenterX, mCenterY + DisplacementRadius);
				}
			}
			mLeftStick = isLeftStick;
			mInverted = invertedControl;
		}

		/// <summary>
		/// Sets the current position of the joystick.
		/// </summary>
		/// <param name="xPosition">X-Position of the joystick</param>
		/// <param name="yPosition">Y-Position of the joystick</param>
		public void SetPosition(float xPosition, float yPosition)
		{
			mXPosition = xPosition;
			mYPosition = yPosition;
		}

		/// <summary>
		/// Returns the current position of the joystick.
		/// </summary>
		/// <returns>An array containing the x and y position of the joystick</returns>
		public float[] GetPosition()
		{
			return new float[] { mXPosition, mYPosition };
		}

		/// <summary>
		/// Calculates the angle of the moved joystick.
		/// </summary>
		/// <returns>Angle of the joystick</returns>
		private float GetAngle()
		{
			if (mXPosition > mCenterX)
			{
				if (mYPosition < mCenterY)
				{
					return mAngle = (int)(Math.Atan((mYPosition - mCenterY) / (mXPosition - mCenterX)) * RAD + 90) - 90;
				}
				else if (mYPosition > mCenterY)
				{
					return mAngle = (int)(Math.Atan((mYPosition - mCenterY) / (mXPosition - mCenterX)) * RAD);
				}
				else
				{
					return mAngle = 0;
				}
			}
			else if (mXPosition < mCenterX)
			{
				if (mYPosition < mCenterY)
				{
					return mAngle = (int)(Math.Atan((mYPosition - mCenterY) / (mXPosition - mCenterX)) * RAD - 90) - 90;
				}
				else if (mYPosition > mCenterY)
				{
					return mAngle = (int)(Math.Atan((mYPosition - mCenterY) / (mXPosition - mCenterX)) * RAD) - 180;
				}
				else
				{
					return mAngle = -180;
				}
			}
			else
			{
				if (mYPosition <= mCenterY)
				{
					return mAngle = -90;
				}
				else
				{
					if (mAngle < 0)
					{
						return mAngle = -270;
					}
					else
					{
						return mAngle = 90;
					}
				}
			}
		}

		/// <summary>
		/// Calculates the direction in which the joystick was moved.
		/// </summary>
		/// <returns>Direction of the joystick</returns>
		private int GetDirection()
		{
			if ((int)mCenterX == (int)mXPosition && (int)mCenterY == (int)mYPosition)
			{
				return mDirection = 0;
			}
			if (mPower == 0 && (int)mAngle == 0)
			{
				return mDirection = 0;
			}
			int a = 0;
			if (mAngle <= 0)
			{
				a = ((int)mAngle * -1) + 90;
			}
			else if (mAngle > 0)
			{
				if (mAngle <= 90)
				{
					a = 90 - (int)mAngle;
				}
				else
				{
					a = 360 - ((int)mAngle - 90);
				}
			}

			mDirection = ((a + 22) / 45) + 1;

			if (mDirection > 8)
			{
				mDirection = 1;
			}
			return mDirection;
		}

		/// <summary>
		/// Calculates the power of the joystick.
		/// </summary>
		/// <returns>Power of the joystick in percent (between 0 and 100)</returns>
		private int GetPower()
		{
			mPower = (int)(100 * Math.Sqrt(
				(mXPosition - mCenterX) * (mXPosition - mCenterX) +
				(mYPosition - mCenterY) * (mYPosition - mCenterY)) / (DisplacementRadius));
			mPower = Math.Min(mPower, 100);
			return mPower;
		}

		/// <summary>
		/// Calculates the length of the vector.
		/// </summary>
		/// <returns>Length of the vector</returns>
		private float GetAbs()
		{
			return mAbs = (float)Math.Sqrt((mXPosition - mCenterX) * (mXPosition - mCenterX) + (mYPosition - mCenterY) * (mYPosition - mCenterY));
		}

		/// <summary>
		/// Calculates the throttle value of the stick.
		/// </summary>
		/// <returns>Throttle value (between 0 and 32767)</returns>
		private Int16 GetThrottleValue()
		{
			int throttleValue = 0;
			if (mYPosition > mCenterY + DisplacementRadius)
			{
				return (Int16)throttleValue;
			}
			throttleValue = (int)(90 * (mCenterY + DisplacementRadius - mYPosition) / DisplacementDiameter);
			throttleValue = Math.Max((Int16)0, throttleValue);
			throttleValue = Math.Min((Int16)255, throttleValue);
			Throttle = (Int16)throttleValue;
			return (Int16)((mThrottle) * mMultThrottle);
		}

		/// <summary>
		/// Calculates the rudder (yaw) value of the stick.
		/// </summary>
		/// <returns>Rudder value</returns>
		private Int16 GetRudderValue()
		{
            int minValue = -15; // ControllerView.Settings.MinYaw;
            int maxValue = 15; // ControllerView.Settings.MaxYaw;
			int rudderValue = -minValue;
			rudderValue = (int)(((Math.Abs(minValue) + Math.Abs(maxValue)) * (mCenterX + DisplacementRadius - mXPosition) / DisplacementDiameter) - maxValue) * (-1);
			rudderValue = Math.Max(minValue, rudderValue);
			rudderValue = Math.Min(maxValue, rudderValue);
			mRudder = (Int16)rudderValue;
			return (Int16)(mRudder * -1);
		}

		/// <summary>
		/// Calculates the elevator (pitch) value of the stick.
		/// </summary>
		/// <returns>Elevator value</returns>
		private Int16 GetElevatorValue()
		{
            int minValue = -20; // ControllerView.Settings.MinPitch;
			int maxValue = 20; // ControllerView.Settings.MaxPitch;
			int elevatorValue = minValue;
			elevatorValue = (int)(((Math.Abs(minValue) + Math.Abs(maxValue)) * (mCenterY + DisplacementRadius - mYPosition) / DisplacementDiameter) - maxValue) * (-1);
			elevatorValue = Math.Max(minValue, elevatorValue);
			elevatorValue = Math.Min(maxValue, elevatorValue);
			mElevator = (Int16)elevatorValue;
			return (Int16)(mElevator);
		}

		/// <summary>
		/// Calculates the aileron (roll) value of the stick.
		/// </summary>
		/// <returns>Aileron value</returns>
		private Int16 GetAileronValue()
		{
            int minValue = -20; // ControllerView.Settings.MinRoll;
            int maxValue = 20; // ControllerView.Settings.MaxRoll;
			int aileronValue = minValue;
			aileronValue = (int)(((Math.Abs(minValue) + Math.Abs(maxValue)) * (mCenterX + DisplacementRadius - mXPosition) / DisplacementDiameter) - maxValue) * (-1);
			aileronValue = Math.Max(minValue, aileronValue);
			aileronValue = Math.Min(maxValue, aileronValue);
			mAileron = (Int16)aileronValue;
			return (Int16)(mAileron * -1);
		}

		/// <summary>
		/// Calls GetPower(), GetAngle() and GetAbs().
		/// </summary>
		public void CalculateValues()
		{
			GetPower();
			GetAngle();
			GetAbs();
		}

		/// <summary>
		/// Checks if stick is currently centered.
		/// </summary>
		/// <returns>True if stick is centered, false if not</returns>
		public bool IsCentered()
		{
			if (mInverted)
			{
				if (mLeftStick)
				{
					return (int)mXPosition == (int)mCenterX && (int)mYPosition == (int)mCenterY;
				}
				else
				{
					return (int)mXPosition == (int)mCenterX && (int)mYPosition == (int)(mCenterY + DisplacementRadius);
				}
			}
			else
			{
				if (mLeftStick)
				{
					return (int)mXPosition == (int)mCenterX && (int)mYPosition == (int)(mCenterY + DisplacementRadius);
				}
				else
				{
					return (int)mXPosition == (int)mCenterX && (int)mYPosition == (int)mCenterY;
				}
			}
		}
	}
}
