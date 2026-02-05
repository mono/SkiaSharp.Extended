using System.Diagnostics;
using System.Windows.Input;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos;

public partial class LottiePage : ContentPage
{
	private TimeSpan duration;
	private TimeSpan progress;
	private bool isPlaying;

	public LottiePage()
	{
		InitializeComponent();

		ResetCommand = new Command(OnReset);
		StepCommand = new Command<string>(OnStep);
		EndCommand = new Command(OnEnd);
		PlayPauseCommand = new Command(OnPlayPause);
		SeekFrameCommand = new Command<string>(OnSeekFrame);

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
		}
	}

	public TimeSpan Progress
	{
		get => progress;
		set
		{
			progress = value;
			OnPropertyChanged();
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

	public ICommand ResetCommand { get; }

	public ICommand StepCommand { get; }

	public ICommand PlayPauseCommand { get; }

	public ICommand EndCommand { get; }

	public ICommand SeekFrameCommand { get; }

	private void OnReset() =>
		Progress = TimeSpan.Zero;

	private void OnStep(string step) =>
		Progress += TimeSpan.FromMilliseconds(int.Parse(step));

	private void OnEnd() =>
		Progress = Duration;

	private void OnPlayPause() =>
		IsPlaying = !IsPlaying;

	private void OnSeekFrame(string frame)
	{
		if (int.TryParse(frame, out var frameNumber))
		{
			lottieView.SeekToFrame(frameNumber);
		}
	}

	private void OnAnimationFailed(object sender, SKLottieAnimationFailedEventArgs e)
	{
		Debug.WriteLine($"Failed to load Lottie animation: {e.Exception}");
	}

	private void OnAnimationLoaded(object sender, SKLottieAnimationLoadedEventArgs e)
	{
		Debug.WriteLine($"Lottie animation loaded: {e.Size}; {e.Duration}; {e.Fps}");
	}

	private void OnAnimationCompleted(object sender, EventArgs e)
	{
		Debug.WriteLine("Lottie animation finished playing.");
	}
}
