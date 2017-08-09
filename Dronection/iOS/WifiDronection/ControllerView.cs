﻿﻿using Foundation;
using System;
using UIKit;
using CoreGraphics;

namespace WiFiDronection
{
	public partial class ControllerView : UIView
	{
		private readonly float SCREEN_WIDTH;
		private readonly float SCREEN_HEIGHT;

		private Joystick mLeftJS;
		private Joystick mRightJS;

		private CGRect mCircleJSLeft;
		private CGRect mCircleJSRight;
		private CGRect mCircleDPLeft;
		private CGRect mCircleDPRight;

        private SocketConnection mSocket;

        public  ControllerSettings Settings;
		private bool mInverted = false;

        private System.Timers.Timer mWriteTimer;

        public ControllerView()
        {
            SCREEN_WIDTH = (float)UIScreen.MainScreen.Bounds.Width;
			SCREEN_HEIGHT = (float)UIScreen.MainScreen.Bounds.Height;

			UserInteractionEnabled = true;
			MultipleTouchEnabled = true;

			Settings = ControllerSettings.Instance;
            Settings.Inverted = false;
            Settings.LoggingActivated = false;
            Settings.AltitudeControlActivated = false;
            Settings.TrimYaw = 0;
            Settings.TrimPitch = 0;
            Settings.TrimRoll = 0;
            Settings.MinYaw = -15;
            Settings.MaxYaw = 15;
            Settings.MinPitch = -20;
            Settings.MaxPitch = 20;
            Settings.MinRoll = -20;
            Settings.MaxRoll = 20;

			mWriteTimer = new System.Timers.Timer();
			mWriteTimer.Interval = 50;//10
			mWriteTimer.AutoReset = true;
			mWriteTimer.Elapsed += Write;
			mWriteTimer.Start();

			InitJoysticks();
        }

		public ControllerView(IntPtr handle) : base(handle)
		{
			CGRect screenSize = UIScreen.MainScreen.Bounds;
			SCREEN_WIDTH = (float)screenSize.Height;
			SCREEN_HEIGHT = (float)screenSize.Width;

			UserInteractionEnabled = true;
			MultipleTouchEnabled = true;

            Settings = ControllerSettings.Instance;
			Settings.Inverted = false;
			Settings.LoggingActivated = false;
			Settings.AltitudeControlActivated = false;
			Settings.TrimYaw = 0;
			Settings.TrimPitch = 0;
			Settings.TrimRoll = 0;
			Settings.MinYaw = -15;
			Settings.MaxYaw = 15;
			Settings.MinPitch = -20;
			Settings.MaxPitch = 20;
			Settings.MinRoll = -20;
			Settings.MaxRoll = 20;

			mWriteTimer = new System.Timers.Timer();
			mWriteTimer.Interval = 50;//10
			mWriteTimer.AutoReset = true;
			mWriteTimer.Elapsed += Write;
			mWriteTimer.Start();

			InitJoysticks();
		}

		private void InitJoysticks()
		{
            BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("bg.png"));

			mLeftJS = new Joystick(SCREEN_WIDTH, SCREEN_HEIGHT, true, mInverted);
			mRightJS = new Joystick(SCREEN_WIDTH, SCREEN_HEIGHT, false, mInverted);

			mCircleJSLeft = new CGRect(mLeftJS.CenterX - Joystick.StickRadius,
                                        mLeftJS.CenterY + Joystick.DisplacementRadius - Joystick.StickRadius,
                                        Joystick.StickRadius * 2, Joystick.StickRadius * 2);

			mCircleJSRight = new CGRect(mRightJS.CenterX - Joystick.StickRadius,
										  mRightJS.CenterY - Joystick.StickRadius,
										  Joystick.StickRadius * 2, Joystick.StickRadius * 2);

            mCircleDPLeft = new CGRect(mLeftJS.CenterX - Joystick.DisplacementRadius,
										 mLeftJS.CenterY - Joystick.DisplacementRadius,
										 Joystick.DisplacementRadius * 2, Joystick.DisplacementRadius * 2);

