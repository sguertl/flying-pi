using Foundation;
using System;
using UIKit;
using SystemConfiguration;

namespace WiFiDronection
{
    public partial class MainViewController : UIViewController
    {

        public MainViewController (IntPtr handle) : base (handle)
        {
			// Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			lblHeader.Font = UIFont.FromName("SourceSansPro-Light", 48f);
			lblSsid.Font = UIFont.FromName("SourceSansPro-Light", 24f);
			lblMac.Font = UIFont.FromName("SourceSansPro-Light", 24f);
			btnConnect.Font = UIFont.FromName("SourceSansPro-Light", 24f);
			btnLogFiles.Font = UIFont.FromName("SourceSansPro-Light", 24f);
			btnHelp.Font = UIFont.FromName("SourceSansPro-Light", 24f);
            lblFooter.Font = UIFont.FromName("SourceSansPro-Light", 14f);

			String[] interfaces;
			CaptiveNetwork.TryGetSupportedInterfaces(out interfaces);
			if (interfaces != null && interfaces.Length >= 1)
			{
				NSDictionary dict;
				CaptiveNetwork.TryCopyCurrentNetworkInfo(interfaces[0], out dict);

                if (dict != null)
                {
                    var bssid = (NSString)dict[CaptiveNetwork.NetworkInfoKeyBSSID];
                    var ssid = (NSString)dict[CaptiveNetwork.NetworkInfoKeySSID];
                    if (ssid.ToString().ToLower().Contains("rasp") || ssid.ToString().ToLower().Contains("rasp"))
                    {
                        lblSsid.Text = "SSID: " + ssid.ToString();
                        lblMac.Text = "MAC: " + bssid.ToString();
                        btnConnect.SetTitle("Connect", UIControlState.Normal);
						btnConnect.Enabled = true;
					}
				}
			}

        }

		partial void OnConnect(UIButton sender)
		{
			this.PerformSegue("connectSegue", sender);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		public override bool ShouldAutorotate()
		{
			return false;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
            return UIInterfaceOrientationMask.Portrait;
		}
    }
}