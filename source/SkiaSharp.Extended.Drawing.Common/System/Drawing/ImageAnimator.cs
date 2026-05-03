using System.Collections.Generic;
using System.Threading;

namespace System.Drawing
{
	/// <summary>Animates an image that has time-based frames.</summary>
	public sealed partial class ImageAnimator
	{
		private static readonly Dictionary<Image, AnimationState> _animatedImages = new();
		private static readonly object _lock = new();

		private class AnimationState
		{
			public EventHandler? Handler;
			public Timer? Timer;
			public int FrameCount;
			public int CurrentFrame;
		}

		private ImageAnimator() {}

		/// <summary>Displays a multi-frame image as an animation.</summary>
		public static void Animate(Image image, EventHandler onFrameChangedHandler)
		{
			if (image == null) throw new ArgumentNullException(nameof(image));
			if (onFrameChangedHandler == null) throw new ArgumentNullException(nameof(onFrameChangedHandler));

			if (!CanAnimate(image)) return;

			lock (_lock)
			{
				if (_animatedImages.ContainsKey(image)) return;

				var state = new AnimationState
				{
					Handler = onFrameChangedHandler,
					FrameCount = image.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time),
					CurrentFrame = 0
				};
				state.Timer = new Timer(_ =>
				{
					lock (_lock)
					{
						if (_animatedImages.ContainsKey(image))
						{
							state.CurrentFrame = (state.CurrentFrame + 1) % Math.Max(1, state.FrameCount);
							state.Handler?.Invoke(image, EventArgs.Empty);
						}
					}
				}, null, 100, 100);
				_animatedImages[image] = state;
			}
		}

		/// <summary>Returns a Boolean value indicating whether the specified image contains time-based frames.</summary>
		public static bool CanAnimate(Image? image)
		{
			if (image == null) return false;
			try
			{
				return image.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time) > 1;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>Terminates a running animation.</summary>
		public static void StopAnimate(Image image, EventHandler onFrameChangedHandler)
		{
			if (image == null) throw new ArgumentNullException(nameof(image));

			lock (_lock)
			{
				if (_animatedImages.TryGetValue(image, out var state))
				{
					state.Timer?.Dispose();
					state.Timer = null;
					_animatedImages.Remove(image);
				}
			}
		}

		/// <summary>Advances the frame in all images currently being animated.</summary>
		public static void UpdateFrames()
		{
			lock (_lock)
			{
				foreach (var kvp in _animatedImages)
				{
					var state = kvp.Value;
					state.CurrentFrame = (state.CurrentFrame + 1) % Math.Max(1, state.FrameCount);
				}
			}
		}

		/// <summary>Advances the frame in the specified image.</summary>
		public static void UpdateFrames(Image? image)
		{
			if (image == null) return;

			lock (_lock)
			{
				if (_animatedImages.TryGetValue(image, out var state))
				{
					state.CurrentFrame = (state.CurrentFrame + 1) % Math.Max(1, state.FrameCount);
				}
			}
		}
	}
}
