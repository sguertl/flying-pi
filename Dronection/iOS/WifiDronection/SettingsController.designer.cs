// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace WiFiDronection
{
    [Register ("SettingsController")]
    partial class SettingsController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnBackFromSettings { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnStartMode1 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnStartMode2 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView imvMode1 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView imvMode2 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblControllerSettings { get; set; }

        [Action ("OnBackFromSettings:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OnBackFromSettings (UIKit.UIButton sender);

        [Action ("OnStartMode1:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OnStartMode1 (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnBackFromSettings != null) {
                btnBackFromSettings.Dispose ();
                btnBackFromSettings = null;
            }

            if (btnStartMode1 != null) {
                btnStartMode1.Dispose ();
                btnStartMode1 = null;
            }

            if (btnStartMode2 != null) {
                btnStartMode2.Dispose ();
                btnStartMode2 = null;
            }

            if (imvMode1 != null) {
                imvMode1.Dispose ();
                imvMode1 = null;
            }

            if (imvMode2 != null) {
                imvMode2.Dispose ();
                imvMode2 = null;
            }

            if (lblControllerSettings != null) {
                lblControllerSettings.Dispose ();
                lblControllerSettings = null;
            }
        }
    }
}