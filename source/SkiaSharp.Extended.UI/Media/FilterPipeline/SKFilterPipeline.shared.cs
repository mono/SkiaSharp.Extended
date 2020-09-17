using System;
using System.Collections.Specialized;
using SkiaSharp.Extended.UI.Media.Extensions;
using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.Media
{
	[ContentProperty(nameof(Filters))]
	public class SKFilterPipeline : BindableObject
	{
		public static readonly BindableProperty FiltersProperty = BindableProperty.Create(
			nameof(Filters),
			typeof(SKFilterCollection),
			typeof(SKFilterPipeline),
			null,
			propertyChanged: OnFiltersChanged,
			defaultValueCreator: CreateDefaultFilters);

		public static readonly BindableProperty SourceProperty = BindableProperty.Create(
			nameof(Source),
			typeof(ImageSource),
			typeof(SKFilterPipeline),
			null,
			propertyChanged: OnSourceChanged);

		private static readonly BindablePropertyKey ImagePropertyKey = BindableProperty.CreateReadOnly(
			nameof(Image),
			typeof(SKImage),
			typeof(SKFilterPipeline),
			null);

		public static readonly BindableProperty ImageProperty = ImagePropertyKey.BindableProperty;

		public SKFilterPipeline()
		{
			DebugUtils.LogPropertyChanged(this);
		}

		public SKFilterCollection? Filters
		{
			get => (SKFilterCollection?)GetValue(FiltersProperty);
			set => SetValue(FiltersProperty, value);
		}

		public ImageSource? Source
		{
			get => (ImageSource?)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		public SKImage? Image
		{
			get => (SKImage?)GetValue(ImageProperty);
			private set => SetValue(ImagePropertyKey, value);
		}

		public event EventHandler? PipelineChanged;

		public event EventHandler<SKFilterChangedEventArgs>? FilterChanged;

		protected virtual void OnPipelineChanged()
		{
			PipelineChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnFilterChanged(SKFilterChangedEventArgs e)
		{
			FilterChanged?.Invoke(this, e);
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			Filters?.SetInheritedBindingContext(BindingContext);
			Source?.SetInheritedBindingContext(BindingContext);
		}

		private static void OnFiltersChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			var pipeline = (SKFilterPipeline)bindable;

			if (oldValue is SKFilterCollection oldCollection)
			{
				oldCollection.CollectionChanged -= OnCollectionChanged;
				oldCollection.FilterChanged -= OnFilterChanged;
				oldCollection.SetInheritedBindingContext(null);
			}

			if (newValue is SKFilterCollection newCollection)
			{
				newCollection.SetInheritedBindingContext(pipeline.BindingContext);
				newCollection.CollectionChanged += OnCollectionChanged;
				newCollection.FilterChanged += OnFilterChanged;
			}

			pipeline.OnPipelineChanged();

			void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				e.OldItems?.SetInheritedBindingContext(null);
				e.NewItems?.SetInheritedBindingContext(pipeline.BindingContext);

				pipeline.OnPipelineChanged();
			}

			void OnFilterChanged(object sender, SKFilterChangedEventArgs e) =>
				pipeline.OnFilterChanged(e);
		}

		private static async void OnSourceChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			var pipeline = (SKFilterPipeline)bindable;

			if (newValue is ImageSource newSource)
			{
				pipeline.Image = await newSource.ToSKImageAsync();

				newSource.SetInheritedBindingContext(null);
			}
			else
			{
				pipeline.Image = null;
			}

			if (oldValue is ImageSource oldSource)
			{
				oldSource?.SetInheritedBindingContext(pipeline.BindingContext);
			}

			pipeline.OnPipelineChanged();
		}

		private static object CreateDefaultFilters(BindableObject bindable)
		{
			var collection = new SKFilterCollection();
			OnFiltersChanged(bindable, null, collection);
			return collection;
		}
	}
}
