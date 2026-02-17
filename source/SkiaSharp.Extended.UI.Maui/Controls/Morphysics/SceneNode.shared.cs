namespace SkiaSharp.Extended.UI.Controls;

/// <summary>
/// Base class for all nodes in the scene graph.
/// </summary>
public abstract class SceneNode : BindableObject
{
	public static readonly BindableProperty IdProperty = BindableProperty.Create(
		nameof(Id),
		typeof(string),
		typeof(SceneNode),
		string.Empty);

	public static readonly BindableProperty XProperty = BindableProperty.Create(
		nameof(X),
		typeof(float),
		typeof(SceneNode),
		0f);

	public static readonly BindableProperty YProperty = BindableProperty.Create(
		nameof(Y),
		typeof(float),
		typeof(SceneNode),
		0f);

	public static readonly BindableProperty RotationProperty = BindableProperty.Create(
		nameof(Rotation),
		typeof(float),
		typeof(SceneNode),
		0f);

	public static readonly BindableProperty ScaleXProperty = BindableProperty.Create(
		nameof(ScaleX),
		typeof(float),
		typeof(SceneNode),
		1f);

	public static readonly BindableProperty ScaleYProperty = BindableProperty.Create(
		nameof(ScaleY),
		typeof(float),
		typeof(SceneNode),
		1f);

	public static readonly BindableProperty OpacityProperty = BindableProperty.Create(
		nameof(Opacity),
		typeof(float),
		typeof(SceneNode),
		1f);

	public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(
		nameof(IsVisible),
		typeof(bool),
		typeof(SceneNode),
		true);

	private readonly List<SceneNode> children = new List<SceneNode>();

	/// <summary>
	/// Gets or sets the unique identifier for this node.
	/// </summary>
	public string Id
	{
		get => (string)GetValue(IdProperty);
		set => SetValue(IdProperty, value);
	}

	/// <summary>
	/// Gets or sets the X position of this node.
	/// </summary>
	public float X
	{
		get => (float)GetValue(XProperty);
		set => SetValue(XProperty, value);
	}

	/// <summary>
	/// Gets or sets the Y position of this node.
	/// </summary>
	public float Y
	{
		get => (float)GetValue(YProperty);
		set => SetValue(YProperty, value);
	}

	/// <summary>
	/// Gets or sets the rotation of this node in degrees.
	/// </summary>
	public float Rotation
	{
		get => (float)GetValue(RotationProperty);
		set => SetValue(RotationProperty, value);
	}

	/// <summary>
	/// Gets or sets the horizontal scale of this node.
	/// </summary>
	public float ScaleX
	{
		get => (float)GetValue(ScaleXProperty);
		set => SetValue(ScaleXProperty, value);
	}

	/// <summary>
	/// Gets or sets the vertical scale of this node.
	/// </summary>
	public float ScaleY
	{
		get => (float)GetValue(ScaleYProperty);
		set => SetValue(ScaleYProperty, value);
	}

	/// <summary>
	/// Gets or sets the opacity of this node (0.0 to 1.0).
	/// </summary>
	public float Opacity
	{
		get => (float)GetValue(OpacityProperty);
		set => SetValue(OpacityProperty, value);
	}

	/// <summary>
	/// Gets or sets whether this node is visible.
	/// </summary>
	public bool IsVisible
	{
		get => (bool)GetValue(IsVisibleProperty);
		set => SetValue(IsVisibleProperty, value);
	}

	/// <summary>
	/// Gets the parent node, if any.
	/// </summary>
	public SceneNode? Parent { get; internal set; }

	/// <summary>
	/// Gets the list of child nodes.
	/// </summary>
	public IReadOnlyList<SceneNode> Children => children;

	/// <summary>
	/// Adds a child node to this node.
	/// </summary>
	public void AddChild(SceneNode child)
	{
		if (child == null)
			throw new ArgumentNullException(nameof(child));

		if (child.Parent != null)
			throw new InvalidOperationException("Node already has a parent");

		children.Add(child);
		child.Parent = this;
	}

	/// <summary>
	/// Removes a child node from this node.
	/// </summary>
	public bool RemoveChild(SceneNode child)
	{
		if (child == null)
			throw new ArgumentNullException(nameof(child));

		if (children.Remove(child))
		{
			child.Parent = null;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Renders this node and its children.
	/// </summary>
	internal void Render(SKCanvas canvas, SKSize size)
	{
		if (!IsVisible)
			return;

		canvas.Save();

		// Apply transformations
		canvas.Translate(X, Y);
		canvas.RotateDegrees(Rotation);
		canvas.Scale(ScaleX, ScaleY);

		// Apply opacity
		if (Opacity < 1f)
		{
			using var paint = new SKPaint
			{
				Color = new SKColor(255, 255, 255, (byte)(Opacity * 255))
			};
			canvas.SaveLayer(paint);
		}

		// Render this node
		OnRender(canvas, size);

		// Render children
		foreach (var child in children)
		{
			child.Render(canvas, size);
		}

		if (Opacity < 1f)
			canvas.Restore();

		canvas.Restore();
	}

	/// <summary>
	/// Override this method to implement custom rendering for this node.
	/// </summary>
	protected abstract void OnRender(SKCanvas canvas, SKSize size);
}
