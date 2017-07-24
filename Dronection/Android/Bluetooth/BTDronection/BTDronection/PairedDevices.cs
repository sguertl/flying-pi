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
    public class PairedDevices : Activity, IEstablishConnection
    {
        // Members
        private ListView m_ListView;
        private BluetoothAdapter m_BtAdapter;
        private List<BluetoothDevice> m_PairedDevice;
        private ArrayAdapter<String> m_Adapter;
        private LinearLayout m_Linear;
        private List<String> m_List;
        private List<String> m_UuidList;
        private bool m_IsConnected;
        private ProgressDialog m_ProgressDialog;
        private Button mBtSearchDevices;
        private TextView mTvExplanation;
        private TextView mTvHeader;


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
                m_List.Add(device.Name + "\n" + device.Address);
            }

            m_Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, m_List);
            m_ListView.Adapter = m_Adapter;
        }

        /// <summary>
        /// Initializes and modifies objects
        /// </summary>
        public void Init()
        {
            // Initializing objects
            m_ListView = FindViewById<ListView>(Resource.Id.listView);
            m_Linear = FindViewById<LinearLayout>(Resource.Id.linear2);
            m_BtAdapter = BluetoothAdapter.DefaultAdapter;
            m_PairedDevice = m_BtAdapter.BondedDevices.Where(bd => bd.Name.ToUpper().Contains("RASPBERRY") || bd.Name.ToUpper().Contains("RPI") || bd.Name.ToUpper().Contains("XMC")).ToList();
            m_List = new List<String>();
            m_UuidList = new List<String>();
            m_IsConnected = true;

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
            m_ListView.SetBackgroundColor(Color.WhiteSmoke);
            m_ListView.DividerHeight = 14;

            // Adding handler when clicking on a ListViewItem
            m_ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => { OnItemClick(sender, e); };
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            TextView view = (TextView)e.View;
            string address = view.Text.Split('\n')[1];
            BluetoothDevice bluetoothDevice = BluetoothAdapter.DefaultAdapter.GetRemoteDevice(address);
            BuildConnection(bluetoothDevice, bluetoothDevice.GetUuids()[0].Uuid.ToString());
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
        public void BuildConnection(BluetoothDevice bluetoothDevice, string uuid)
        {
            Toast.MakeText(ApplicationContext, "Connecting...", 0).Show();
            // Creating a ConnectionThread object
            ConnectedThread connect = new ConnectedThread(bluetoothDevice, uuid, this);
            connect.Start();

            while (!ConnectedThread.m_Socket.IsConnected) { if (ConnectedThread.m_FailedCon) break; }
            if (!ConnectedThread.m_FailedCon)
            {
                /*var activity2 = new Intent(this, typeof(ConnectedDevices));
                IList<String> ll = new List<string>();
                ll.Add(bluetoothDevice.Name);
                ll.Add(bluetoothDevice.Address);
                activity2.PutStringArrayListExtra("MyData", ll);
                StartActivity(activity2);*/
                StartActivity(typeof(ControllerActivity));
            }
        }
    }
}