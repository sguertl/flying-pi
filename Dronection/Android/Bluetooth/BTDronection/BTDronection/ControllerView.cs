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
*  File: ControllerView.cs                                              *
*  Created on: 2017-07-19                                               *
*  Author(s): Sebastian Guertl                                          *
*             Adrian Klapsch                                            *
*                                                                       *
*  ControllerView is mainly responsible for drawing the joysticks       *
*  and transferring the resulting data to the recipient.                *
*                                                                       *
************************************************************************/

using System;

using Android.Content;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Graphics.Drawables.Shapes;
using Android.Util;

namespace BTDronection
{
    public class ControllerView : View, View.IOnTouchListener
    {
        // Controller Settings
        //ControllerSettings mSettings;

        /// <summary>
        /// The width of the screen in px
        /// </summary>
        public float ScreenWidth;
        /// <summary>
        /// The height of the screen in px
        /// </summary>
        public float ScreenHeight;

        // Joystick ovals
        private ShapeDrawable mShapeStickLeft;
        private ShapeDrawable mShapeStickRight;

        // Displacement ovals
        private ShapeDrawable mShapeRadiusLeft;
        private ShapeDrawable mShapeRadiusRight;

        // Joystick border
        private ShapeDrawable mShapeBorderStickLeft;
        private ShapeDrawable mShapeBorderStickRight;

        // Displacement border
        private ShapeDrawable mShapeBorderRadiusLeft;
        private ShapeDrawable mShapeBorderRadiusRight;

        /// <summary>
        /// Left joystick
        /// </summary>
        public Joystick mLeftJS;
        /// <summary>
        /// Right joystick
        /// </summary>
        public Joystick mRightJS;

        /// <summary>
        /// Socket to transfer data via wifi
        /// </summary>
        //private SocketConnection mSocket;
        private SocketConnection mSocket;

        public void SetSocket(SocketConnection conn)
        {
            mSocket = conn;
        }

        /// <summary>
        /// Timer for sending data and checking Wifi connection
        /// </summary>
        private System.Timers.Timer mWriteTimer;
        public System.Timers.Timer WriteTimer
        {
            get { return mWriteTimer; }
            set { mWriteTimer = value; }
        }

        /// <summary>
        /// Instance of ControllerSettings
        /// </summary>
        public static ControllerSettings Settings { get; set; }

        /// <summary>
        /// View constructor.
        /// </summary>
        /// <param name="context"></param>
        public ControllerView(Context context) : base(context)
        {
            Init();
        }

        /// <summary>
        /// View constructor.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attrs"></param>
        public ControllerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        /// <summary>
        /// View constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attrs"></param>
        /// <param name="defStyle"></param>
        public ControllerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init();
        }

        /// <summary>
        /// Initalizes members and creates drawable shapes.
        /// Sets the write timer interval.
        /// </summary>
        private void Init()
        {
            // Initialize ControllerSettings
			Settings = new ControllerSettings
            {
                AltitudeControlActivated = false,
                Inverted = ControllerActivity.Inverted,
                TrimPitch = 0,
                TrimRoll = 0,
                TrimYaw = 0
            };

            SetOnTouchListener(this);
            SetBackgroundColor(Color.White);

            // Get screen resolution
            ScreenWidth = Resources.DisplayMetrics.WidthPixels;
            ScreenHeight = Resources.DisplayMetrics.HeightPixels;


            InitShapes();
            InitJoysticks();

            this.SetBackgroundResource(Resource.Drawable.bg);

            // Initialize timer
            mWriteTimer = new System.Timers.Timer();
            mWriteTimer.Interval = 50;//10
            mWriteTimer.AutoReset = true;
            mWriteTimer.Elapsed += Write;
            mWriteTimer.Start();
        }

