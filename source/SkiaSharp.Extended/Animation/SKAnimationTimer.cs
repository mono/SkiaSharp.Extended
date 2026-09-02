using System;
using System.Threading;

namespace SkiaSharp.Extended;

/// <summary>
/// A reusable timer-based animation runner that handles the timer, cancellation token,
/// and <see cref="SynchronizationContext"/> dispatch in one place.
/// Used internally by <see cref="SKGestureTracker"/> for fling and zoom animations.
/// </summary>
internal sealed class SKAnimationTimer : IDisposable
{
	private Timer? _timer;
	private int _token;
	private bool _disposed;

	/// <summary>Whether the animation timer is currently running.</summary>
	public bool IsRunning { get; private set; }

	/// <summary>
	/// Starts (or restarts) the animation timer.
	/// The <paramref name="tick"/> callback is invoked on the provided <paramref name="syncContext"/>
	/// (if non-null) or on the timer thread pool thread.
	/// Any previously running timer is stopped before starting the new one.
	/// </summary>
	/// <param name="interval">The interval between ticks.</param>
	/// <param name="tick">The callback to invoke on each tick.</param>
	/// <param name="syncContext">Optional synchronization context to marshal the callback onto.</param>
	public void Start(TimeSpan interval, Action tick, SynchronizationContext? syncContext = null)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(SKAnimationTimer));

		StopCore(); // cancel any running timer

		IsRunning = true;
		var token = Interlocked.Increment(ref _token);
		var ms = (int)interval.TotalMilliseconds;

		_timer = new Timer(_ =>
		{
			if (token != Volatile.Read(ref _token) || !IsRunning || _disposed)
				return;

			if (syncContext != null)
			{
				syncContext.Post(__ =>
				{
					if (token == Volatile.Read(ref _token) && IsRunning && !_disposed)
						tick();
				}, null);
			}
			else
			{
				if (IsRunning && !_disposed)
					tick();
			}
		}, null, ms, ms);
	}

	/// <summary>Stops the animation timer. Safe to call when not running.</summary>
	public void Stop()
	{
		if (!IsRunning)
			return;
		StopCore();
	}

	private void StopCore()
	{
		IsRunning = false;
		Interlocked.Increment(ref _token);
		var t = _timer;
		_timer = null;
		t?.Change(Timeout.Infinite, Timeout.Infinite);
		t?.Dispose();
	}

	public void Dispose()
	{
		if (_disposed)
			return;
		_disposed = true;
		StopCore();
	}
}
