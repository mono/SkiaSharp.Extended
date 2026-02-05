using System.Diagnostics;
using System.Windows.Input;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos;

public partial class LottiePage : ContentPage
{
	private TimeSpan duration;
	private TimeSpan progress;
	private bool isPlaying;
	private string currentFrameInfo = string.Empty;

	public LottiePage()
	{
		InitializeComponent();

		ResetCommand = new Command(OnReset);
		StepCommand = new Command<string>(OnStep);
		EndCommand = new Command(OnEnd);
		PlayPauseCommand = new Command(OnPlayPause);
		SeekToFrameCommand = new Command<string>(OnSeekToFrame);
		SeekToProgressCommand = new Command<string>(OnSeekToProgress);
		SeekToLastFrameCommand = new Command(OnSeekToLastFrame);

		IsPlaying = true;

		BindingContext = this;
	}

	public TimeSpan Duration
	{
		get => duration;
		set
		{
			duration = value;
			OnPropertyChanged();
			UpdateFrameInfo();
		}
	}

	public TimeSpan Progress
	{
		get => progress;
		set
		{
			progress = value;
			OnPropertyChanged();
			UpdateFrameInfo();
		}
	}

	public bool IsPlaying
	{
		get => isPlaying;
		set
		{
			isPlaying = value;
			OnPropertyChanged();
		}
	}

	public string CurrentFrameInfo
	{
		get => currentFrameInfo;
		set
		{
			currentFrameInfo = value;
			OnPropertyChanged();
		}
	}

	public ICommand ResetCommand { get; }

	public ICommand StepCommand { get; }

	public ICommand PlayPauseCommand { get; }

	public ICommand EndCommand { get; }

	public ICommand SeekToFrameCommand { get; }

	public ICommand SeekToProgressCommand { get; }

	public ICommand SeekToLastFrameCommand { get; }

	private void OnReset() =>
		Progress = TimeSpan.Zero;

	private void OnStep(string step) =>
		Progress += TimeSpan.FromMilliseconds(int.Parse(step));

	private void OnEnd() =>
		Progress = Duration;

	private void OnPlayPause() =>
		IsPlaying = !IsPlaying;

	private void OnSeekToFrame(string frameStr)
	{
		if (lottieView is not null && int.TryParse(frameStr, out var frame))
		{
			lottieView.SeekToFrame(frame, stopPlayback: true);
			IsPlaying = false;
		}
	}

	private void OnSeekToProgress(string progressStr)
	{
		if (lottieView is not null && double.TryParse(progressStr, out var prog))
		{
			lottieView.SeekToProgress(prog, stopPlayback: true);
			IsPlaying = false;
		}
	}

	private void OnSeekToLastFrame()
	{
		if (lottieView?.FrameCount > 0)
		{
			lottieView.SeekToFrame(lottieView.FrameCount - 1, stopPlayback: true);
			IsPlaying = false;
		}
	}

	private void UpdateFrameInfo()
	{
		if (lottieView?.FrameCount > 0)
		{
			CurrentFrameInfo = $"Frame: {lottieView.CurrentFrame} / {lottieView.FrameCount} ({lottieView.Fps:F1} fps)";
		}
	}

	private void OnAnimationFailed(object sender, SKLottieAnimationFailedEventArgs e)
	{
		Debug.WriteLine($"Failed to load Lottie animation: {e.Exception}");
	}

	private void OnAnimationLoaded(object sender, SKLottieAnimationLoadedEventArgs e)
	{
		Debug.WriteLine($"Lottie animation loaded: {e.Size}; {e.Duration}; {e.Fps}");
		UpdateFrameInfo();
	}

	private void OnAnimationCompleted(object sender, EventArgs e)
	{
		Debug.WriteLine("Lottie animation finished playing.");
	}
}
