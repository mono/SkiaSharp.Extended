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

		void OnAnimationFailed(object? sender, EventArgs e)
		{
			Cleanup();
			tcs.SetException(new Exception("Unable to load Lottie animation."));
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
