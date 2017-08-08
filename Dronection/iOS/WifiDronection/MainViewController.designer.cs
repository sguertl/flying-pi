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
    [Register ("MainViewController")]
    partial class MainViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnConnect { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnHelp { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnLogFiles { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView imvLogo { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblFooter { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblHeader { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblMac { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblSsid { get; set; }

        [Action ("OnConnect:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OnConnect (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnConnect != null) {
                btnConnect.Dispose ();
                btnConnect = null;
            }

            if (btnHelp != null) {
                btnHelp.Dispose ();
                btnHelp = null;
            }

            if (btnLogFiles != null) {
                btnLogFiles.Dispose ();
                btnLogFiles = null;
            }

            if (imvLogo != null) {
                imvLogo.Dispose ();
                imvLogo = null;
            }

            if (lblFooter != null) {
                lblFooter.Dispose ();
                lblFooter = null;
            }

            if (lblHeader != null) {
                lblHeader.Dispose ();
                lblHeader = null;
            }

            if (lblMac != null) {
                lblMac.Dispose ();
                lblMac = null;
            }

            if (lblSsid != null) {
                lblSsid.Dispose ();
                lblSsid = null;
            }
        }
    }
}