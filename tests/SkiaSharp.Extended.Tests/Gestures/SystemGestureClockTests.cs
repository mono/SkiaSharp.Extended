using System;
using System.Threading;
using Xunit;

namespace SkiaSharp.Extended.Tests.Gestures;

public class SystemGestureClockTests
{
	[Fact]
	public void Schedule_WithoutContextOrDispatcher_SerializesCallbacks()
	{
		var previousContext = SynchronizationContext.Current;
		SynchronizationContext.SetSynchronizationContext(null);

		try
		{
			var activeCallbacks = 0;
			var maxActiveCallbacks = 0;
			var callbackCount = 0;
			using var completed = new ManualResetEventSlim();
			using var registration = SystemGestureClock.Default.Schedule(
				TimeSpan.Zero,
				TimeSpan.FromMilliseconds(1),
				() =>
				{
					var active = Interlocked.Increment(ref activeCallbacks);
					UpdateMaximum(ref maxActiveCallbacks, active);
					Thread.Sleep(10);
					Interlocked.Decrement(ref activeCallbacks);
					if (Interlocked.Increment(ref callbackCount) >= 2)
						completed.Set();
				});

			Assert.True(completed.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
			Assert.Equal(1, Volatile.Read(ref maxActiveCallbacks));
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(previousContext);
		}
	}

	[Fact]
	public void ExplicitDispatcher_MarshalsAndCoalescesPendingCallbacks()
	{
		Action? queuedCallback = null;
		var dispatchCount = 0;
		var callbackCount = 0;
		using var dispatched = new ManualResetEventSlim();
		var clock = new SystemGestureClock(callback =>
		{
			Interlocked.Increment(ref dispatchCount);
			queuedCallback = callback;
			dispatched.Set();
		});

		using var registration = clock.Schedule(
			TimeSpan.Zero,
			TimeSpan.FromMilliseconds(1),
			() => callbackCount++);

		Assert.True(dispatched.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
		Thread.Sleep(20);
		Assert.Equal(1, Volatile.Read(ref dispatchCount));
		Assert.Equal(0, callbackCount);

		queuedCallback!();

		Assert.Equal(1, callbackCount);
	}

	private static void UpdateMaximum(ref int target, int candidate)
	{
		var current = Volatile.Read(ref target);
		while (candidate > current)
		{
			var observed = Interlocked.CompareExchange(ref target, candidate, current);
			if (observed == current)
				return;
			current = observed;
		}
	}
}
