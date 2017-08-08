﻿using Foundation;
using System;
using UIKit;
using CoreGraphics;

namespace WiFiDronection
{
	public partial class ControllerView : UIView
	{
		private readonly float SCREEN_WIDTH;
		private readonly float SCREEN_HEIGHT;

		private Joystick m_LeftJS;
		private Joystick m_RightJS;

		private CGRect m_CircleJSLeft;
		private CGRect m_CircleJSRight;
		private CGRect m_CircleDPLeft;
		private CGRect m_CircleDPRight;

		private bool m_Inverted = false;

        public ControllerView()
        {
            SCREEN_WIDTH = (float)UIScreen.MainScreen.Bounds.Width;
			SCREEN_HEIGHT = (float)UIScreen.MainScreen.Bounds.Height;

			UserInteractionEnabled = true;
			MultipleTouchEnabled = true;

			InitJoysticks();
        }

		public ControllerView(IntPtr handle) : base(handle)
		{
			CGRect screenSize = UIScreen.MainScreen.Bounds;
			SCREEN_WIDTH = (float)screenSize.Height;
			SCREEN_HEIGHT = (float)screenSize.Width;

			UserInteractionEnabled = true;
			MultipleTouchEnabled = true;

			InitJoysticks();
		}

		private void InitJoysticks()
		{
            BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("bg.png"));

			m_LeftJS = new Joystick(SCREEN_WIDTH, SCREEN_HEIGHT, true, m_Inverted);
			m_RightJS = new Joystick(SCREEN_WIDTH, SCREEN_HEIGHT, false, m_Inverted);

			m_CircleJSLeft = new CGRect(m_LeftJS.CenterX - Joystick.StickRadius,
										 m_LeftJS.CenterY - Joystick.StickRadius,
                                        Joystick.StickRadius * 2, Joystick.StickRadius * 2);

			m_CircleJSRight = new CGRect(m_RightJS.CenterX - Joystick.StickRadius,
										  m_RightJS.CenterY - Joystick.StickRadius,
										  Joystick.StickRadius * 2, Joystick.StickRadius * 2);

            m_CircleDPLeft = new CGRect(m_LeftJS.CenterX - Joystick.DisplacementRadius,
										 m_LeftJS.CenterY - Joystick.DisplacementRadius,
										 Joystick.DisplacementRadius * 2, Joystick.DisplacementRadius * 2);

			m_CircleDPRight = new CGRect(m_RightJS.CenterX - Joystick.DisplacementRadius,
										  m_RightJS.CenterY - Joystick.DisplacementRadius,
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
				path1.AddEllipseInRect(m_CircleDPLeft);
				path1.AddEllipseInRect(m_CircleDPRight);

				path1.CloseSubpath();
				g.AddPath(path1);
				g.DrawPath(CGPathDrawingMode.FillStroke);

				var path2 = new CGPath();

				g.SetFillColor(UIColor.DarkGray.CGColor);
				path2.AddEllipseInRect(m_CircleJSLeft);
				path2.AddEllipseInRect(m_CircleJSRight);

				path2.CloseSubpath();
				g.AddPath(path2);
				g.DrawPath(CGPathDrawingMode.FillStroke);

				g.ShowTextAtPoint(m_LeftJS.CenterX, 30, "LEFT JOYSTICK");
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
					UpdateOvals(m_LeftJS.CenterX, (float)p.Y);
				}
				else
				{
					UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY);
				}

			}
			SetNeedsDisplay();
		}

		private void UpdateOvals(float xPosition, float yPosition)
		{
			if (xPosition <= SCREEN_WIDTH / 2)
			{
				m_LeftJS.SetPosition(xPosition, yPosition);
				if ((m_LeftJS.Abs) <= Joystick.DisplacementRadius)
				{
                    m_CircleJSLeft.X = m_LeftJS.GetPosition()[0] - Joystick.StickRadius;
					m_CircleJSLeft.Y = m_LeftJS.GetPosition()[1] - Joystick.StickRadius;
				}
				else
				{
					m_CircleJSLeft.X = (int)(Joystick.DisplacementRadius * Math.Cos(m_LeftJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)m_LeftJS.CenterX;
					m_CircleJSLeft.Y = (int)(Joystick.DisplacementRadius * Math.Sin(m_LeftJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)m_LeftJS.CenterY;
				}
			}
			else
			{
				m_RightJS.SetPosition(xPosition, yPosition);
				if ((m_RightJS.Abs) <= Joystick.DisplacementRadius)
				{
					m_CircleJSRight.X = m_RightJS.GetPosition()[0] - Joystick.StickRadius;
					m_CircleJSRight.Y = m_RightJS.GetPosition()[1] - Joystick.StickRadius;
				}
				else
				{
					m_CircleJSRight.X = (int)(Joystick.DisplacementRadius * Math.Cos(m_RightJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)m_RightJS.CenterX;
					m_CircleJSRight.Y = (int)(Joystick.DisplacementRadius * Math.Sin(m_RightJS.Angle * Math.PI / 180)) - (int)Joystick.StickRadius + (int)m_RightJS.CenterY;
				}
			}
		}
	}
}