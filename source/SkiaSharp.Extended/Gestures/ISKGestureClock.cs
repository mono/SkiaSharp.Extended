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
	/// Implementations must serialize callbacks. Production callers should marshal them to the
	/// tracker owner thread when UI thread affinity is required. Dispose the returned handle to
	/// cancel; once disposed, no further callbacks are invoked (including any already marshalled
	/// but not yet run).
	/// </remarks>
	IDisposable Schedule(TimeSpan dueTime, TimeSpan period, Action onTick);
}

/// <summary>
/// The default <see cref="ISKGestureClock"/> used in production. Reads wall-clock time from
/// <see cref="DateTime.UtcNow"/> and schedules callbacks with <see cref="Timer"/>. Callbacks are
/// marshalled through an explicitly supplied dispatcher or through the
/// <see cref="SynchronizationContext"/> active when <see cref="Schedule"/> is called. For
/// compatibility, context-free callers run serialized callbacks on timer threads; UI hosts
/// without a synchronization context should supply a dispatcher.
/// </summary>
internal sealed class SystemGestureClock : ISKGestureClock
{
	/// <summary>Gets the shared default instance.</summary>
	public static readonly SystemGestureClock Default = new();

	private readonly Action<Action>? _dispatcher;

	public SystemGestureClock()
	{
	}

	public SystemGestureClock(Action<Action> dispatcher)
	{
		_dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
	}

	/// <inheritdoc />
	public long GetTimestamp() => DateTime.UtcNow.Ticks;

	/// <inheritdoc />
	public IDisposable Schedule(TimeSpan dueTime, TimeSpan period, Action onTick)
	{
		if (onTick is null)
			throw new ArgumentNullException(nameof(onTick));

		var context = SynchronizationContext.Current;
		var runInline = _dispatcher is null && context is null;

		return new ScheduledTimer(dueTime, period, onTick, _dispatcher, context, runInline);
	}

	private sealed class ScheduledTimer : IDisposable
	{
		private readonly Action _onTick;
		private readonly Action<Action>? _dispatcher;
		private readonly SynchronizationContext? _context;
		private readonly SendOrPostCallback _post;
		private readonly bool _runInline;
		private Timer? _timer;
		private int _disposed;
		private int _callbackPending;

		public ScheduledTimer(
			TimeSpan dueTime,
			TimeSpan period,
			Action onTick,
			Action<Action>? dispatcher,
			SynchronizationContext? context,
			bool runInline)
		{
			_onTick = onTick;
			_dispatcher = dispatcher;
			_context = context;
			_runInline = runInline;
			_post = _ =>
			{
				try
				{
					if (Volatile.Read(ref _disposed) == 0)
						_onTick();
				}
				finally
				{
					Volatile.Write(ref _callbackPending, 0);
				}
			};

			var repeat = period == TimeSpan.Zero ? Timeout.InfiniteTimeSpan : period;
			_timer = new Timer(OnTimer, null, dueTime, repeat);
		}

		private void OnTimer(object? state)
		{
			if (Volatile.Read(ref _disposed) != 0)
				return;

			// Coalesce timer ticks while the previous callback is queued or executing. This
			// prevents concurrent access even when a dispatcher is temporarily backlogged.
			if (Interlocked.CompareExchange(ref _callbackPending, 1, 0) != 0)
				return;

			try
			{
				if (_dispatcher != null)
					_dispatcher(() => _post(null));
				else if (_context != null)
					_context.Post(_post, null);
				else if (_runInline)
					_post(null);
			}
			catch
			{
				Volatile.Write(ref _callbackPending, 0);
				throw;
			}
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
