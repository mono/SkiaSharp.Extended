using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos;

public partial class LottiePage : ContentPage
{
	private TimeSpan duration;
	private TimeSpan progress;
	private bool isPlaying;
	private SKLottieImageSource? lottieSource;
	private string selectedAnimationType = "Base64 Embedded (dotnetbot)";
	private string animationDescription = "Images embedded as base64 in JSON";

	public LottiePage()
	{
		InitializeComponent();

		ResetCommand = new Command(OnReset);
		StepCommand = new Command<string>(OnStep);
		EndCommand = new Command(OnEnd);
		PlayPauseCommand = new Command(OnPlayPause);

		AnimationTypes = new ObservableCollection<string>
		{
			"Base64 Embedded (dotnetbot)",
			"External Images (File System)",
			".lottie Format (ZIP)"
		};

		IsPlaying = true;

		// Set initial animation
		SelectedAnimationType = AnimationTypes[0];
		LoadSelectedAnimation();

		BindingContext = this;
	}

	public ObservableCollection<string> AnimationTypes { get; }

	public string SelectedAnimationType
	{
		get => selectedAnimationType;
		set
		{
			if (selectedAnimationType != value)
			{
				selectedAnimationType = value;
				OnPropertyChanged();
				UpdateAnimationDescription();
			}
		}
	}

	public string AnimationDescription
	{
		get => animationDescription;
		set
		{
			if (animationDescription != value)
			{
				animationDescription = value;
				OnPropertyChanged();
			}
		}
	}

	public SKLottieImageSource? LottieSource
	{
		get => lottieSource;
		set
		{
			if (lottieSource != value)
			{
				lottieSource = value;
				OnPropertyChanged();
			}
		}
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

	private void OnReset() =>
		Progress = TimeSpan.Zero;

	private void OnStep(string step) =>
		Progress += TimeSpan.FromMilliseconds(int.Parse(step));

	private void OnEnd() =>
		Progress = Duration;

	private void OnPlayPause() =>
		IsPlaying = !IsPlaying;

	private void OnAnimationTypeChanged(object? sender, EventArgs e)
	{
		LoadSelectedAnimation();
	}

	private void LoadSelectedAnimation()
	{
		// Reset playback
		Progress = TimeSpan.Zero;
		IsPlaying = true;

		// Load the selected animation type
		switch (SelectedAnimationType)
		{
			case "Base64 Embedded (dotnetbot)":
				LottieSource = new SKFileLottieImageSource
				{
					File = "Lottie/dotnetbot.json"
				};
				break;

			case "External Images (File System)":
				// Note: This demonstrates the limitation - we need to extract images
				// from app package to file system first in a real MAUI app
				// For the demo, we're using relative paths that work in the test environment
				LottieSource = new SKFileLottieImageSource
				{
					File = "Lottie/with-external-images.json",
					ImageAssetsFolder = "Lottie"
				};
				break;

			case ".lottie Format (ZIP)":
				// SKFileLottieImageSource now auto-detects .lottie format
				// Uses real dotnetbot.lottie with 10 embedded images
				LottieSource = new SKFileLottieImageSource
				{
					File = "Lottie/dotnetbot.lottie"
				};
				break;
		}
	}

	private void UpdateAnimationDescription()
	{
		AnimationDescription = SelectedAnimationType switch
		{
			"Base64 Embedded (dotnetbot)" => "Images embedded as base64 data URIs in JSON file",
			"External Images (File System)" => "Images loaded from file system using ImageAssetsFolder property",
			".lottie Format (ZIP)" => "Animation with 10 images bundled in .lottie ZIP container (dotLottie v1.0)",
			_ => ""
		};
	}

	private void OnAnimationFailed(object sender, SKLottieAnimationFailedEventArgs e)
	{
		Debug.WriteLine($"Failed to load Lottie animation: {e.Exception}");
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await DisplayAlert("Animation Error", 
				$"Failed to load animation: {e.Exception?.Message}", 
				"OK");
		});
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
