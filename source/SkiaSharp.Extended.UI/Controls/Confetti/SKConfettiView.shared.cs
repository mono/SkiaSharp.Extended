using System.Collections.Specialized;

namespace SkiaSharp.Extended.UI.Controls;

public class SKConfettiView : SKAnimatedSurfaceView
{
	private static readonly BindablePropertyKey IsCompletePropertyKey = BindableProperty.CreateReadOnly(
		nameof(IsComplete),
		typeof(bool),
		typeof(SKConfettiView),
		false);

	public static readonly BindableProperty IsCompleteProperty = IsCompletePropertyKey.BindableProperty;

	public static readonly BindableProperty SystemsProperty = BindableProperty.Create(
		nameof(Systems),
		typeof(SKConfettiSystemCollection),
		typeof(SKConfettiView),
		null,
		propertyChanged: OnSystemsPropertyChanged,
		defaultValueCreator: _ => CreateDefaultSystems());

	public SKConfettiView()
	{
		Themes.SKConfettiViewResources.EnsureRegistered();

		SizeChanged += OnSizeChanged;
		PropertyChanged += (_, e) =>
		{
			if (nameof(IsAnimationEnabled).Equals(e.PropertyName, StringComparison.OrdinalIgnoreCase))
				OnIsAnimationEnabledPropertyChanged();
		};

		IsAnimationEnabled = true;

		OnSystemsPropertyChanged(this, null, Systems);
	}

	public bool IsComplete
	{
		get => (bool)GetValue(IsCompleteProperty);
		private set => SetValue(IsCompletePropertyKey, value);
	}

	public SKConfettiSystemCollection? Systems
	{
		get => (SKConfettiSystemCollection?)GetValue(SystemsProperty);
		set => SetValue(SystemsProperty, value);
	}

	protected override void Update(TimeSpan deltaTime)
	{
		if (Systems is null)
			return;

		for (var i = Systems.Count - 1; i >= 0; i--)
		{
			var system = Systems[i];
			system.Update(deltaTime);

			if (system.IsComplete)
				Systems.RemoveAt(i);
		}
	}

	protected override void OnPaintSurface(SKCanvas canvas, SKSize size)
	{
		var particles = 0;

		if (Systems?.Count > 0)
		{
			foreach (var system in Systems)
			{
				system.Draw(canvas);

				particles += system.ParticleCount;
			}
		}

#if DEBUG
		WriteDebugStatus($"Particles: {particles}");
#endif
	}

	private void OnSizeChanged(object? sender, EventArgs e)
	{
		if (Systems is null)
			return;

		foreach (var system in Systems)
		{
			system.UpdateEmitterBounds(Width, Height);
		}
	}

	private void OnSystemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems is not null)
		{
			foreach (SKConfettiSystem system in e.NewItems)
			{
				system.UpdateEmitterBounds(Width, Height);
				system.IsAnimationEnabled = IsAnimationEnabled;
			}

			Invalidate();
		}

		UpdateIsComplete();
	}

	private void OnIsAnimationEnabledPropertyChanged()
	{
		if (Systems is null)
			return;

		foreach (var system in Systems)
		{
			system.IsAnimationEnabled = IsAnimationEnabled;
		}
	}

	private static void OnSystemsPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
		if (bindable is not SKConfettiView cv)
			return;

		if (oldValue is SKConfettiSystemCollection oldCollection)
			oldCollection.CollectionChanged -= cv.OnSystemsCollectionChanged;

		if (newValue is SKConfettiSystemCollection newCollection)
			newCollection.CollectionChanged += cv.OnSystemsCollectionChanged;

		cv.UpdateIsComplete();
	}

	private void UpdateIsComplete()
	{
		if (Systems is null || Systems.Count == 0)
		{
			IsComplete = true;
			return;
		}

		var isComplete = false;
		foreach (var system in Systems)
		{
			if (system.IsComplete)
			{
				isComplete = true;
				break;
			}
		}

		IsComplete = isComplete;
	}

	private static SKConfettiSystemCollection CreateDefaultSystems() =>
		new SKConfettiSystemCollection
		{
			new SKConfettiSystem()
		};
}
