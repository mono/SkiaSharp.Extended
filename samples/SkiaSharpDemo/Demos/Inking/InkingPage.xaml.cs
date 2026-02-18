using SkiaSharp.Extended.Inking;
using SkiaSharp.Extended.UI.Controls;

namespace SkiaSharpDemo.Demos.Inking;

public partial class InkingPage : ContentPage
{
    private SKInkPlayer? player;
    private bool isAnimating;

    public InkingPage()
    {
        InitializeComponent();
    }

    private void OnStrokeCompleted(object? sender, SKSignatureStrokeCompletedEventArgs e)
    {
        // Update UI when stroke is completed
        System.Diagnostics.Debug.WriteLine($"Stroke completed. Total strokes: {e.StrokeCount}");
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

        // Animation loop with frame timing
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (isAnimating && player.IsPlaying)
        {
            var frameStart = stopwatch.ElapsedMilliseconds;
            
            player.Update();
            signaturePad.Invalidate();
            
            // Calculate remaining time to maintain ~60 FPS
            var elapsed = stopwatch.ElapsedMilliseconds - frameStart;
            var delay = Math.Max(1, 16 - (int)elapsed);
            await Task.Delay(delay);
        }

        if (!player.IsPlaying)
        {
            StopAnimation();
        }
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
        playButton.Text = "Play Signature";
        
        if (player != null)
        {
            player.PlaybackCompleted -= OnPlaybackCompleted;
            player = null;
        }
    }
}
