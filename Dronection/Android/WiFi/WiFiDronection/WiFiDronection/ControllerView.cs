﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Graphics.Drawables.Shapes;

namespace WiFiDronection
{
    public class ControllerView : View, View.IOnTouchListener
    {
        public static ControllerSettings Settings { get; set; }

        // Screen metrics in px
        public float ScreenWidth;
        public float ScreenHeight;

        // Joystick ovals
        private ShapeDrawable m_ShapeStickLeft;
        private ShapeDrawable m_ShapeStickRight;

        // Displacement ovals
        private ShapeDrawable m_ShapeRadiusLeft;
        private ShapeDrawable m_ShapeRadiusRight;

        // Joystick border
        private ShapeDrawable m_ShapeBorderStickLeft;
        private ShapeDrawable m_ShapeBorderStickRight;

        // Displacement border
        private ShapeDrawable m_ShapeBorderRadiusLeft;
        private ShapeDrawable m_ShapeBorderRadiusRight;

        // Joystick controllers
        private Joystick m_LeftJS;
        private Joystick m_RightJS;

        // Transfer data via bluetooth
        SocketConnection m_SocketConnection;

        // Timer for sending data and checking BT connection
        private System.Timers.Timer m_WriteTimer;

        /// <summary>
        /// View constructor
        /// </summary>
        public ControllerView(Context context) : base(context)
        {
            Init();
        }

        /// <summary>
        /// View constructor
        /// </summary>
        public ControllerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        /// <summary>
        /// View constructor
        /// </summary>
        public ControllerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init();
        }

        /// <summary>
        /// Initialize members and create drawable shapes 
        /// </summary>
        private void Init()
        {
            m_SocketConnection = SocketConnection.Instance;
            Settings = new ControllerSettings
            {
                AltitudeControlActivated = false,
                Inverted = ControllerActivity.Inverted,
                TrimYaw = 0,
                TrimPitch = 0,
                TrimRoll = 0
            };
            SetOnTouchListener(this);
            SetBackgroundColor(Color.White);

            ScreenWidth = Resources.DisplayMetrics.WidthPixels;
            ScreenHeight = Resources.DisplayMetrics.HeightPixels;

            //m_Transfer = new DataTransfer(this);

            InitShapes();
            InitJoysticks();

            this.SetBackgroundResource(Resource.Drawable.bg);

            m_WriteTimer = new System.Timers.Timer();
            m_WriteTimer.Interval = 50;//10
            m_WriteTimer.AutoReset = true;
            m_WriteTimer.Elapsed += Write;
            m_WriteTimer.Start();
        }


        private void InitShapes()
        {
            // Paint for joystick ovals
            var paintStick = new Paint();
            paintStick.SetARGB(255, 88, 88, 88);
            paintStick.SetStyle(Paint.Style.Fill);
            // Shape for left joystick
            m_ShapeStickLeft = new ShapeDrawable(new OvalShape());
            m_ShapeStickLeft.Paint.Set(paintStick);
            // Shape for right joystick
            m_ShapeStickRight = new ShapeDrawable(new OvalShape());
            m_ShapeStickRight.Paint.Set(paintStick);

            // Paint for displacement ovals
            var paintRadius = new Paint();
            paintRadius.Color = Color.LightGray;
            //paintRadius.SetARGB(255, 230, 230, 230);
            //paintRadius.SetStyle(Paint.Style.Fill);
            paintRadius.SetStyle(Paint.Style.Fill);
            // Shape for left displacement 
            m_ShapeRadiusLeft = new ShapeDrawable(new OvalShape());
            m_ShapeRadiusLeft.Paint.Set(paintRadius);
            // Shape for right displacement
            m_ShapeRadiusRight = new ShapeDrawable(new OvalShape());
            m_ShapeRadiusRight.Paint.Set(paintRadius);

            // Paint for border ovals
            var paintBorder = new Paint();
            paintBorder.SetARGB(255, 44, 44, 44);
            paintStick.SetStyle(Paint.Style.Fill);
            // Shape for left joystick border
            m_ShapeBorderStickLeft = new ShapeDrawable(new OvalShape());
            m_ShapeBorderStickLeft.Paint.Set(paintBorder);
            // Shape for right joystick border
            m_ShapeBorderStickRight = new ShapeDrawable(new OvalShape());
            m_ShapeBorderStickRight.Paint.Set(paintBorder);
            // Shape for left displacement border
            m_ShapeBorderRadiusLeft = new ShapeDrawable(new OvalShape());
            m_ShapeBorderRadiusLeft.Paint.Set(paintBorder);
            // Shape for right displacement border
            m_ShapeBorderRadiusRight = new ShapeDrawable(new OvalShape());
            m_ShapeBorderRadiusRight.Paint.Set(paintBorder);
        }