		/// <summary>
		/// Initializes the shapes for the stick and the radius.
		/// </summary>
		private void InitShapes()
		{
			// Paint for joystick ovals
			var paintStick = new Paint();
			paintStick.Color = Color.ParseColor("#644F54");
			paintStick.SetStyle(Paint.Style.Fill);
			// Shape for left joystick
			mShapeStickLeft = new ShapeDrawable(new OvalShape());
			mShapeStickLeft.Paint.Set(paintStick);
			// Shape for right joystick
			mShapeStickRight = new ShapeDrawable(new OvalShape());
			mShapeStickRight.Paint.Set(paintStick);

			// Paint for displacement ovals
			var paintRadius = new Paint();
			paintRadius.Color = Color.ParseColor("#e9e6cd");
			//paintRadius.SetARGB(255, 230, 230, 230);
			//paintRadius.SetStyle(Paint.Style.Fill);
			paintRadius.SetStyle(Paint.Style.Fill);
			// Shape for left displacement 
			mShapeRadiusLeft = new ShapeDrawable(new OvalShape());
			mShapeRadiusLeft.Paint.Set(paintRadius);
			// Shape for right displacement
			mShapeRadiusRight = new ShapeDrawable(new OvalShape());
			mShapeRadiusRight.Paint.Set(paintRadius);

			// Paint for border ovals
			var paintBorder = new Paint();
			paintBorder.SetARGB(255, 44, 44, 44);
			paintStick.SetStyle(Paint.Style.Fill);
			// Shape for left joystick border
			mShapeBorderStickLeft = new ShapeDrawable(new OvalShape());
			mShapeBorderStickLeft.Paint.Set(paintBorder);
			// Shape for right joystick border
			mShapeBorderStickRight = new ShapeDrawable(new OvalShape());
			mShapeBorderStickRight.Paint.Set(paintBorder);
			// Shape for left displacement border
			mShapeBorderRadiusLeft = new ShapeDrawable(new OvalShape());
			mShapeBorderRadiusLeft.Paint.Set(paintBorder);
			// Shape for right displacement border
			mShapeBorderRadiusRight = new ShapeDrawable(new OvalShape());
			mShapeBorderRadiusRight.Paint.Set(paintBorder);
		}

		/// <summary>
		/// Sets the bounds for every joystick and displacement oval.
		/// </summary>
		private void InitJoysticks()
		{
			mLeftJS = new Joystick(ScreenWidth, ScreenHeight, true, Settings.Inverted);
			mRightJS = new Joystick(ScreenWidth, ScreenHeight, false, Settings.Inverted);

			SetBoundsForLeftStick(
				(int)mLeftJS.CenterX - (int)Joystick.StickRadius,
				Settings.Inverted ? (int)mLeftJS.CenterY - (int)Joystick.StickRadius : (int)mLeftJS.CenterY + (int)Joystick.StickRadius,
				(int)mLeftJS.CenterX + (int)Joystick.StickRadius,
				Settings.Inverted ? (int)mLeftJS.CenterY + (int)Joystick.StickRadius : (int)mLeftJS.CenterY + 3 * (int)Joystick.StickRadius);

			SetBoundsForRightStick(
				(int)mRightJS.CenterX - (int)Joystick.StickRadius,
				Settings.Inverted ? (int)mRightJS.CenterY + (int)Joystick.StickRadius : (int)mRightJS.CenterY - (int)Joystick.StickRadius,
				(int)mRightJS.CenterX + (int)Joystick.StickRadius,
				Settings.Inverted ? (int)mRightJS.CenterY + 3 * (int)Joystick.StickRadius : (int)mRightJS.CenterY + (int)Joystick.StickRadius);

			mShapeRadiusLeft.SetBounds(
				(int)mLeftJS.CenterX - (int)Joystick.DisplacementRadius,
				(int)mLeftJS.CenterY - (int)Joystick.DisplacementRadius,
				(int)mLeftJS.CenterX + (int)Joystick.DisplacementRadius,
				(int)mLeftJS.CenterY + (int)Joystick.DisplacementRadius);

			mShapeRadiusRight.SetBounds(
				(int)mRightJS.CenterX - (int)Joystick.DisplacementRadius,
				(int)mRightJS.CenterY - (int)Joystick.DisplacementRadius,
				(int)mRightJS.CenterX + (int)Joystick.DisplacementRadius,
				(int)mRightJS.CenterY + (int)Joystick.DisplacementRadius);

			mShapeBorderRadiusLeft.SetBounds(
				(int)mLeftJS.CenterX - (int)Joystick.DisplacementRadius - 2,
				(int)mLeftJS.CenterY - (int)Joystick.DisplacementRadius - 2,
				(int)mLeftJS.CenterX + (int)Joystick.DisplacementRadius + 2,
				(int)mLeftJS.CenterY + (int)Joystick.DisplacementRadius + 2);

			mShapeBorderRadiusRight.SetBounds(
				(int)mRightJS.CenterX - (int)Joystick.DisplacementRadius - 2,
				(int)mRightJS.CenterY - (int)Joystick.DisplacementRadius - 2,
				(int)mRightJS.CenterX + (int)Joystick.DisplacementRadius + 2,
				(int)mRightJS.CenterY + (int)Joystick.DisplacementRadius + 2);
		}