			mCircleDPRight = new CGRect(mRightJS.CenterX - Joystick.DisplacementRadius,
										  mRightJS.CenterY - Joystick.DisplacementRadius,
										  Joystick.DisplacementRadius * 2, Joystick.DisplacementRadius * 2);

			SetNeedsDisplay();
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);
			using (var g = UIGraphics.GetCurrentContext())
			{

				//create geometry
				var path1 = new CGPath();

				g.SetFillColor(UIColor.LightGray.CGColor);
				path1.AddEllipseInRect(mCircleDPLeft);
				path1.AddEllipseInRect(mCircleDPRight);

				path1.CloseSubpath();
				g.AddPath(path1);
				g.DrawPath(CGPathDrawingMode.FillStroke);

				var path2 = new CGPath();

				g.SetFillColor(UIColor.DarkGray.CGColor);
				path2.AddEllipseInRect(mCircleJSLeft);
				path2.AddEllipseInRect(mCircleJSRight);

				path2.CloseSubpath();
				g.AddPath(path2);
				g.DrawPath(CGPathDrawingMode.FillStroke);

				g.ShowTextAtPoint(mLeftJS.CenterX, 30, "LEFT JOYSTICK");
			}
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);
			foreach (UITouch touch in touches)
			{
				CGPoint p = touch.LocationInView(this);
				UpdateOvals((float)p.X, (float)p.Y);
			}
			SetNeedsDisplay();
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);
			foreach (UITouch touch in touches)
			{
				CGPoint p = touch.LocationInView(this);
				UpdateOvals((float)p.X, (float)p.Y);
			}
			SetNeedsDisplay();
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);
			foreach (UITouch touch in touches)
			{
				CGPoint p = touch.LocationInView(this);
				if ((float)p.X <= SCREEN_WIDTH / 2)
				{
                    UpdateOvals(mLeftJS.CenterX, mLeftJS.CenterY + Joystick.DisplacementRadius);
				}
				else
				{
					UpdateOvals(mRightJS.CenterX, mRightJS.CenterY);
				}
			}
			SetNeedsDisplay();
		}

		private void UpdateOvals(float xPosition, float yPosition)
		{
			if (xPosition <= SCREEN_WIDTH / 2)
			{
				mLeftJS.SetPosition(xPosition, yPosition);
				if ((mLeftJS.Abs) <= Joystick.DisplacementRadius)
				{
                    mCircleJSLeft.X = mLeftJS.GetPosition()[0] - Joystick.StickRadius;
					mCircleJSLeft.Y = mLeftJS.GetPosition()[1] - Joystick.StickRadius;
				}
				else
				{
					mCircleJSLeft.X = (int)(Joystick.DisplacementRadius * Math.Cos(mLeftJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)mLeftJS.CenterX;
					mCircleJSLeft.Y = (int)(Joystick.DisplacementRadius * Math.Sin(mLeftJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)mLeftJS.CenterY;
				}
			}
			else
			{
				mRightJS.SetPosition(xPosition, yPosition);
				if ((mRightJS.Abs) <= Joystick.DisplacementRadius)
				{
					mCircleJSRight.X = mRightJS.GetPosition()[0] - Joystick.StickRadius;
					mCircleJSRight.Y = mRightJS.GetPosition()[1] - Joystick.StickRadius;
				}
				else
				{
					mCircleJSRight.X = (int)(Joystick.DisplacementRadius * Math.Cos(mRightJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)mRightJS.CenterX;
					mCircleJSRight.Y = (int)(Joystick.DisplacementRadius * Math.Sin(mRightJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)mRightJS.CenterY;
				}
			}
		}

		/// <summary>
		/// Helper method for sending data via bluetooth to the device.
		/// Throttle = speed
		/// Rudder = Yaw = rotation
		/// Elevator = Pitch = north south
		/// Aileron = Roll = east west
		/// </summary>
		public void Write(object sender, System.Timers.ElapsedEventArgs e)
		{
            if (mSocket.IsConnected)
			{
				// Test
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
}