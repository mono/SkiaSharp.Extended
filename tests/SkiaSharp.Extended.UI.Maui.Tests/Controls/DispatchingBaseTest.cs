namespace SkiaSharp.Extended.UI.Controls.Tests;

public class DispatchingBaseTest : IDisposable
{
	private readonly MockDispatcherProvider dipatcherProvider;

	public DispatchingBaseTest()
	{
		DispatcherProvider.SetCurrent(dipatcherProvider = new MockDispatcherProvider());
	}

	public void Dispose() =>
		dipatcherProvider.Dispose();

	private class MockDispatcherProvider : IDispatcherProvider, IDisposable
	{
		private readonly ThreadLocal<IDispatcher?> dispatcherInstance = new(() => new MockDispatcher());

		public void Dispose() =>
			dispatcherInstance.Dispose();

		public IDispatcher? GetForCurrentThread() =>
			dispatcherInstance.Value;
	}

	private class MockDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;

		public bool Dispatch(Action action)
		{
			action();
			return true;
		}

		public bool DispatchDelayed(TimeSpan delay, Action action)
		{
			this.StartTimer(delay, () =>
			{
				action();
				return false;
			});
			return true;
		}

		public IDispatcherTimer CreateTimer() =>
			new MockDispatcherTimer(this);
	}

	private class MockDispatcherTimer : IDispatcherTimer
	{
		private readonly MockDispatcher dispatcher;
		private Timer? timer;

		public MockDispatcherTimer(MockDispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
		}

		public TimeSpan Interval { get; set; }

		public bool IsRepeating { get; set; }

		public bool IsRunning => timer != null;

		public event EventHandler? Tick;

		public void Start()
		{
			timer = new Timer(OnTimeout, null, Interval, IsRepeating ? Interval : Timeout.InfiniteTimeSpan);

			void OnTimeout(object? state)
			{
				dispatcher.Dispatch(() => Tick?.Invoke(this, EventArgs.Empty));
			}
		}

		public void Stop()
		{
			timer?.Dispose();
			timer = null;
		}
	}
}
