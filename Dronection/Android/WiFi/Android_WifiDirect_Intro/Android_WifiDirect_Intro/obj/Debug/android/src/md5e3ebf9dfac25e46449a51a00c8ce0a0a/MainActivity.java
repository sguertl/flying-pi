package md5e3ebf9dfac25e46449a51a00c8ce0a0a;


public class MainActivity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer,
		android.net.wifi.p2p.WifiP2pManager.PeerListListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onResume:()V:GetOnResumeHandler\n" +
			"n_onPause:()V:GetOnPauseHandler\n" +
			"n_onPeersAvailable:(Landroid/net/wifi/p2p/WifiP2pDeviceList;)V:GetOnPeersAvailable_Landroid_net_wifi_p2p_WifiP2pDeviceList_Handler:Android.Net.Wifi.P2p.WifiP2pManager/IPeerListListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("Android_WifiDirect_Intro.MainActivity, Android_WifiDirect_Intro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", MainActivity.class, __md_methods);
	}


	public MainActivity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == MainActivity.class)
			mono.android.TypeManager.Activate ("Android_WifiDirect_Intro.MainActivity, Android_WifiDirect_Intro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onResume ()
	{
		n_onResume ();
	}

	private native void n_onResume ();


	public void onPause ()
	{
		n_onPause ();
	}

	private native void n_onPause ();


	public void onPeersAvailable (android.net.wifi.p2p.WifiP2pDeviceList p0)
	{
		n_onPeersAvailable (p0);
	}

	private native void n_onPeersAvailable (android.net.wifi.p2p.WifiP2pDeviceList p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
