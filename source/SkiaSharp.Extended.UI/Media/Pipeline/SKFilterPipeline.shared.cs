using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SkiaSharp.Extended.UI.Media
{
	public class SKFilterPipeline : ObservableCollection<SKFilter>
	{
		public SKFilterPipeline()
		{
		}

		public SKFilterPipeline(IEnumerable<SKFilter> collection)
			: base(collection)
		{
		}

		public SKFilterPipeline(List<SKFilter> list)
			: base(list)
		{
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnCollectionChanged(e);

			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems)
				{
					if (item is SKFilter filter)
						filter.FilterChanged += OnPipelineChanged;
				}
			}

			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems)
				{
					if (item is SKFilter filter)
						filter.FilterChanged -= OnPipelineChanged;
				}
			}
		}

		private void OnPipelineChanged(object sender, SKFilterChangedEventArgs e)
		{
			PipelineChanged?.Invoke(sender, e);
		}

		public event EventHandler<SKFilterChangedEventArgs>? PipelineChanged;
	}
}
