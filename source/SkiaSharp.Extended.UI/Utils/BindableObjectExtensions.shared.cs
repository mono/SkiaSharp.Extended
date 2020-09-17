using System.Collections;
using Xamarin.Forms;

namespace SkiaSharp.Extended.UI
{
	internal static class BindableObjectExtensions
	{
		public static void SetInheritedBindingContext(this BindableObject obj, object? bindingContext)
		{
			BindableObject.SetInheritedBindingContext(obj, bindingContext);
		}

		public static void SetInheritedBindingContext(this IList? children, object? bindingContext)
		{
			if (children?.Count > 0)
			{
				foreach (var item in children)
				{
					if (item is BindableObject bo)
					{
						BindableObject.SetInheritedBindingContext(bo, bindingContext);
					}
				}
			}
		}
	}
}
