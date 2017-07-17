package md5e3ebf9dfac25e46449a51a00c8ce0a0a;


public class WifiDirectBroadcastReceiver
	extends android.content.BroadcastReceiver
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onReceive:(Landroid/content/Context;Landroid/content/Intent;)V:GetOnReceive_Landroid_content_Context_Landroid_content_Intent_Handler\n" +
			"";
		mono.android.Runtime.register ("Android_WifiDirect_Intro.WifiDirectBroadcastReceiver, Android_WifiDirect_Intro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", WifiDirectBroadcastReceiver.class, __md_methods);
	}


	public WifiDirectBroadcastReceiver () throws java.lang.Throwable
	{
		super ();
		if (getClass () == WifiDirectBroadcastReceiver.class)
			mono.android.TypeManager.Activate ("Android_WifiDirect_Intro.WifiDirectBroadcastReceiver, Android_WifiDirect_Intro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public WifiDirectBroadcastReceiver (android.net.wifi.p2p.WifiP2pManager p0, android.net.wifi.p2p.WifiP2pManager.Channel p1, md5e3ebf9dfac25e46449a51a00c8ce0a0a.MainActivity p2) throws java.lang.Throwable
	{
		super ();
		if (getClass () == WifiDirectBroadcastReceiver.class)
			mono.android.TypeManager.Activate ("Android_WifiDirect_Intro.WifiDirectBroadcastReceiver, Android_WifiDirect_Intro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Android.Net.Wifi.P2p.WifiP2pManager, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:Android.Net.Wifi.P2p.WifiP2pManager+Channel, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:Android_WifiDirect_Intro.MainActivity, Android_WifiDirect_Intro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public void onReceive (android.content.Context p0, android.content.Intent p1)
	{
		n_onReceive (p0, p1);
	}

	private native void n_onReceive (android.content.Context p0, android.content.Intent p1);

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