		/// <summary>
		/// Checks single or multitouch and sets new bounds.
		/// </summary>
		public bool OnTouch(View v, MotionEvent e)
		{
			switch (e.Action)
			{
				case MotionEventActions.Up:
                    ResetJoystickPosition(e.GetX());
					break;
				case MotionEventActions.Pointer1Up:
                        ResetJoystickPosition(e.GetX(0));                    
					break;
				case MotionEventActions.Pointer2Up:
                        ResetJoystickPosition(e.GetX(1));
                    break;
				default:
					for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
					{
						UpdateOvals(e.GetX(i), e.GetY(i));
					}
					break;
			}

            //if (Settings.Inverted)
            //{
            //	if (e.PointerCount == 1 && e.GetX() <= ScreenWidth / 2 && !mRightJS.IsCentered())
            //	{
            //                 if (Settings.AltitudeControlActivated)
            //                 {
            //                     UpdateOvals(mRightJS.CenterX, mRightJS.CenterY);
            //                 }
            //                 else
            //                 {
            //                     UpdateOvals(mRightJS.CenterX, mRightJS.CenterY + Joystick.DisplacementRadius);
            //                 }

            //	}
            //	else if (e.PointerCount == 1 && e.GetX() > ScreenWidth / 2 && !mLeftJS.IsCentered())
            //	{
            //		UpdateOvals(mLeftJS.CenterX, mLeftJS.CenterY);
            //	}
            //}
            //else
            //{
            //	if (e.PointerCount == 1 && e.GetX() <= ScreenWidth / 2 && !mRightJS.IsCentered())
            //	{
            //		UpdateOvals(mRightJS.CenterX, mRightJS.CenterY);
            //	}
            //	else if (e.PointerCount == 1 && e.GetX() > ScreenWidth / 2 && !mLeftJS.IsCentered())
            //	{
            //                 if (Settings.AltitudeControlActivated)
            //                 {
            //                     UpdateOvals(mLeftJS.CenterX, mLeftJS.CenterY);
            //                 }
            //                 else
            //                 {
            //                     UpdateOvals(mLeftJS.CenterX, mLeftJS.CenterY + Joystick.DisplacementRadius);
            //                 }
            //	}
            //}

            Invalidate();
			return true;
		}

        /// <summary>
        /// Sets the Joystick back to his main position
        /// </summary>
        /// <param name="x"></param>
        public void ResetJoystickPosition(float x)
        {
            if (Settings.Inverted)
            {
                if (x <= ScreenWidth / 2)
                {
                    UpdateOvals(mLeftJS.CenterX, mLeftJS.CenterY);
                }
                else
                {
                    if (Settings.AltitudeControlActivated)
                    {
                        UpdateOvals(mRightJS.CenterX, mRightJS.CenterY);
                    }
                    else
                    {
                        UpdateOvals(mRightJS.CenterX, mRightJS.CenterY + Joystick.DisplacementRadius);
                    }
                }
            }
            else
            {
                if (x <= ScreenWidth / 2)
                {
                    if (Settings.AltitudeControlActivated)
                    {
                        UpdateOvals(mLeftJS.CenterX, mLeftJS.CenterY);
                    }
                    else
                    {
                        UpdateOvals(mLeftJS.CenterX, mLeftJS.CenterY + Joystick.DisplacementRadius);
                    }
                }
                else
                {
                    UpdateOvals(mRightJS.CenterX, mRightJS.CenterY);
                }
            }
        }


