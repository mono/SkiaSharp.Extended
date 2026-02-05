namespace SkiaSharp.Extended.UI.Controls.Tests;

public static class SKLottieViewExtensions
{
	public static Task WaitForAnimation(this SKLottieView lottieView, int timeout = 3000)
	{
		var tcs = new TaskCompletionSource<bool>();

		var cts = new CancellationTokenSource();
		cts.CancelAfter(timeout);

		var reg = cts.Token.Register(OnTimeout);

		lottieView.AnimationLoaded += OnAnimationLoaded;
		lottieView.AnimationFailed += OnAnimationFailed;

		return tcs.Task;

		void OnAnimationLoaded(object? sender, EventArgs e)
		{
			Cleanup();
			tcs.SetResult(true);
		}

		void OnAnimationFailed(object? sender, SKLottieAnimationFailedEventArgs e)
		{
			Cleanup();
			var message = "Unable to load Lottie animation.";
			if (e.Exception != null)
				tcs.SetException(new Exception(message, e.Exception));
			else
				tcs.SetException(new Exception(message));
		}

		void OnTimeout()
		{
			Cleanup();
			tcs.SetCanceled();
		}

		void Cleanup()
		{
			lottieView.AnimationLoaded -= OnAnimationLoaded;
			lottieView.AnimationFailed -= OnAnimationFailed;
			cts.Dispose();
		}
	}
}