        /// <summary>
        /// Sets the bounds for every joystick and displacement oval
        /// </summary>
        private void InitJoysticks()
        {
            m_LeftJS = new Joystick(ScreenWidth, ScreenHeight, true, Settings.Inverted);
            m_RightJS = new Joystick(ScreenWidth, ScreenHeight, false, Settings.Inverted);

            SetBoundsForLeftStick(
                (int)m_LeftJS.CenterX - (int)Joystick.StickRadius,
                Settings.Inverted ? (int)m_LeftJS.CenterY - (int)Joystick.StickRadius : (int)m_LeftJS.CenterY + (int)Joystick.StickRadius,
                (int)m_LeftJS.CenterX + (int)Joystick.StickRadius,
                Settings.Inverted ? (int)m_LeftJS.CenterY + (int)Joystick.StickRadius : (int)m_LeftJS.CenterY + 3 * (int)Joystick.StickRadius);

            SetBoundsForRightStick(
                (int)m_RightJS.CenterX - (int)Joystick.StickRadius,
                Settings.Inverted ? (int)m_RightJS.CenterY + (int)Joystick.StickRadius : (int)m_RightJS.CenterY - (int)Joystick.StickRadius,
                (int)m_RightJS.CenterX + (int)Joystick.StickRadius,
                Settings.Inverted ? (int)m_RightJS.CenterY + 3 * (int)Joystick.StickRadius : (int)m_RightJS.CenterY + (int)Joystick.StickRadius);

            m_ShapeRadiusLeft.SetBounds(
                (int)m_LeftJS.CenterX - (int)Joystick.DisplacementRadius,
                (int)m_LeftJS.CenterY - (int)Joystick.DisplacementRadius,
                (int)m_LeftJS.CenterX + (int)Joystick.DisplacementRadius,
                (int)m_LeftJS.CenterY + (int)Joystick.DisplacementRadius);

            m_ShapeRadiusRight.SetBounds(
                (int)m_RightJS.CenterX - (int)Joystick.DisplacementRadius,
                (int)m_RightJS.CenterY - (int)Joystick.DisplacementRadius,
                (int)m_RightJS.CenterX + (int)Joystick.DisplacementRadius,
                (int)m_RightJS.CenterY + (int)Joystick.DisplacementRadius);

            m_ShapeBorderRadiusLeft.SetBounds(
                (int)m_LeftJS.CenterX - (int)Joystick.DisplacementRadius - 2,
                (int)m_LeftJS.CenterY - (int)Joystick.DisplacementRadius - 2,
                (int)m_LeftJS.CenterX + (int)Joystick.DisplacementRadius + 2,
                (int)m_LeftJS.CenterY + (int)Joystick.DisplacementRadius + 2);

            m_ShapeBorderRadiusRight.SetBounds(
                (int)m_RightJS.CenterX - (int)Joystick.DisplacementRadius - 2,
                (int)m_RightJS.CenterY - (int)Joystick.DisplacementRadius - 2,
                (int)m_RightJS.CenterX + (int)Joystick.DisplacementRadius + 2,
                (int)m_RightJS.CenterY + (int)Joystick.DisplacementRadius + 2);
        }

