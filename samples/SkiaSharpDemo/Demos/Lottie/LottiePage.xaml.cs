using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos;

public partial class LottiePage : ContentPage
{
	private static readonly FilePickerFileType JsonFileTypes = new(new Dictionary<DevicePlatform, IEnumerable<string>>
	{
		{ DevicePlatform.iOS, ["public.json"] },
		{ DevicePlatform.MacCatalyst, ["public.json"] },
		{ DevicePlatform.Android, ["application/json"] },
		{ DevicePlatform.WinUI, [".json"] },
	});

	private TimeSpan duration;
	private TimeSpan progress;
	private bool isPlaying = true;
	private bool isLoaded;
	private bool isComplete;
	private bool hasError;
	private double animationSpeed = 1.0;
	private SKLottieRepeatMode repeatMode = SKLottieRepeatMode.Restart;
	private int repeatCount = -1;
	private string statusText = "Loading";
	private string sourceName = "Bundled dotnetbot.json";
	private string naturalSize = "—";
	private string frameRate = "—";
	private string errorText = string.Empty;

	public LottiePage()
	{
		InitializeComponent();

		ResetCommand = new Command(OnReset);
		StepCommand = new Command<string>(OnStep);
		EndCommand = new Command(OnEnd);
		PlayPauseCommand = new Command(OnPlayPause);
		SetSpeedCommand = new Command<string>(OnSetSpeed);

		BindingContext = this;
	}

	public TimeSpan Duration
	{
		get => duration;
		set => SetProperty(ref duration, value);
	}

	public TimeSpan Progress
	{
		get => progress;
		set => SetProperty(ref progress, value);
	}

	public bool IsPlaying
	{
		get => isPlaying;
		set
		{
			if (!SetProperty(ref isPlaying, value))
				return;

			OnPropertyChanged(nameof(PlayPauseText));
			if (!IsComplete && !HasError)
				StatusText = GetPlaybackStatus();
		}
	}

	public bool IsAnimationLoaded
	{
		get => isLoaded;
		private set => SetProperty(ref isLoaded, value);
	}

	public bool IsComplete
	{
		get => isComplete;
		private set
		{
			if (SetProperty(ref isComplete, value))
				OnPropertyChanged(nameof(PlayPauseText));
		}
	}

	public bool HasError
	{
		get => hasError;
		private set => SetProperty(ref hasError, value);
	}

	public double AnimationSpeed
	{
		get => animationSpeed;
		set
		{
			if (!SetProperty(ref animationSpeed, value))
				return;

			if (!IsComplete && !HasError)
				StatusText = GetPlaybackStatus();
		}
	}

	public SKLottieRepeatMode RepeatMode
	{
		get => repeatMode;
		set
		{
			if (SetProperty(ref repeatMode, value))
				OnPropertyChanged(nameof(RepeatSummary));
		}
	}

	public int RepeatCount
	{
		get => repeatCount;
		set
		{
			if (SetProperty(ref repeatCount, value))
				OnPropertyChanged(nameof(RepeatSummary));
		}
	}

	public string StatusText
	{
		get => statusText;
		private set => SetProperty(ref statusText, value);
	}

	public string SourceName
	{
		get => sourceName;
		private set => SetProperty(ref sourceName, value);
	}

	public string NaturalSize
	{
		get => naturalSize;
		private set => SetProperty(ref naturalSize, value);
	}

	public string FrameRate
	{
		get => frameRate;
		private set => SetProperty(ref frameRate, value);
	}

	public string ErrorText
	{
		get => errorText;
		private set => SetProperty(ref errorText, value);
	}

	public string PlayPauseText => IsComplete ? "Replay" : IsPlaying ? "Pause" : "Play";

	public string RepeatSummary =>
		RepeatCount < 0
			? $"{RepeatMode}, infinite"
			: $"{RepeatMode}, {RepeatCount} additional";

	public ICommand ResetCommand { get; }

	public ICommand StepCommand { get; }

	public ICommand PlayPauseCommand { get; }

	public ICommand EndCommand { get; }

	public ICommand SetSpeedCommand { get; }

	private void OnReset()
	{
		Progress = TimeSpan.Zero;
		IsComplete = false;
		IsPlaying = true;
		StatusText = GetPlaybackStatus();
	}

	private void OnStep(string step)
	{
		Progress += TimeSpan.FromMilliseconds(int.Parse(step, CultureInfo.InvariantCulture));
		IsComplete = false;
	}

	private void OnEnd()
	{
		Progress = Duration;
		IsComplete = false;
	}

	private void OnPlayPause()
	{
		if (IsComplete)
		{
			OnReset();
			return;
		}

		IsPlaying = !IsPlaying;
	}

	private void OnSetSpeed(string speed) =>
		AnimationSpeed = double.Parse(speed, CultureInfo.InvariantCulture);

	private void OnAnimationFailed(object sender, SKLottieAnimationFailedEventArgs e)
	{
		Debug.WriteLine($"Failed to load Lottie animation: {e.Exception}");
		IsAnimationLoaded = false;
		IsComplete = false;
		HasError = true;
		StatusText = "Error";
		ErrorText = e.Exception?.Message ?? "The animation could not be loaded.";
		NaturalSize = "—";
		FrameRate = "—";
	}

	private void OnAnimationLoaded(object sender, SKLottieAnimationLoadedEventArgs e)
	{
		Debug.WriteLine($"Lottie animation loaded: {e.Size}; {e.Duration}; {e.Fps}");
		IsAnimationLoaded = true;
		IsComplete = false;
		HasError = false;
		ErrorText = string.Empty;
		NaturalSize = $"{e.Size.Width:0} × {e.Size.Height:0}";
		FrameRate = $"{e.Fps:0.##} fps";
		StatusText = GetPlaybackStatus();
	}

	private void OnAnimationCompleted(object sender, EventArgs e)
	{
		Debug.WriteLine("Lottie animation finished playing.");
		IsComplete = true;
		StatusText = "Complete";
	}

	private async void OnSelectLottieJsonClicked(object sender, EventArgs e)
	{
		try
		{
			var file = await FilePicker.Default.PickAsync(new PickOptions
			{
				PickerTitle = "Select a Lottie JSON animation",
				FileTypes = JsonFileTypes,
			});
			if (file is null)
				return;

			StatusText = "Loading";
			SourceName = file.FileName;
			HasError = false;
			ErrorText = string.Empty;
			lottieView.Source = (SKLottieImageSource)SKLottieImageSource.FromStream(
				async _ => (Stream?)await file.OpenReadAsync());
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Could not select Lottie animation: {ex}");
			HasError = true;
			StatusText = "Error";
			ErrorText = ex.Message;
		}
	}

	private string GetPlaybackStatus() =>
		!IsPlaying || AnimationSpeed == 0 ? "Paused" : "Playing";

	private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(storage, value))
			return false;

		storage = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}
