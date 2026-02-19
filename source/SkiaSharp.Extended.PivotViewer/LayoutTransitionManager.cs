using System;
using System.Collections.Generic;

namespace SkiaSharp.Extended.PivotViewer
{
    /// <summary>
    /// Manages animated transitions between layout states (grid ↔ graph, filter changes).
    /// Each item smoothly animates from its old position to its new position.
    /// </summary>
    public class LayoutTransitionManager
    {
        private Dictionary<string, ItemTransition> _transitions = new Dictionary<string, ItemTransition>();
        private double _progress;
        private double _duration;
        private bool _isAnimating;

        /// <summary>Default transition duration in seconds.</summary>
        public double Duration { get; set; } = 0.5;

        /// <summary>Whether a transition is currently in progress.</summary>
        public bool IsAnimating => _isAnimating;

        /// <summary>Current transition progress (0.0 to 1.0).</summary>
        public double Progress => _progress;

        /// <summary>
        /// Start a transition from the old positions to the new positions.
        /// Items are matched by their PivotViewerItem.Id.
        /// </summary>
        public void BeginTransition(ItemPosition[] oldPositions, ItemPosition[] newPositions)
        {
            _transitions.Clear();
            _progress = 0;
            _duration = Math.Max(0.001, Duration);
            _isAnimating = true;

            // Build lookup for old positions
            var oldMap = new Dictionary<string, ItemPosition>();
            foreach (var pos in oldPositions)
                oldMap[pos.Item.Id] = pos;

            // Build set of items in new layout
            var newIds = new HashSet<string>();

            foreach (var newPos in newPositions)
            {
                newIds.Add(newPos.Item.Id);
                ItemPosition oldPos;
                if (oldMap.TryGetValue(newPos.Item.Id, out var existing))
                {
                    oldPos = existing;
                }
                else
                {
                    // New item: fade in from center of new position
                    oldPos = new ItemPosition(newPos.Item, newPos.X + newPos.Width / 2,
                        newPos.Y + newPos.Height / 2, 0, 0);
                }

                _transitions[newPos.Item.Id] = new ItemTransition(oldPos, newPos);
            }

            // Items leaving scope: shrink to center of old position
            foreach (var oldPos in oldPositions)
            {
                if (!newIds.Contains(oldPos.Item.Id))
                {
                    var shrinkTarget = new ItemPosition(oldPos.Item,
                        oldPos.X + oldPos.Width / 2,
                        oldPos.Y + oldPos.Height / 2, 0, 0);
                    _transitions[oldPos.Item.Id] = new ItemTransition(oldPos, shrinkTarget);
                }
            }
        }

        /// <summary>
        /// Update the transition. Returns true if the transition is still in progress.
        /// </summary>
        public bool Update(double deltaTimeSeconds)
        {
            if (!_isAnimating) return false;

            _progress += deltaTimeSeconds / _duration;
            if (_progress >= 1.0)
            {
                _progress = 1.0;
                _isAnimating = false;
            }

            return _isAnimating;
        }

        /// <summary>
        /// Get the current interpolated positions.
        /// If no transition is active, returns the target positions directly.
        /// </summary>
        public ItemPosition[] GetCurrentPositions()
        {
            var result = new ItemPosition[_transitions.Count];
            int i = 0;

            double t = EaseOutCubic(_progress);

            foreach (var pair in _transitions)
            {
                var trans = pair.Value;
                if (!_isAnimating)
                {
                    result[i] = trans.Target;
                }
                else
                {
                    double x = Lerp(trans.Source.X, trans.Target.X, t);
                    double y = Lerp(trans.Source.Y, trans.Target.Y, t);
                    double w = Lerp(trans.Source.Width, trans.Target.Width, t);
                    double h = Lerp(trans.Source.Height, trans.Target.Height, t);
                    result[i] = new ItemPosition(trans.Target.Item, x, y, w, h);
                }
                i++;
            }

            return result;
        }

        /// <summary>Cancel the current transition and snap to final positions.</summary>
        public void CancelTransition()
        {
            _progress = 1.0;
            _isAnimating = false;
        }

        private static double Lerp(double a, double b, double t) => a + (b - a) * t;

        private static double EaseOutCubic(double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return 1.0 - Math.Pow(1.0 - t, 3);
        }

        private struct ItemTransition
        {
            public ItemPosition Source;
            public ItemPosition Target;

            public ItemTransition(ItemPosition source, ItemPosition target)
            {
                Source = source;
                Target = target;
            }
        }
    }
}
