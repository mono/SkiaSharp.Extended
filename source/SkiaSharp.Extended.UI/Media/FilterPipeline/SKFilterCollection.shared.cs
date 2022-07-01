using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SkiaSharp.Extended.UI.Media
{
	public class SKFilterCollection : ObservableCollection<SKFilter>
	{
		public SKFilterCollection()
		{
			DebugUtils.LogPropertyChanged(this);
		}

		public SKFilterCollection(IEnumerable<SKFilter> collection)
			: base(collection)
		{
			DebugUtils.LogPropertyChanged(this);
		}

		public event EventHandler<SKFilterChangedEventArgs>? FilterChanged;

		protected virtual void OnFilterChanged(SKFilterChangedEventArgs e)
		{
			FilterChanged?.Invoke(this, e);
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnCollectionChanged(e);

			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems)
				{
					if (item is SKFilter filter)
						filter.FilterChanged -= OnChanged;
				}
			}

			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems)
				{
					if (item is SKFilter filter)
						filter.FilterChanged += OnChanged;
				}
			}

			void OnChanged(object sender, SKFilterChangedEventArgs e)
			{
				OnFilterChanged(e);
			}
		}
	}
}
