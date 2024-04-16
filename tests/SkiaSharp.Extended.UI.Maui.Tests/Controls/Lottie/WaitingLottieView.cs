namespace SkiaSharp.Extended.UI.Controls.Tests;

class WaitingLottieView : SKLottieView
{
	public WaitingLottieView()
	{
		ResetTask();
	}

	public Task LoadedTask { get; private set; } = null!;

	public void ResetTask() =>
		LoadedTask = this.WaitForAnimation();

	public void CallUpdate(TimeSpan deltaTime) =>
		Update(deltaTime);
}
