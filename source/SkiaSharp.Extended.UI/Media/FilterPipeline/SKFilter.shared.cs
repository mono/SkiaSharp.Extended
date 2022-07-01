using System;
using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.Media
{
	public abstract class SKFilter : BindableObject
	{
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
			nameof(IsEnabled),
			typeof(bool),
			typeof(SKFilter),
			true,
			propertyChanged: OnFilterChanged);

		protected SKFilter()
		{
			DebugUtils.LogPropertyChanged(this);
		}

		public bool IsEnabled
		{
			get => (bool)GetValue(IsEnabledProperty);
			set => SetValue(IsEnabledProperty, value);
		}

		protected SKPaint Paint { get; } = new SKPaint();

		public SKPaint GetPaint() => Paint;

		public event EventHandler<SKFilterChangedEventArgs>? FilterChanged;

		protected virtual void OnFilterChanged()
		{
			FilterChanged?.Invoke(this, new SKFilterChangedEventArgs(this));
		}

		private static void OnFilterChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			var filter = (SKFilter)bindable;

			filter.OnFilterChanged();
		}
	}
}
