using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public class FilterDataTemplateSelector : DataTemplateSelector
	{
		public FilterDataTemplateCollection Templates { get; } = new FilterDataTemplateCollection();

		public DataTemplate? Unknown { get; set; }

		protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
		{
			if (item != null)
			{
				var type = item.GetType();
				var template = Templates.FirstOrDefault(t => t.FilterType == type)?.Template;
				if (template != null)
					return template;
			}

			return Unknown;
		}
	}

	public class FilterDataTemplateCollection : List<FilterDataTemplate>
	{
	}

	[ContentProperty(nameof(Template))]
	public class FilterDataTemplate : BindableObject
	{
		public Type? FilterType { get; set; }

		public DataTemplate? Template { get; set; }
	}
}
