#if OSX
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif IOS
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace CrossPlatformCircus
{
#if IOS
	[Register("AppDelegate")]
	class Program : UIApplicationDelegate
#else
	static class Program
#endif
	{
#if IOS
		Game1 game;
#endif

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
#if WINDOWS || XBOX || OSX || IOS
		static void Main(string[] args)
		{
	#if IOS
			UIApplication.Main (args, null, "AppDelegate");
	#elif OSX
			NSApplication.Init ();
			
			using (var p = new NSAutoreleasePool ()) {
				NSApplication.SharedApplication.Delegate = new AppDelegate ();
				NSApplication.Main (args);
			}
	#else
			using (Game1 game = new Game1())
			{
				game.Run();
			}
	#endif
		}

#if IOS
		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			game = new Game1 ();
			game.Run ();
		}
#endif

#if OSX
		class AppDelegate : NSApplicationDelegate
		{
			Game1 game;
			
			public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
			{
				game = new Game1 ();
				game.Run ();
			}
			
			public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
			{
				return true;
			}
		}
#endif
#endif

#if WINRT
		static void Main()
		{
			var factory = new MonoGame.Framework.GameFrameworkViewSource<Game1>();
			Windows.ApplicationModel.Core.CoreApplication.Run(factory);
		}
#endif
	}
}

