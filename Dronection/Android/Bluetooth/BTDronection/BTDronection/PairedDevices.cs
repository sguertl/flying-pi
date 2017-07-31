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
using System.Threading;
using Android.Graphics;

namespace BTDronection
{
    [Activity(Label = "PairedDevices", Theme = "@android:style/Theme.Holo.Light.NoActionBar.Fullscreen",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PairedDevices : Activity
    {
        // Members
        private ListView m_PeerListView;
        private BluetoothAdapter m_BtAdapter;
        private List<BluetoothDevice> m_PairedDevice;
        private ListAdapter m_Adapter;
        private LinearLayout m_Linear;
        private List<String> m_PeerList;
        private bool m_IsConnected;
        private ProgressDialog m_ProgressDialog;
        private Button mBtSearchDevices;
        private TextView mTvExplanation;
        private TextView mTvHeader;
        private SocketConnection mSocketConnection;


        public bool IsConnected
        {
            get { return m_IsConnected; }
            set { m_IsConnected = value; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PairedLayout);
  
            Init();

            // Displaying the paired devices
            GetPairedDevices();
        }


        private void GetPairedDevices()
        {
            // Displaying all paired devices on a ListView
            foreach (BluetoothDevice device in m_PairedDevice)
            {
                m_PeerList.Add(device.Name + "\n" + device.Address);
            }

            //m_Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, m_PeerList);
            m_Adapter = new ListAdapter(this, m_PeerList);
            m_PeerListView.Adapter = m_Adapter;
        }

        /// <summary>
        /// Initializes and modifies objects
        /// </summary>
        public void Init()
        {
            // Initializing objects
            m_PeerListView = FindViewById<ListView>(Resource.Id.listView);
            m_Linear = FindViewById<LinearLayout>(Resource.Id.linear2);
            m_BtAdapter = BluetoothAdapter.DefaultAdapter;
            m_PairedDevice = m_BtAdapter.BondedDevices.Where(bd => bd.Name.ToUpper().Contains("RASPBERRY") || bd.Name.ToUpper().Contains("RPI") || bd.Name.ToUpper().Contains("XMC")).ToList();
            m_PeerList = new List<String>();
            m_IsConnected = true;

            mSocketConnection = SocketConnection.Instance;

            m_ProgressDialog = new ProgressDialog(this);
            m_ProgressDialog.SetMessage("Connecting with device");
            m_ProgressDialog.SetCancelable(false);

            var font = Typeface.CreateFromAsset(Assets, "SourceSansPro-Light.ttf");

            mBtSearchDevices = FindViewById<Button>(Resource.Id.btSearchDevices);
            mBtSearchDevices.Click += OnSearchDevices;
            mBtSearchDevices.Typeface = font;

            mTvExplanation = FindViewById<TextView>(Resource.Id.tvExplanation);
            mTvExplanation.Typeface = font;

            mTvHeader = FindViewById<TextView>(Resource.Id.tvHeader);
            mTvHeader.Typeface = font;

            // Setting activity background
            m_Linear.SetBackgroundColor(Color.White);

            // Setting background color of the ListView
            m_PeerListView.SetBackgroundColor(Color.WhiteSmoke);
            m_PeerListView.DividerHeight = 14;

            // Adding handler when clicking on a ListViewItem
            m_PeerListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => { OnItemClick(sender, e); };
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            TextView view = (TextView)e.View.FindViewById<TextView>(Resource.Id.tvListItem);
            string address = view.Text.Split('\n')[1];
            BluetoothDevice bluetoothDevice = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(address);
            BuildConnection(bluetoothDevice);
        }

        private void OnSearchDevices(object sender, EventArgs e)
        {
            Intent bluetoothSettings = new Intent();
            bluetoothSettings.SetAction(Android.Provider.Settings.ActionBluetoothSettings);
            StartActivity(bluetoothSettings);
        }

        /// <summary>
        /// Builds a connection to a BluetoothDevice
        /// </summary>
        /// <param name="bluetoothDevice"></param>
        /// <param name="uuid"></param>
        public void BuildConnection(BluetoothDevice bluetoothDevice)
        {
            Toast.MakeText(ApplicationContext, "Connecting...", 0).Show();

            mSocketConnection.BuildConnection(bluetoothDevice);
            if(mSocketConnection.IsConnected == true)
            {
                StartActivity(typeof(ControllerActivity));
            }
            else
            {
                Toast.MakeText(this, "Could not connect to peer", ToastLength.Short).Show();
            }

           /* mSocketConnection.Peer = bluetoothDevice;
            mSocketConnection.Start();

            while (mSocketConnection.IsConnected == false)
            {
                if(mSocketConnection.IsConnectionFailed == true)
                {
                    Toast.MakeText(this, "Could not connect to peer", ToastLength.Short).Show();
                    break;
                }
            }
            if(mSocketConnection.IsConnectionFailed == false)
            {
                StartActivity(typeof(ControllerActivity));
            }*/
        }
    }
}