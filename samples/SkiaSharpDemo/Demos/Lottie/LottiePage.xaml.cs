using System.Diagnostics;
using System.Windows.Input;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos;

public partial class LottiePage : ContentPage
{
	private TimeSpan duration;
	private TimeSpan progress;
	private bool isPlaying;
	private double animationSpeed = 1.0;
	private SKLottieRepeatMode repeatMode = SKLottieRepeatMode.Restart;
	private int repeatCount = -1;
	private int totalFrameCount = 0; // full frame count (before any FrameStart/FrameEnd)

	public LottiePage()
	{
		InitializeComponent();

		ResetCommand = new Command(OnReset);
		StepCommand = new Command<string>(OnStep);
		EndCommand = new Command(OnEnd);
		PlayPauseCommand = new Command(OnPlayPause);
		SetSpeedCommand = new Command<string>(OnSetSpeed);
		SetFrameRangeCommand = new Command<string>(OnSetFrameRange);
		ResetFrameRangeCommand = new Command(OnResetFrameRange);

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

	public double AnimationSpeed
	{
		get => animationSpeed;
		set
		{
			animationSpeed = value;
			OnPropertyChanged();
		}
	}

	public SKLottieRepeatMode RepeatMode
	{
		get => repeatMode;
		set
		{
			repeatMode = value;
			OnPropertyChanged();
		}
	}

	public int RepeatCount
	{
		get => repeatCount;
		set
		{
			repeatCount = value;
			OnPropertyChanged();
		}
	}

	public ICommand ResetCommand { get; }

	public ICommand StepCommand { get; }

	public ICommand PlayPauseCommand { get; }

	public ICommand EndCommand { get; }

	public ICommand SetSpeedCommand { get; }

	public ICommand SetFrameRangeCommand { get; }

	public ICommand ResetFrameRangeCommand { get; }

	private void OnReset() =>
		Progress = TimeSpan.Zero;

	private void OnStep(string step) =>
		Progress += TimeSpan.FromMilliseconds(int.Parse(step));

	private void OnEnd() =>
		Progress = Duration;

	private void OnPlayPause() =>
		IsPlaying = !IsPlaying;

	private void OnSetSpeed(string speed) =>
		AnimationSpeed = double.Parse(speed);

	private void OnSetFrameRange(string which)
	{
		var mid = totalFrameCount / 2;
		if (which == "first")
		{
			lottieView.FrameStart = 0;
			lottieView.FrameEnd = mid;
		}
		else
		{
			lottieView.FrameStart = mid;
			lottieView.FrameEnd = -1;
		}
	}

	private void OnResetFrameRange()
	{
		lottieView.FrameStart = 0;
		lottieView.FrameEnd = -1;
	}

	private void OnAnimationFailed(object sender, SKLottieAnimationFailedEventArgs e)
	{
		Debug.WriteLine($"Failed to load Lottie animation: {e.Exception}");
	}

	private void OnAnimationLoaded(object sender, SKLottieAnimationLoadedEventArgs e)
	{
		// Capture the full frame count before any FrameStart/FrameEnd narrowing
		totalFrameCount = lottieView.FrameCount;
		Debug.WriteLine($"Lottie animation loaded: {e.Size}; {e.Duration}; {e.Fps}");
	}

	private void OnAnimationCompleted(object sender, EventArgs e)
	{
		Debug.WriteLine("Lottie animation finished playing.");
	}
}
