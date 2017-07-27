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
    public class Joystick
    {
        // -------------------------- CONSTANTS --------------------------------

        private const double RAD = 1 / (2 * Math.PI) * 360; // 1 rad in degrees

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

        public readonly float StickDiameter; // Diameter of the joystick
        public readonly float DisplacementDiameter; // Diameter of the displacement

        public static float StickRadius; // Radius of the joystick
        public static float DisplacementRadius; // Radius of the displacement


        // --------------------------- VARIABLES ------------------------------

        private float mMult = 0.1f;
        private float mMultRudder = 0.4f;
        private float mMultThrottle = 0.7f;

        private float mXPosition; // Current x of joystick
        private float mYPosition; // Current y of joystick
        private readonly bool mLeftStick; // Side of stick
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
        public Int16 Throttle { get { return GetThrottleValue(); } private set { mThrottle = value; } }

        // Rudder value of the stick
        private Int16 mRudder;
        public Int16 Rudder { get { return GetRudderValue(); } private set { mRudder = value; } }

        // Elevator value of the stick
        private Int16 mElevator;
        public Int16 Elevator { get { return GetElevatorValue(); } private set { mElevator = value; } }

        // Aileron value of the stick
        private Int16 mAileron;
        public Int16 Aileron { get { return GetAileronValue(); } private set { mAileron = value; } }

        public Joystick(float width, float height, bool isLeftStick, bool invertedControl)
        {
            StickDiameter = (width / 8 + width / 2) / 2 - width / 5;
            DisplacementDiameter = StickDiameter * 2.25f;

            StickRadius = StickDiameter / 2;
            DisplacementRadius = DisplacementDiameter / 2;

            mCenterY = height / 2; // / 16 + height / 2 + StickRadius / 2;
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
        /// Sets the current position of the joystick
        /// </summary>
        /// <param name="xPosition">X-Position of the joystick</param>
        /// <param name="yPosition">Y-Position of the joystick</param>
        public void SetPosition(float xPosition, float yPosition)
        {
            mXPosition = xPosition;
            mYPosition = yPosition;
        }

        /// <summary>
        /// Returns the current position of the joystick
        /// </summary>
        /// <returns>An array containing the x and y position of the joystick</returns>
        public float[] GetPosition()
        {
            return new float[] { mXPosition, mYPosition };
        }

        /// <summary>
        /// Calculates the angle of the moved joystick
        /// </summary>
        /// <returns>Angle of the joystick</returns>
        private float GetAngle()
        {
            if (mXPosition > mCenterX)
            {
                if (mYPosition < mCenterY)
                {
                    //return mAngle = (int)(Math.Atan((mYPosition - CENTER_Y) / (mXPosition - CENTER_X)) * RAD + 90);
                    return mAngle = (int)(Math.Atan((mYPosition - mCenterY) / (mXPosition - mCenterX)) * RAD + 90) - 90;
                }
                else if (mYPosition > mCenterY)
                {
                    //return mAngle = (int)(Math.Atan((mYPosition - CENTER_Y) / (mXPosition - CENTER_X)) * RAD) + 90;
                    return mAngle = (int)(Math.Atan((mYPosition - mCenterY) / (mXPosition - mCenterX)) * RAD);
                }
                else
                {
                    //return mAngle = 90;
                    return mAngle = 0;
                }
            }
            else if (mXPosition < mCenterX)
            {
                if (mYPosition < mCenterY)
                {
                    //return mAngle = (int)(Math.Atan((mYPosition - CENTER_Y) / (mXPosition - CENTER_X)) * RAD - 90);
                    return mAngle = (int)(Math.Atan((mYPosition - mCenterY) / (mXPosition - mCenterX)) * RAD - 90) - 90;
                }
                else if (mYPosition > mCenterY)
                {
                    //return mAngle = (int)(Math.Atan((mYPosition - CENTER_Y) / (mXPosition - CENTER_X)) * RAD) - 90;
                    return mAngle = (int)(Math.Atan((mYPosition - mCenterY) / (mXPosition - mCenterX)) * RAD) - 180;
                }
                else
                {
                    //return mAngle = -90;
                    return mAngle = -180;
                }
            }
            else
            {
                if (mYPosition <= mCenterY)
                {
                    //return mAngle = 0;
                    return mAngle = -90;
                }
                else
                {
                    if (mAngle < 0)
                    {
                        //return mAngle = -180;
                        return mAngle = -270;
                    }
                    else
                    {
                        //return mAngle = 180;
                        return mAngle = 90;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the direction in which the joystick was moved
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
        /// Calculates the power of the joystick
        /// </summary>
        /// <returns>Power of the joystick in percent (max. 100)</returns>
        private int GetPower()
        {
            mPower = (int)(100 * Math.Sqrt(
                (mXPosition - mCenterX) * (mXPosition - mCenterX) +
                (mYPosition - mCenterY) * (mYPosition - mCenterY)) / (DisplacementRadius));
            mPower = Math.Min(mPower, 100);
            return mPower;
        }

        /// <summary>
        /// Calculates the length of the vector
        /// </summary>
        /// <returns>Length of the vector</returns>
        private float GetAbs()
        {
            return mAbs = (float)Math.Sqrt((mXPosition - mCenterX) * (mXPosition - mCenterX) + (mYPosition - mCenterY) * (mYPosition - mCenterY));
        }

        /// <summary>
        /// Calculates the throttle value of the stick
        /// </summary>
        /// <returns>Throttle value (between 0 and 32767)</returns>
        private Int16 GetThrottleValue()
        {
            int throttleValue = 0;
            if (mYPosition > mCenterY + DisplacementRadius)
            {
                return (Int16)throttleValue;
            }
            //throttleValue = (int)(40 * (mCenterY + DisplacementRadius - mYPosition) / DisplacementDiameter);
            //throttleValue = Math.Max((Int16)0, throttleValue);
            //throttleValue = Math.Min((Int16)40, throttleValue);
            //throttleValue = (int)(32767 * (mCenterY + DisplacementRadius - mYPosition) / DisplacementDiameter);
            throttleValue = (int)(90 * (mCenterY + DisplacementRadius - mYPosition) / DisplacementDiameter);
            throttleValue = Math.Max((Int16)0, throttleValue);
            //throttleValue = Math.Min((Int16)32767, throttleValue);
            throttleValue = Math.Min((Int16)255, throttleValue);
            Throttle = (Int16)throttleValue;
            //Console.WriteLine("****************"+mThrottle);
            return (Int16)((mThrottle) * mMultThrottle);
        }

        /// <summary>
        /// Calculates the rudder value of the stick
        /// </summary>
        /// <returns>Rudder value (between -32768 and 32767)</returns>
        private Int16 GetRudderValue()
        {
            //int rudderValue = -32768;
            int rudderValue = -90;
            /* if (mXPosition < mCenterX - DisplacementRadius)
             {
                 return (Int16)rudderValue;
             }*/
            //rudderValue = (int)((65536 * (mCenterX + DisplacementRadius - mXPosition) / DisplacementDiameter) - 32768) * (-1);
            rudderValue = (int)((180 * (mCenterX + DisplacementRadius - mXPosition) / DisplacementDiameter) - 90) * (-1);
            //rudderValue = Math.Max(-32768, rudderValue);
            rudderValue = Math.Max(-90, rudderValue);
            //rudderValue = Math.Min(32767, rudderValue);
            rudderValue = Math.Min(90, rudderValue);
            mRudder = (Int16)rudderValue;
            return (Int16)((mRudder * -1) * mMultRudder);
        }

        /// <summary>
        /// Calculates the elevator value of the stick
        /// </summary>
        /// <returns>Elevator value (between -32768 and 32767)</returns>
        private Int16 GetElevatorValue()
        {
            //int elevatorValue = -32768;
            int elevatorValue = -100;
            /*  if (mYPosition > mCenterY + DisplacementRadius)
              {
                  return (Int16)elevatorValue;
              }*/
            elevatorValue = (int)((200 * (mCenterY + DisplacementRadius - mYPosition) / DisplacementDiameter) - 100) * (-1);
            // elevatorValue = Math.Max(0, elevatorValue);
            // elevatorValue = Math.Min(40, elevatorValue);
            //elevatorValue = (int)(65536 * (mCenterY + DisplacementRadius - mYPosition) / DisplacementDiameter) - 32768;
            //   elevatorValue = (int)(200 * (mCenterY + DisplacementRadius - mYPosition) / DisplacementDiameter) - 100;
            //elevatorValue = Math.Max(-32768, elevatorValue);
            elevatorValue = Math.Max(-100, elevatorValue);
            //elevatorValue = Math.Min(32767, elevatorValue);
            elevatorValue = Math.Min(100, elevatorValue);
            mElevator = (Int16)elevatorValue;
            return (Int16)((mElevator * 1) * mMult);
        }

        /// <summary>
        /// Calculates the aileron value of the stick
        /// </summary>
        /// <returns>Aileron value (between -32768 and 32767)</returns>
        private Int16 GetAileronValue()
        {
            //int aileronValue = -32768;
            int aileronValue = -100;
            /*   if (mXPosition < mCenterX - DisplacementRadius)
               {
                   //Fehler -100

                   return (Int16)aileronValue;
               }*/
            //aileronValue = (int)((65536 * (mCenterX + DisplacementRadius - mXPosition) / DisplacementDiameter) - 32768) * (-1);
            aileronValue = (int)((200 * (mCenterX + DisplacementRadius - mXPosition) / DisplacementDiameter) - 100) * (-1);
            //aileronValue = Math.Max(-32768, aileronValue);
            aileronValue = Math.Max(-100, aileronValue);
            //aileronValue = Math.Min(32767, aileronValue);
            aileronValue = Math.Min(100, aileronValue);
            mAileron = (Int16)aileronValue;
            return (Int16)((mAileron * -1) * mMult);
        }

        /// <summary>
        /// Helper method which calls GetPower(), GetAngle() and GetAbs()
        /// </summary>
        public void CalculateValues()
        {
            GetPower();
            GetAngle();
            GetAbs();
        }

        /// <summary>
        /// Checks if stick is currently centered
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