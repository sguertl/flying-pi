using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Android.Graphics;

namespace Datalyze
{
    [Activity(Label = "BTConnectionActivity", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class BTConnectionActivity : Activity
    {
        // Widgets
        private ListView mPeerListView;
        private ListAdapter mAdapter;
        private LinearLayout mLinearLayout;
        private ProgressDialog mProgressDialog;
        private Button mBtSearchDevices;
        private TextView mTvExplanation;
        private TextView mTvHeader;

        // Bluetooth members
        private BluetoothAdapter mBtAdapter;
        private List<BluetoothDevice> mPairedDevices;
        private List<String> mPeers;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BTConnectionLayout);

            BluetoothAdapter btAdapter = BluetoothAdapter.DefaultAdapter;
            if(btAdapter.IsEnabled == false)
            {
                Intent intent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(intent, 1);
            }
            
            if(btAdapter.IsEnabled == true)
            {
                Init();
            }
        }

        public void Init()
        {
            // Initializing objects
            mPeerListView = FindViewById<ListView>(Resource.Id.listView);
            mLinearLayout = FindViewById<LinearLayout>(Resource.Id.linear2);
            mBtAdapter = BluetoothAdapter.DefaultAdapter;
            mPairedDevices = mBtAdapter.BondedDevices.ToList();//.Where(bd => bd.Name.ToUpper().Contains("RASPBERRY") || bd.Name.ToUpper().Contains("RPI") || bd.Name.ToUpper().Contains("XMC")).ToList();
            mPeers = new List<String>();

            mProgressDialog = new ProgressDialog(this);
            mProgressDialog.SetMessage("Connecting with device");
            mProgressDialog.SetCancelable(false);

            // var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            mBtSearchDevices = FindViewById<Button>(Resource.Id.btSearchDevices);
            mBtSearchDevices.Click += OnSearchDevices;
            //mBtSearchDevices.Typeface = font;

            mTvExplanation = FindViewById<TextView>(Resource.Id.tvExplanation);
            // mTvExplanation.Typeface = font;

            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeader);
            //mTvHeader.Typeface = font;

            // Set activity background
            mLinearLayout.SetBackgroundColor(Color.White);

            // Set background color of the ListView
            mPeerListView.DividerHeight = 14;

            // Add handler when clicking on a ListViewItem
            mPeerListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => { OnItemClick(sender, e); };

            GetPairedDevices();
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            TextView view = (TextView)e.View.FindViewById<TextView>(Resource.Id.tvListItem);
            string address = view.Text.Split('\n')[1];
            BluetoothDevice bluetoothDevice = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(address);
            // Connection aufbauen
            Intent intent = new Intent(BaseContext ,typeof(BTAnalyzeActivity));
            intent.PutExtra("device", bluetoothDevice);
            StartActivity(intent);
            // *****
        }


        private void OnSearchDevices(object sender, EventArgs e)
        {
            Intent bluetoothSettings = new Intent();
            bluetoothSettings.SetAction(Android.Provider.Settings.ActionBluetoothSettings);
            StartActivityForResult(bluetoothSettings, 0);
        }

        private void GetPairedDevices()
        {
            // Display all paired devices on a ListView
            foreach (BluetoothDevice device in mPairedDevices)
            {
                if (mPeers.Contains(device.Name + "\n" + device.Address) == false)
                {
                    mPeers.Add(device.Name + "\n" + device.Address);
                }
            }

            //mAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mPeers);
            mAdapter = new ListAdapter(this, mPeers);
            mPeerListView.Adapter = mAdapter;
        }
    }
}