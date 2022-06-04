using System.Collections.Concurrent;

namespace SkiaSharp.Extended.UI;

internal class SKObjectPool<T>
{
	private readonly ConcurrentBag<T> objects;
	private readonly Func<T> generator;

	public SKObjectPool(Func<T> objectGenerator)
	{
		generator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
		objects = new ConcurrentBag<T>();
	}

	public T Get() =>
		objects.TryTake(out var item) ? item : generator();

	public void Return(T item) =>
		objects.Add(item);
}