		/// <summary>
		/// Sets new bounds for the joystick oval.
		/// </summary>
		/// <param name="xPosition">X-Position of the touch</param>
		/// <param name="yPosition">Y-Position of the touch</param>
		public void UpdateOvals(float xPosition, float yPosition)
		{
			// Check if touch is in left or right half of the screen
			if (xPosition <= ScreenWidth / 2)
			{
				// Handle touch in the left half
				mLeftJS.SetPosition(xPosition, yPosition);
				// Check if touch was inside the displacement radius
				if ((mLeftJS.Abs) <= Joystick.DisplacementRadius)
				{
					// Draw left joystick with original coordinates
					SetBoundsForLeftStick(
						(int)xPosition - (int)Joystick.StickRadius,
						(int)yPosition - (int)Joystick.StickRadius,
						(int)xPosition + (int)Joystick.StickRadius,
						(int)yPosition + (int)Joystick.StickRadius);
				}
				else
				{
					// Draw left joystick with maximum coordinates
					SetBoundsForLeftStick(
						(int)(Joystick.DisplacementRadius * Math.Cos(mLeftJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)mLeftJS.CenterX,
						(int)(Joystick.DisplacementRadius * Math.Sin(mLeftJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)mLeftJS.CenterY,
						(int)(Joystick.DisplacementRadius * Math.Cos(mLeftJS.Angle * Math.PI / 180)) + (int)Joystick.StickRadius + (int)mLeftJS.CenterX,
						(int)(Joystick.DisplacementRadius * Math.Sin(mLeftJS.Angle * Math.PI / 180)) + (int)Joystick.StickRadius + (int)mLeftJS.CenterY);
				}
			}
			else
			{
				// Handle touch in the right half
				mRightJS.SetPosition(xPosition, yPosition);
				// Check if touch was inside the displacement radius
				if ((mRightJS.Abs) <= Joystick.DisplacementRadius)
				{
					// Draw right joystick with original coordinates
					SetBoundsForRightStick(
						(int)xPosition - (int)Joystick.StickRadius,
						(int)yPosition - (int)Joystick.StickRadius,
						(int)xPosition + (int)Joystick.StickRadius,
						(int)yPosition + (int)Joystick.StickRadius);
				}
				else
				{
					// Draw left joystick with maximum coordinates
					SetBoundsForRightStick(
						(int)(Joystick.DisplacementRadius * Math.Cos(mRightJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)mRightJS.CenterX,
						(int)(Joystick.DisplacementRadius * Math.Sin(mRightJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)mRightJS.CenterY,
						(int)(Joystick.DisplacementRadius * Math.Cos(mRightJS.Angle * Math.PI / 180)) + (int)Joystick.StickRadius + (int)mRightJS.CenterX,
						(int)(Joystick.DisplacementRadius * Math.Sin(mRightJS.Angle * Math.PI / 180)) + (int)Joystick.StickRadius + (int)mRightJS.CenterY);
				}
			}
		}

		/// <summary>
		/// Draws the shapes onto the canvas, which is displayed afterwards.
		/// </summary>
		protected override void OnDraw(Canvas canvas)
		{
			this.SetBackgroundResource(Resource.Drawable.bg);

			// Draw shapes
			mShapeBorderRadiusLeft.Draw(canvas);
			mShapeBorderRadiusRight.Draw(canvas);
			mShapeRadiusLeft.Draw(canvas);
			mShapeRadiusRight.Draw(canvas);
			mShapeBorderStickLeft.Draw(canvas);
			mShapeBorderStickRight.Draw(canvas);
			mShapeStickLeft.Draw(canvas);
			mShapeStickRight.Draw(canvas);

			mLeftJS.CalculateValues();
			mRightJS.CalculateValues();
		}

		/// <summary>
		/// Helper method for setting the bounds of the left joystick.
		/// </summary>
		/// <param name="left">Position of left bound</param>
		/// <param name="top">Position of top bound</param>
		/// <param name="right">Position of right bound</param>
		/// <param name="bottom">Position of bottom bound</param>
		private void SetBoundsForLeftStick(int left, int top, int right, int bottom)
		{
			mShapeStickLeft.SetBounds(left, top, right, bottom);
			mShapeBorderStickLeft.SetBounds(left - 2, top - 2, right + 2, bottom + 2);
		}

		/// <summary>
		/// Helper method for setting the bounds of the right joystick.
		/// </summary>
		/// <param name="left">Position of left bound</param>
		/// <param name="top">Position of top bound</param>
		/// <param name="right">Position of right bound</param>
		/// <param name="bottom">Position of bottom bound</param>
		private void SetBoundsForRightStick(int left, int top, int right, int bottom)
		{
			mShapeStickRight.SetBounds(left, top, right, bottom);
			mShapeBorderStickRight.SetBounds(left - 2, top - 2, right + 2, bottom + 2);
		}

        /// <summary>
        /// Helper method for sending data via bluetooth to the device
        /// Throttle = speed
        /// Rudder = Yaw = rotation
        /// Elevator = Pitch = north south
        /// Aileron = Elevator = east west
        /// </summary>
        public void Write(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Settings.Inverted)
            {
                //int throttle = Settings.AltitudeControlActivated ? 50 : mLeftJS.Throttle;
                int throttle = mLeftJS.Throttle;
                mSocket.Write((Int16)throttle,
                              (Int16)(mLeftJS.Rudder - Settings.TrimYaw),
                              (Int16)(mRightJS.Aileron - Settings.TrimRoll),
                              (Int16)(mRightJS.Elevator - Settings.TrimPitch));
            }
            else
            {
                //int throttle = Settings.AltitudeControlActivated ? 50 : mRightJS.Throttle;
                int throttle = mRightJS.Throttle;
                mSocket.Write((Int16)throttle,
                              (Int16)(mLeftJS.Rudder - Settings.TrimYaw),
                              (Int16)(mLeftJS.Aileron - Settings.TrimRoll),
                              (Int16)(mRightJS.Elevator - Settings.TrimPitch));
            }
        }
    }
}