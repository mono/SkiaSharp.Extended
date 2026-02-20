using SkiaSharp;
using SkiaSharp.Extended.Inking;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos.Inking;

public partial class InkingPage : ContentPage
{
    private SKInkPlayer? player;
    private bool isAnimating;
    private CancellationTokenSource? animationCts;
    private SKColor currentColor = SKColors.Black;
    private SKStrokeCapStyle currentCapStyle = SKStrokeCapStyle.Round;
    private string selectedColorName = "Black";
    private readonly Dictionary<string, Border> colorBorders = new();

    public InkingPage()
    {
        InitializeComponent();
        
        // Initialize color borders dictionary
        colorBorders["Black"] = colorBlack;
        colorBorders["DarkBlue"] = colorDarkBlue;
        colorBorders["Red"] = colorRed;
        colorBorders["Green"] = colorGreen;
        colorBorders["Purple"] = colorPurple;
        colorBorders["Orange"] = colorOrange;
        
        // Set initial selection
        UpdateColorSelection("Black");
    }

    private void OnStrokeCompleted(object? sender, SKSignatureStrokeCompletedEventArgs e)
    {
        // Update UI when stroke is completed
        System.Diagnostics.Debug.WriteLine($"Stroke completed. Total strokes: {e.StrokeCount}");
    }