        /// <summary>
        /// Checks single or multitouch and sets new bounds
        /// </summary>
        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Up:
                    if (Settings.Inverted)
                    {
                        if (e.GetX() <= ScreenWidth / 2)
                        {
                            UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY);
                        }
                        else
                        {
                            UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY + Joystick.DisplacementRadius);
                        }
                    }
                    else
                    {
                        if (e.GetX() <= ScreenWidth / 2)
                        {
                            UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY + Joystick.DisplacementRadius);
                        }
                        else
                        {
                            UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY);
                        }
                    }
                    break;
                case MotionEventActions.Pointer1Up:
                    if (Settings.Inverted)
                    {
                        for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                        {
                            if (e.GetX(i) <= ScreenWidth / 2)
                            {
                                UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY);
                            }
                            else
                            {
                                UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY + Joystick.DisplacementRadius);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                        {
                            if (e.GetX(i) <= ScreenWidth / 2)
                            {
                                UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY + Joystick.DisplacementRadius);
                            }
                            else
                            {
                                UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY);
                            }
                        }
                    }
                    break;
                case MotionEventActions.Pointer2Up:
                    if (Settings.Inverted)
                    {
                        for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                        {
                            if (e.GetX(i) <= ScreenWidth / 2)
                            {
                                UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY);
                            }
                            else
                            {
                                UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY + Joystick.DisplacementRadius);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                        {
                            if (e.GetX(i) <= ScreenWidth / 2)
                            {
                                UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY + Joystick.DisplacementRadius);
                            }
                            else
                            {
                                UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY);
                            }
                        }
                    }
                    break;
                default:
                    for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                    {
                        UpdateOvals(e.GetX(i), e.GetY(i));
                    }
                    break;
            }

            if (Settings.Inverted)
            {
                if (e.PointerCount == 1 && e.GetX() <= ScreenWidth / 2 && !m_RightJS.IsCentered())
                {
                    UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY + Joystick.DisplacementRadius);
                }
                else if (e.PointerCount == 1 && e.GetX() > ScreenWidth / 2 && !m_LeftJS.IsCentered())
                {
                    UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY);
                }
            }
            else
            {
                if (e.PointerCount == 1 && e.GetX() <= ScreenWidth / 2 && !m_RightJS.IsCentered())
                {
                    UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY);
                }
                else if (e.PointerCount == 1 && e.GetX() > ScreenWidth / 2 && !m_LeftJS.IsCentered())
                {
                    UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY + Joystick.DisplacementRadius);
                }
            }

            Invalidate();
            return true;
        }

        /// <summary>
        /// Sets new bounds for the joystick oval
        /// </summary>
        /// <param name="xPosition">X-Position of the touch</param>
        /// <param name="yPosition">Y-Position of the touch</param>
        private void UpdateOvals(float xPosition, float yPosition)
        {
            // Check if touch is in left or right half of the screen
            if (xPosition <= ScreenWidth / 2)
            {
                // Handle touch in the left half
                m_LeftJS.SetPosition(xPosition, yPosition);
                // Check if touch was inside the displacement radius
                if ((m_LeftJS.Abs) <= Joystick.DisplacementRadius)
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
                    (int)(Joystick.DisplacementRadius * Math.Cos(m_LeftJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)m_LeftJS.CenterX,
                    (int)(Joystick.DisplacementRadius * Math.Sin(m_LeftJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)m_LeftJS.CenterY,
                    (int)(Joystick.DisplacementRadius * Math.Cos(m_LeftJS.Angle * Math.PI / 180)) + (int)Joystick.StickRadius + (int)m_LeftJS.CenterX,
                    (int)(Joystick.DisplacementRadius * Math.Sin(m_LeftJS.Angle * Math.PI / 180)) + (int)Joystick.StickRadius + (int)m_LeftJS.CenterY);

                    // OPTION: Set radius as position
                    //m_LeftJS.SetPosition((int)(m_LeftJS.m_DisplacementRadius * Math.Cos(m_LeftJS.GetAngle() * Math.PI / 180)) + (int)m_LeftJS.CENTER_X, 
                    //    (int)(m_LeftJS.m_DisplacementRadius * Math.Sin(m_LeftJS.GetAngle() * Math.PI / 180)) + (int)m_LeftJS.CENTER_Y);

                }
            }
            else
            {
                // Handle touch in the right half
                m_RightJS.SetPosition(xPosition, yPosition);
                // Check if touch was inside the displacement radius
                if ((m_RightJS.Abs) <= Joystick.DisplacementRadius)
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
                    (int)(Joystick.DisplacementRadius * Math.Cos(m_RightJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)m_RightJS.CenterX,
                    (int)(Joystick.DisplacementRadius * Math.Sin(m_RightJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)m_RightJS.CenterY,
                    (int)(Joystick.DisplacementRadius * Math.Cos(m_RightJS.Angle * Math.PI / 180)) + (int)Joystick.StickRadius + (int)m_RightJS.CenterX,
                    (int)(Joystick.DisplacementRadius * Math.Sin(m_RightJS.Angle * Math.PI / 180)) + (int)Joystick.StickRadius + (int)m_RightJS.CenterY);

                    // OPTION: Set radius as position 
                    //m_RightJS.SetPosition((int)(m_RightJS.m_DisplacementRadius * Math.Cos(m_RightJS.GetAngle() * Math.PI / 180)) + (int)m_RightJS.CENTER_X,
                    //    (int)(m_RightJS.m_DisplacementRadius * Math.Sin(m_RightJS.GetAngle() * Math.PI / 180)) + (int)m_RightJS.CENTER_Y);
                }
            }
        }

        /// <summary>
        /// Draws the shapes onto the canvas, which is displayed afterwards
        /// </summary>
        protected override void OnDraw(Canvas canvas)
        {
            this.SetBackgroundResource(Resource.Drawable.bg);

            // Draw shapes
            m_ShapeBorderRadiusLeft.Draw(canvas);
            m_ShapeBorderRadiusRight.Draw(canvas);
            m_ShapeRadiusLeft.Draw(canvas);
            m_ShapeRadiusRight.Draw(canvas);
            m_ShapeBorderStickLeft.Draw(canvas);
            m_ShapeBorderStickRight.Draw(canvas);
            m_ShapeStickLeft.Draw(canvas);
            m_ShapeStickRight.Draw(canvas);

            // Set paint for data text
            var paint = new Paint();
            paint.SetARGB(255, 0, 0, 0);
            paint.TextSize = 20;
            paint.TextAlign = Paint.Align.Center;
            paint.StrokeWidth = 5;

            m_LeftJS.CalculateValues();
            m_RightJS.CalculateValues();
        }

        /// <summary>
        /// Helper method for setting the bounds of the left joystick
        /// </summary>
        /// <param name="left">Position of left bound</param>
        /// <param name="top">Position of top bound</param>
        /// <param name="right">Position of right bound</param>
        /// <param name="bottom">Position of bottom bound</param>
        private void SetBoundsForLeftStick(int left, int top, int right, int bottom)
        {
            m_ShapeStickLeft.SetBounds(left, top, right, bottom);
            m_ShapeBorderStickLeft.SetBounds(left - 2, top - 2, right + 2, bottom + 2);
        }

        /// <summary>
        /// Helper method for setting the bounds of the right joystick
        /// </summary>
        /// <param name="left">Position of left bound</param>
        /// <param name="top">Position of top bound</param>
        /// <param name="right">Position of right bound</param>
        /// <param name="bottom">Position of bottom bound</param>
        private void SetBoundsForRightStick(int left, int top, int right, int bottom)
        {
            m_ShapeStickRight.SetBounds(left, top, right, bottom);
            m_ShapeBorderStickRight.SetBounds(left - 2, top - 2, right + 2, bottom + 2);
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
            if (m_SocketConnection.isConnected)
            {
                if (!Settings.Inverted)
                {

                    m_SocketConnection.Write((Int16)m_LeftJS.Throttle,
                                      (Int16)(m_LeftJS.Rudder + Settings.TrimYaw),
                                      (Int16)(m_RightJS.Aileron + Settings.TrimPitch),
                                      (Int16)(m_RightJS.Elevator + Settings.TrimRoll));
                }
                else
                {
                    m_SocketConnection.Write((Int16)m_RightJS.Throttle,
                                      (Int16)(m_LeftJS.Rudder + Settings.TrimYaw),
                                      (Int16)(m_LeftJS.Aileron + Settings.TrimPitch),
                                      (Int16)(m_RightJS.Elevator + Settings.TrimRoll));
                }
            }
        }
    }
}