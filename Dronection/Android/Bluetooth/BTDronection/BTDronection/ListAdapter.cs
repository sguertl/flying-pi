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
using Android.Graphics;

namespace BTDronection
{
    public class ListAdapter : BaseAdapter<string>
    {
        private List<string> mDevices;
        private Activity mContext;
        private Typeface mFont;

        public ListAdapter(Activity context, List<string> deviceList)
        {
            mContext = context;
            mDevices = deviceList.ToList();
            mFont = Typeface.CreateFromAsset(mContext.Assets, "SourceSansPro-Light.ttf");
        }

        public void DeleteElement(string element)
        {
            mDevices.Remove(element);
        }

        public override string this[int position]
        {
            get { return mDevices[position]; }
        }

        public override int Count
        {
            get { return mDevices.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            string text = mDevices[position];
            View costumView = convertView;
            if(costumView == null)
            {
                LayoutInflater inflater = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
                costumView = inflater.Inflate(Resource.Layout.CustomListItemLayout, parent, false);
            }
            int id = Resource.Id.tvListItem;
            costumView.FindViewById<TextView>(id).Text = text;
            costumView.FindViewById<TextView>(id).Typeface = mFont;
            return costumView;
        }
    }
}