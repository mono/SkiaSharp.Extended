﻿using Xamarin.Forms;

namespace SkiaSharp.Extended.UI.WPF.Tests
{
	public static class TestUtils
	{
		static TestUtils()
		{
			var app = new System.Windows.Application();

			Xamarin.Forms.Init();
		}

		public static void EnsureFormsInit()
		{
		}
	}
}
