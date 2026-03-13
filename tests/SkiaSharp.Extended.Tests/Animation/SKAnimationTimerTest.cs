using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Extended.Tests.Animation;

public class SKAnimationTimerTest : IDisposable
{
	private readonly SKAnimationTimer _anim = new();

	public void Dispose() => _anim.Dispose();

	// ── IsRunning ─────────────────────────────────────────────────────────────

	[Fact]
	public void IsRunning_FalseInitially()
	{
		Assert.False(_anim.IsRunning);
	}

	[Fact]
	public void IsRunning_TrueAfterStart()
	{
		_anim.Start(TimeSpan.FromSeconds(60), () => { });
		Assert.True(_anim.IsRunning);
	}

	[Fact]
	public void IsRunning_FalseAfterStop()
	{
		_anim.Start(TimeSpan.FromSeconds(60), () => { });
		_anim.Stop();
		Assert.False(_anim.IsRunning);
	}

	[Fact]
	public void IsRunning_FalseAfterDispose()
	{
		_anim.Start(TimeSpan.FromSeconds(60), () => { });
		_anim.Dispose();
		Assert.False(_anim.IsRunning);
	}

	// ── Tick fires ────────────────────────────────────────────────────────────

	[Fact]
	public async Task Start_TickCallbackFires()
	{
		var tcs = new TaskCompletionSource<bool>();

		_anim.Start(TimeSpan.FromMilliseconds(20), () => tcs.TrySetResult(true));

		var result = await Task.WhenAny(tcs.Task, Task.Delay(2000));
		Assert.Same(tcs.Task, result);
		Assert.True(tcs.Task.Result);
	}

	[Fact]
	public async Task Start_TickCallbackFiresMultipleTimes()
	{
		var count = 0;
		var tcs = new TaskCompletionSource<bool>();

		_anim.Start(TimeSpan.FromMilliseconds(20), () =>
		{
			if (Interlocked.Increment(ref count) >= 3)
				tcs.TrySetResult(true);
		});

		var result = await Task.WhenAny(tcs.Task, Task.Delay(2000));
		Assert.Same(tcs.Task, result);
		Assert.True(count >= 3);
	}

	// ── Stop cancels correctly ────────────────────────────────────────────────

	[Fact]
	public async Task Stop_PreventsCallbackFromFiringAfterStop()
	{
		// Track how many callbacks fire after we signal "stop was requested"
		var stopRequested = false;
		var firedAfterStopSignal = 0;
		var firstTickSeen = new TaskCompletionSource<bool>();

		_anim.Start(TimeSpan.FromMilliseconds(20), () =>
		{
			firstTickSeen.TrySetResult(true);
			// Count callbacks that arrive after the stop signal was set
			if (Volatile.Read(ref stopRequested))
				Interlocked.Increment(ref firedAfterStopSignal);
		});

		// Wait for at least one tick to confirm the animation is running
		await Task.WhenAny(firstTickSeen.Task, Task.Delay(1000));

		// Signal stop and stop the animation
		Volatile.Write(ref stopRequested, true);
		_anim.Stop();

		// Wait enough time for any in-flight tick to complete and new ones to not fire
		await Task.Delay(150);

		// At most 1 in-flight tick may have already passed the token check before Stop completed.
		// Zero or one is acceptable; two or more means the timer kept running.
		Assert.True(Volatile.Read(ref firedAfterStopSignal) <= 1,
			$"At most 1 in-flight callback is acceptable; got {firedAfterStopSignal}");
	}

	[Fact]
	public async Task ImmediateStop_NoCallbackFires()
	{
		var count = 0;

		// Start with a long interval and stop immediately — callback should never fire
		_anim.Start(TimeSpan.FromMilliseconds(500), () => Interlocked.Increment(ref count));
		_anim.Stop();

		await Task.Delay(200);

		Assert.Equal(0, count);
	}

	// ── Double-stop is safe ───────────────────────────────────────────────────

	[Fact]
	public void Stop_SafeWhenNotRunning()
	{
		var ex = Record.Exception(() => _anim.Stop());
		Assert.Null(ex);
	}

	[Fact]
	public void Stop_SafeToCallTwice()
	{
		_anim.Start(TimeSpan.FromSeconds(60), () => { });
		_anim.Stop();
		var ex = Record.Exception(() => _anim.Stop());
		Assert.Null(ex);
	}

	// ── Restart behavior ──────────────────────────────────────────────────────

