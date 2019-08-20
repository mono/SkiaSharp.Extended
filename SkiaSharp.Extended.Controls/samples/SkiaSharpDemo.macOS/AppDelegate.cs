using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace SkiaSharpDemo.macOS
{
	[Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
	{
		private NSWindow window;

		public AppDelegate()
		{
			var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
			var rect = new CoreGraphics.CGRect(0, 0, 800, 600);

			window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			window.Title = "SkiaSharp";
			window.TitleVisibility = NSWindowTitleVisibility.Hidden;
			window.Center();
		}

		public override NSWindow MainWindow => window;

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

		public override void DidFinishLaunching(NSNotification notification)
		{
			Forms.Init();
			LoadApplication(new App());
			base.DidFinishLaunching(notification);
		}
	}
}