    private void OnColorTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string colorName)
        {
            UpdateColorSelection(colorName);
            ApplySettings();
        }
    }

    private void UpdateColorSelection(string colorName)
    {
        selectedColorName = colorName;
        
        // Update color
        currentColor = colorName switch
        {
            "Black" => SKColors.Black,
            "DarkBlue" => SKColor.Parse("#00008B"),
            "Red" => SKColors.Red,
            "Green" => SKColor.Parse("#008000"),
            "Purple" => SKColor.Parse("#800080"),
            "Orange" => SKColor.Parse("#FFA500"),
            _ => SKColors.Black
        };

        // Update border visual selection
        foreach (var (name, border) in colorBorders)
        {
            border.StrokeThickness = name == colorName ? 3 : 0;
            border.Stroke = name == colorName ? Colors.DodgerBlue : Colors.Transparent;
        }
    }

    private void OnMinWidthChanged(object? sender, ValueChangedEventArgs e)
    {
        var value = (int)Math.Round(e.NewValue);
        minWidthLabel.Text = value.ToString();
        
        // Ensure max is always >= min
        if (maxWidthSlider.Value < value)
        {
            maxWidthSlider.Value = value;
        }
        
        ApplySettings();
    }

    private void OnMaxWidthChanged(object? sender, ValueChangedEventArgs e)
    {
        var value = (int)Math.Round(e.NewValue);
        maxWidthLabel.Text = value.ToString();
        
        // Ensure min is always <= max
        if (minWidthSlider.Value > value)
        {
            minWidthSlider.Value = value;
        }
        
        ApplySettings();
    }

    private void OnCapStyleTapped(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            // Update cap style based on button
            currentCapStyle = button.Text switch
            {
                "Round" => SKStrokeCapStyle.Round,
                "Flat" => SKStrokeCapStyle.Flat,
                "Tapered" => SKStrokeCapStyle.Tapered,
                _ => SKStrokeCapStyle.Round
            };

            // Update button visual state
            capRound.BackgroundColor = currentCapStyle == SKStrokeCapStyle.Round ? Colors.DodgerBlue : Colors.LightGray;
            capRound.TextColor = currentCapStyle == SKStrokeCapStyle.Round ? Colors.White : Colors.Black;
            
            capFlat.BackgroundColor = currentCapStyle == SKStrokeCapStyle.Flat ? Colors.DodgerBlue : Colors.LightGray;
            capFlat.TextColor = currentCapStyle == SKStrokeCapStyle.Flat ? Colors.White : Colors.Black;
            
            capTapered.BackgroundColor = currentCapStyle == SKStrokeCapStyle.Tapered ? Colors.DodgerBlue : Colors.LightGray;
            capTapered.TextColor = currentCapStyle == SKStrokeCapStyle.Tapered ? Colors.White : Colors.Black;

            ApplySettings();
        }
    }

    private void OnSmoothingChanged(object? sender, ValueChangedEventArgs e)
    {
        var value = (int)Math.Round(e.NewValue);
        smoothingLabel.Text = value.ToString();
        ApplySettings();
    }

    private void ApplySettings()
    {
        // Convert SKColor to MAUI Color for the bindable property
        var mauiColor = Color.FromRgba(
            currentColor.Red / 255.0,
            currentColor.Green / 255.0,
            currentColor.Blue / 255.0,
            currentColor.Alpha / 255.0);
        
        // Apply settings to the signature pad's bindable properties
        signaturePad.StrokeColor = mauiColor;
        signaturePad.MinStrokeWidth = (float)minWidthSlider.Value;
        signaturePad.MaxStrokeWidth = (float)maxWidthSlider.Value;
        
        // Apply to ink canvas brush for cap style and smoothing
        signaturePad.InkCanvas.Brush.CapStyle = currentCapStyle;
        signaturePad.InkCanvas.Brush.SmoothingFactor = (int)Math.Round(smoothingSlider.Value);
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        StopAnimation();
        signaturePad.Clear();
    }

    private void OnUndoClicked(object? sender, EventArgs e)
    {
        StopAnimation();
        signaturePad.Undo();
    }

    private async void OnPlaySignatureClicked(object? sender, EventArgs e)
    {
        if (isAnimating)
        {
            StopAnimation();
            return;
        }

        // Clear the pad first
        signaturePad.Clear();

        // Create a sample signature recording
        var bounds = signaturePad.GetStrokeBounds();
        float width = (float)signaturePad.Width;
        float height = (float)signaturePad.Height;
        
        if (width <= 0) width = 400;
        if (height <= 0) height = 200;

        var recording = SKInkRecording.CreateSampleSignature(width, height);

        // Set up the player
        player = new SKInkPlayer();
        player.PlaybackSpeed = 1.5f;
        player.Load(recording, signaturePad.InkCanvas);
        player.PlaybackCompleted += OnPlaybackCompleted;

        // Start playback
        isAnimating = true;
        playButton.Text = "Stop";
        player.Play();

        // Create cancellation token for animation loop
        animationCts = new CancellationTokenSource();
        var token = animationCts.Token;

        // Run animation on a background task to not block UI
        try
        {
            await Task.Run(async () =>
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                while (!token.IsCancellationRequested && player.IsPlaying)
                {
                    var frameStart = stopwatch.ElapsedMilliseconds;

                    // Update player state
                    var hasMore = player.Update();

                    // Request UI update on main thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            signaturePad.Invalidate();
                        }
                    });

                    if (!hasMore)
                        break;

                    // Calculate remaining time to maintain ~60 FPS
                    var elapsed = stopwatch.ElapsedMilliseconds - frameStart;
                    var delay = Math.Max(1, 16 - (int)elapsed);
                    await Task.Delay(delay, token).ConfigureAwait(false);
                }
            }, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Animation was cancelled
        }

        // Ensure final state is rendered
        MainThread.BeginInvokeOnMainThread(() =>
        {
            signaturePad.Invalidate();
            if (!player?.IsPlaying ?? true)
            {
                StopAnimation();
            }
        });
    }

    private void OnPlaybackCompleted(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StopAnimation();
        });
    }

    private void StopAnimation()
    {
        isAnimating = false;
        animationCts?.Cancel();
        animationCts?.Dispose();
        animationCts = null;
        playButton.Text = "Play Signature";
        
        if (player != null)
        {
            player.PlaybackCompleted -= OnPlaybackCompleted;
            player = null;
        }
    }
}
