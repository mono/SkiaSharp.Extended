using System;
using System.Threading;

namespace SkiaSharp.Extended;

/// <summary>
/// Internal abstraction over the current time and scheduled callbacks used by the gesture
/// system. It lets long-press, fling, and zoom timing run against a real timer in production
/// and against a deterministic fake clock in tests. This is not part of the public API.
/// </summary>
internal interface ISKGestureClock
{
	/// <summary>
	/// Gets the current timestamp, in ticks (10,000 ticks per millisecond).
	/// </summary>
	long GetTimestamp();

	/// <summary>
	/// Schedules <paramref name="onTick"/> to be invoked after <paramref name="dueTime"/>, and then
	/// repeatedly every <paramref name="period"/>. A <paramref name="period"/> of
	/// <see cref="TimeSpan.Zero"/> schedules a single (one-shot) callback.
	/// </summary>
	/// <remarks>
	/// Implementations must invoke <paramref name="onTick"/> on the same thread that called
	/// <see cref="Schedule"/> (the UI thread). Dispose the returned handle to cancel; once disposed,
	/// no further callbacks are invoked (including any already marshalled but not yet run).
	/// </remarks>
	IDisposable Schedule(TimeSpan dueTime, TimeSpan period, Action onTick);
}

/// <summary>
/// The default <see cref="ISKGestureClock"/> used in production. Reads wall-clock time from
/// <see cref="DateTime.UtcNow"/> and schedules callbacks with <see cref="Timer"/>, marshalling
/// each callback back to the <see cref="SynchronizationContext"/> captured when
/// <see cref="Schedule"/> was called (the UI thread).
/// </summary>
internal sealed class SystemGestureClock : ISKGestureClock
{
	/// <summary>Gets the shared default instance.</summary>
	public static readonly SystemGestureClock Default = new();

	/// <inheritdoc />
	public long GetTimestamp() => DateTime.UtcNow.Ticks;

	/// <inheritdoc />
	public IDisposable Schedule(TimeSpan dueTime, TimeSpan period, Action onTick)
	{
		if (onTick is null)
			throw new ArgumentNullException(nameof(onTick));

		var context = SynchronizationContext.Current
			?? throw new InvalidOperationException(
				"Gesture timing requires a SynchronizationContext (for example, the UI thread). " +
				"Create and drive the gesture tracker or detector on the UI thread.");

		return new ScheduledTimer(dueTime, period, onTick, context);
	}

	private sealed class ScheduledTimer : IDisposable
	{
		private readonly Action _onTick;
		private readonly SynchronizationContext _context;
		private readonly SendOrPostCallback _post;
		private Timer? _timer;
		private int _disposed;

		public ScheduledTimer(TimeSpan dueTime, TimeSpan period, Action onTick, SynchronizationContext context)
		{
			_onTick = onTick;
			_context = context;
			_post = _ =>
			{
				if (Volatile.Read(ref _disposed) == 0)
					_onTick();
			};

			var repeat = period == TimeSpan.Zero ? Timeout.InfiniteTimeSpan : period;
			_timer = new Timer(OnTimer, null, dueTime, repeat);
		}

		private void OnTimer(object? state)
		{
			if (Volatile.Read(ref _disposed) != 0)
				return;

			_context.Post(_post, null);
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) != 0)
				return;

			var timer = _timer;
			_timer = null;
			timer?.Change(Timeout.Infinite, Timeout.Infinite);
			timer?.Dispose();
		}
	}
}
