using Foundation;
using System;
using UIKit;

namespace WiFiDronection
{
    public partial class SettingsController : UIViewController
    {
        
        public SettingsController (IntPtr handle) : base (handle)
        {
            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            lblControllerSettings.Font = UIFont.FromName("SourceSansPro-Light", 48f);
            btnStartMode1.Font = UIFont.FromName("SourceSansPro-Light", 24f);
            btnStartMode2.Font = UIFont.FromName("SourceSansPro-Light", 24f);
            btnBackFromSettings.Font = UIFont.FromName("SourceSansPro-Light", 24f);
        }

		partial void OnBackFromSettings(UIButton sender)
		{
            this.PerformSegue("backFromSettingsSegue", sender);
		}

		partial void OnStartMode1(UIButton sender)
		{
            ControllerView cv = new ControllerView();
            this.View = cv;
		}

		public override bool ShouldAutorotate()
		{
			return true;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
            return UIInterfaceOrientationMask.LandscapeLeft;
		}
    }
}