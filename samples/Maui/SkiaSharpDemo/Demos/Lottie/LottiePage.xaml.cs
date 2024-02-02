using System.Diagnostics;
using System.Windows.Input;

namespace SkiaSharpDemo.Demos;

public partial class LottiePage : ContentPage
{
	private TimeSpan duration;
	private TimeSpan progress;

	public LottiePage()
	{
		InitializeComponent();

		ResetCommand = new Command(OnReset);
		StepCommand = new Command<string>(OnStep);
		EndCommand = new Command(OnEnd);
		PlayPauseCommand = new Command(OnPlayPause);

		IsBusy = true;

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
		IsBusy = !IsBusy;

	private void OnAnimationFailed(object sender, EventArgs e)
	{
		Debug.WriteLine("Failed to load Lottie animation.");
	}

	private void OnAnimationLoaded(object sender, EventArgs e)
	{
		Debug.WriteLine("Lottie animation loaded.");
	}

	private void OnAnimationCompleted(object sender, EventArgs e)
	{
		Debug.WriteLine("Lottie animation finished playing.");
	}
}
