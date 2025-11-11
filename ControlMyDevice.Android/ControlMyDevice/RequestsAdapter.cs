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

namespace ControlMyDevice
{
	public class RequestsAdapter : BaseAdapter<RequestItem>
	{
		private List<RequestItem> _items;
		private Activity _context;
		private Func<int, int> _acceptClick;
		private Func<int, int> _rejectClick;

		public RequestsAdapter (Activity context, List<RequestItem> items, Func<int, int> acceptClick, Func<int, int> rejectClick)
		{
			_context = context;
			_items = items;
			_acceptClick = acceptClick;
			_rejectClick = rejectClick;
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override RequestItem this[int index] {
			get {
				return _items [index];
			}
		}

		public override int Count {
			get {
				return _items.Count;
			}
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			View view = convertView;
			if (view == null)
				view = _context.LayoutInflater.Inflate (Resource.Layout.RequestItem, null);
			TextView userEmail = view.FindViewById<TextView> (Resource.Id.userEmail);
			userEmail.Text = _items [position].UserEmail;

			Button btnAccept = view.FindViewById<Button> (Resource.Id.btnAccept);
			btnAccept.Click += (object sender, EventArgs e) => {
				_acceptClick(_items [position].DeviceUserRequestId);
			};

			Button btnReject = view.FindViewById<Button> (Resource.Id.btnReject);
			btnReject.Click += (object sender, EventArgs e) => {
				_rejectClick(_items [position].DeviceUserRequestId);
			};

			return view;
		}
	}
}

