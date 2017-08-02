using System;
using System.Threading.Tasks;
using Foundation;
using Sockets.Plugin;
using SystemConfiguration;
using UIKit;

namespace WiFiTest
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        
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
				}				
			}

            StartConnecting();

        }

		async Task StartConnecting()
		{
			Task connectSocket = Connect();
			await connectSocket;
		}

		public async Task Connect()
		{
			var address = "172.24.1.1";
			var port = 5050;
			var r = new Random();
			var client = new TcpSocketClient();
			await client.ConnectAsync(address, port);
			// we're connected!
			for (int i = 0; i < 5; i++)
			{
				// write to the 'WriteStream' property of the socket client to send data
				var nextByte = (byte)r.Next(0, 254);
				client.WriteStream.WriteByte(nextByte);
				await client.WriteStream.FlushAsync();
				// wait a little before sending the next bit of data
				await Task.Delay(TimeSpan.FromMilliseconds(500));
			}
			await client.DisconnectAsync();
		}

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
