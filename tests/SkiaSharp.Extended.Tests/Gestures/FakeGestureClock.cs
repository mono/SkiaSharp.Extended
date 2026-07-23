using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.Tests.Gestures;

/// <summary>
/// A deterministic <see cref="ISKGestureClock"/> for tests. Virtual time only advances when
/// <see cref="Advance"/> is called, which fires any scheduled callbacks synchronously on the
/// calling thread — so gesture timing (long-press, fling, zoom) is fully deterministic and does
/// not require real delays or a <see cref="System.Threading.SynchronizationContext"/>.
/// </summary>
internal sealed class FakeGestureClock : ISKGestureClock
{
	private readonly List<Scheduled> _scheduled = new();
	private long _ticks;

	public FakeGestureClock(long startTicks = 0) => _ticks = startTicks;

	public long GetTimestamp() => _ticks;

	public IDisposable Schedule(TimeSpan dueTime, TimeSpan period, Action onTick)
	{
		if (onTick is null)
			throw new ArgumentNullException(nameof(onTick));

		var due = dueTime.Ticks < 0 ? 0 : dueTime.Ticks;
		var scheduled = new Scheduled(_ticks + due, period.Ticks, onTick);
		_scheduled.Add(scheduled);
		return scheduled;
	}

	/// <summary>
	/// Advances virtual time by <paramref name="by"/>, firing any due callbacks (including periodic
	/// reschedules) synchronously and in chronological order.
	/// </summary>
	public void Advance(TimeSpan by)
	{
		var target = _ticks + by.Ticks;

		while (true)
		{
			Scheduled? next = null;
			foreach (var s in _scheduled)
			{
				if (s.Removed)
					continue;
				if (s.NextTicks <= target && (next is null || s.NextTicks < next.NextTicks))
					next = s;
			}

			if (next is null)
				break;

			_ticks = next.NextTicks;
			if (next.PeriodTicks > 0)
				next.NextTicks += next.PeriodTicks;
			else
				next.Removed = true;

			next.OnTick(); // may schedule or cancel timers

			_scheduled.RemoveAll(x => x.Removed);
		}

		_ticks = target;
	}

	private sealed class Scheduled : IDisposable
	{
		public long NextTicks;
		public readonly long PeriodTicks;
		public readonly Action OnTick;
		public bool Removed;

		public Scheduled(long nextTicks, long periodTicks, Action onTick)
		{
			NextTicks = nextTicks;
			PeriodTicks = periodTicks;
			OnTick = onTick;
		}

		public void Dispose() => Removed = true;
	}
}
