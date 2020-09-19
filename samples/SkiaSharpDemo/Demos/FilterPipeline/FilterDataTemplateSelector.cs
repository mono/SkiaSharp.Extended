using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkiaSharp.Extended.UI.Media;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public class FilterDataTemplateSelector : DataTemplateSelector
	{
		private readonly Dictionary<Type, DataTemplate> templates = new Dictionary<Type, DataTemplate>();
		private DataTemplate? unknownTemplate;

		public FilterDataTemplateCollection Templates { get; } = new FilterDataTemplateCollection();

		public BindableObject? Unknown { get; set; }

		protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
		{
			if (item != null)
			{
				var type = item.GetType();
				var templateObject = Templates.FirstOrDefault(t => t.FilterType == type)?.Template;
				if (templateObject != null)
				{
					if (!templates.TryGetValue(type, out var template))
					{
						template = new DataTemplate(() => templateObject);
						templates[type] = template;
					}
					return template;
				}
			}

			if (unknownTemplate == null)
				unknownTemplate = new DataTemplate(() => Unknown);
			return unknownTemplate;
		}
	}

	public class FilterDataTemplateCollection : List<FilterDataTemplate>
	{
	}

	[ContentProperty(nameof(Template))]
	public class FilterDataTemplate : BindableObject
	{
		public Type? FilterType { get; set; }

		public BindableObject? Template { get; set; }
	}

	public class EnabledOpacityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value is SKFilter filter && filter.IsEnabled ? 1.0 : 0.5;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotSupportedException();
	}
}