	[Fact]
	public async Task Restart_OldCallbackStops_NewCallbackFires()
	{
		var oldCount = 0;
		var newCount = 0;

		// Start first animation
		_anim.Start(TimeSpan.FromMilliseconds(20), () => Interlocked.Increment(ref oldCount));

		// Let it tick once or twice
		await Task.Delay(80);

		var countAfterFirstPhase = Volatile.Read(ref oldCount);
		Assert.True(countAfterFirstPhase > 0, "First animation should have fired at least once");

		// Restart with a new callback
		_anim.Start(TimeSpan.FromMilliseconds(20), () => Interlocked.Increment(ref newCount));

		// Wait for new callback to fire
		await Task.Delay(100);

		var finalOldCount = Volatile.Read(ref oldCount);
		var finalNewCount = Volatile.Read(ref newCount);

		// New callback should have fired
		Assert.True(finalNewCount > 0, "New callback should have fired after restart");

		// Old count should not have grown significantly after restart
		// (allow a small window for in-flight ticks right at the transition)
		Assert.True(finalOldCount - countAfterFirstPhase <= 2,
			$"Old callback should stop after restart: grew by {finalOldCount - countAfterFirstPhase}");
	}

	[Fact]
	public void Restart_IsRunningRemainsTrue()
	{
		_anim.Start(TimeSpan.FromSeconds(60), () => { });
		_anim.Start(TimeSpan.FromSeconds(60), () => { }); // restart without stopping
		Assert.True(_anim.IsRunning);
	}

	// ── Rapid start/stop ──────────────────────────────────────────────────────

	[Fact]
	public async Task RapidStartStop_NoCrash()
	{
		var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

		for (int i = 0; i < 50; i++)
		{
			try
			{
				_anim.Start(TimeSpan.FromMilliseconds(5), () => { });
				_anim.Stop();
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}
		}

		await Task.Delay(50); // let any pending timer ticks drain

		Assert.Empty(exceptions);
	}

	[Fact]
	public async Task RapidRestart_CallbackDoesNotFireAfterStop()
	{
		var firedWhenStopped = false;

		for (int i = 0; i < 20; i++)
		{
			_anim.Start(TimeSpan.FromMilliseconds(5), () =>
			{
				if (!_anim.IsRunning)
					firedWhenStopped = true;
			});
			_anim.Stop();
		}

		await Task.Delay(100);
		Assert.False(firedWhenStopped);
	}

	// ── Dispose ───────────────────────────────────────────────────────────────

	[Fact]
	public void Dispose_SafeToCallMultipleTimes()
	{
		var anim = new SKAnimationTimer();
		anim.Dispose();
		var ex = Record.Exception(() => anim.Dispose());
		Assert.Null(ex);
	}

	[Fact]
	public void Dispose_StopsRunningAnimation()
	{
		var anim = new SKAnimationTimer();
		anim.Start(TimeSpan.FromSeconds(60), () => { });
		Assert.True(anim.IsRunning);
		anim.Dispose();
		Assert.False(anim.IsRunning);
	}

	[Fact]
	public async Task Dispose_NoCallbacksFireAfterDispose()
	{
		var count = 0;
		var anim = new SKAnimationTimer();
		anim.Start(TimeSpan.FromMilliseconds(20), () => Interlocked.Increment(ref count));
		await Task.Delay(60); // let at least one tick fire

		anim.Dispose();
		var countAtDispose = Volatile.Read(ref count);

		await Task.Delay(100);

		Assert.Equal(countAtDispose, Volatile.Read(ref count));
	}

	[Fact]
	public void Start_AfterDispose_ThrowsObjectDisposedException()
	{
		var anim = new SKAnimationTimer();
		anim.Dispose();
		Assert.Throws<ObjectDisposedException>(() =>
			anim.Start(TimeSpan.FromSeconds(1), () => { }));
	}

	// ── SynchronizationContext dispatch ───────────────────────────────────────

	[Fact]
	public async Task Start_WithNullSyncContext_FiresOnThreadPool()
	{
		var tcs = new TaskCompletionSource<bool>();

		_anim.Start(TimeSpan.FromMilliseconds(20), () => tcs.TrySetResult(true), syncContext: null);

		var result = await Task.WhenAny(tcs.Task, Task.Delay(2000));
		Assert.Same(tcs.Task, result);
	}

	[Fact]
	public async Task Start_WithSyncContext_DispatchesThroughIt()
	{
		var dispatchCount = 0;
		var tcs = new TaskCompletionSource<bool>();

		var ctx = new CountingSynchronizationContext(() => Interlocked.Increment(ref dispatchCount));

		_anim.Start(TimeSpan.FromMilliseconds(20), () => tcs.TrySetResult(true), syncContext: ctx);

		var result = await Task.WhenAny(tcs.Task, Task.Delay(2000));
		_anim.Stop();

		Assert.Same(tcs.Task, result);
		Assert.True(Volatile.Read(ref dispatchCount) >= 1,
			"Callback should have been dispatched through SynchronizationContext");
	}

	/// <summary>A SynchronizationContext that counts Post calls and invokes them inline.</summary>
	private sealed class CountingSynchronizationContext : SynchronizationContext
	{
		private readonly Action _onPost;

		public CountingSynchronizationContext(Action onPost) => _onPost = onPost;

		public override void Post(SendOrPostCallback d, object? state)
		{
			_onPost();
			ThreadPool.QueueUserWorkItem(_ => d(state));
		}

		public override void Send(SendOrPostCallback d, object? state) => d(state);
	}
}
