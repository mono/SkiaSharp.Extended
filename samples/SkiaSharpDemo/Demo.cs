using System;
using System.Collections.Generic;
using SkiaSharp;
using Xamarin.Forms;

namespace SkiaSharpDemo
{
	public class Demo
	{
		public string Title { get; set; }

		public string Description { get; set; }

		public SKPath ImagePath { get; set; }

		public Color Color { get; set; }

		public Type PageType { get; set; }
	}

	public class DemoGroup : List<Demo>
	{
		public DemoGroup(string name)
		{
			Name = name;
		}

		public string Name { get; set; }
	}
}
