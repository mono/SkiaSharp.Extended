namespace System.Drawing.Imaging;

/// <summary>
///  Defines a 5×5 matrix that contains the coordinates for the RGBAW color space.
/// </summary>
public sealed partial class ColorMatrix
{
	private readonly float[,] _matrix = new float[5, 5];

	/// <summary>
	///  Initializes a new instance of the <see cref="ColorMatrix"/> class with an identity matrix.
	/// </summary>
	public ColorMatrix()
	{
		_matrix[0, 0] = 1f;
		_matrix[1, 1] = 1f;
		_matrix[2, 2] = 1f;
		_matrix[3, 3] = 1f;
		_matrix[4, 4] = 1f;
	}

	/// <summary>
	///  Initializes a new instance of the <see cref="ColorMatrix"/> class using the elements in the specified matrix.
	/// </summary>
	/// <param name="newColorMatrix">The values of the elements for the new <see cref="ColorMatrix"/>.</param>
	public ColorMatrix(float[][] newColorMatrix)
	{
		if (newColorMatrix is null) throw new ArgumentNullException(nameof(newColorMatrix));
		if (newColorMatrix.Length != 5) throw new ArgumentException("Matrix must have 5 rows.", nameof(newColorMatrix));
		for (int i = 0; i < 5; i++)
		{
			if (newColorMatrix[i] is null || newColorMatrix[i].Length != 5)
				throw new ArgumentException("Each row must have 5 columns.", nameof(newColorMatrix));
			for (int j = 0; j < 5; j++)
				_matrix[i, j] = newColorMatrix[i][j];
		}
	}

	/// <summary>Gets or sets the element at the 0,0 position.</summary>
	public float Matrix00 { get => _matrix[0, 0]; set => _matrix[0, 0] = value; }
	/// <summary>Gets or sets the element at the 0,1 position.</summary>
	public float Matrix01 { get => _matrix[0, 1]; set => _matrix[0, 1] = value; }
	/// <summary>Gets or sets the element at the 0,2 position.</summary>
	public float Matrix02 { get => _matrix[0, 2]; set => _matrix[0, 2] = value; }
	/// <summary>Gets or sets the element at the 0,3 position.</summary>
	public float Matrix03 { get => _matrix[0, 3]; set => _matrix[0, 3] = value; }
	/// <summary>Gets or sets the element at the 0,4 position.</summary>
	public float Matrix04 { get => _matrix[0, 4]; set => _matrix[0, 4] = value; }
	/// <summary>Gets or sets the element at the 1,0 position.</summary>
	public float Matrix10 { get => _matrix[1, 0]; set => _matrix[1, 0] = value; }
	/// <summary>Gets or sets the element at the 1,1 position.</summary>
	public float Matrix11 { get => _matrix[1, 1]; set => _matrix[1, 1] = value; }
	/// <summary>Gets or sets the element at the 1,2 position.</summary>
	public float Matrix12 { get => _matrix[1, 2]; set => _matrix[1, 2] = value; }
	/// <summary>Gets or sets the element at the 1,3 position.</summary>
	public float Matrix13 { get => _matrix[1, 3]; set => _matrix[1, 3] = value; }
	/// <summary>Gets or sets the element at the 1,4 position.</summary>
	public float Matrix14 { get => _matrix[1, 4]; set => _matrix[1, 4] = value; }
	/// <summary>Gets or sets the element at the 2,0 position.</summary>
	public float Matrix20 { get => _matrix[2, 0]; set => _matrix[2, 0] = value; }
	/// <summary>Gets or sets the element at the 2,1 position.</summary>
	public float Matrix21 { get => _matrix[2, 1]; set => _matrix[2, 1] = value; }
	/// <summary>Gets or sets the element at the 2,2 position.</summary>
	public float Matrix22 { get => _matrix[2, 2]; set => _matrix[2, 2] = value; }
	/// <summary>Gets or sets the element at the 2,3 position.</summary>
	public float Matrix23 { get => _matrix[2, 3]; set => _matrix[2, 3] = value; }
	/// <summary>Gets or sets the element at the 2,4 position.</summary>
	public float Matrix24 { get => _matrix[2, 4]; set => _matrix[2, 4] = value; }
	/// <summary>Gets or sets the element at the 3,0 position.</summary>
	public float Matrix30 { get => _matrix[3, 0]; set => _matrix[3, 0] = value; }
	/// <summary>Gets or sets the element at the 3,1 position.</summary>
	public float Matrix31 { get => _matrix[3, 1]; set => _matrix[3, 1] = value; }
	/// <summary>Gets or sets the element at the 3,2 position.</summary>
	public float Matrix32 { get => _matrix[3, 2]; set => _matrix[3, 2] = value; }
	/// <summary>Gets or sets the element at the 3,3 position.</summary>
	public float Matrix33 { get => _matrix[3, 3]; set => _matrix[3, 3] = value; }
	/// <summary>Gets or sets the element at the 3,4 position.</summary>
	public float Matrix34 { get => _matrix[3, 4]; set => _matrix[3, 4] = value; }
	/// <summary>Gets or sets the element at the 4,0 position.</summary>
	public float Matrix40 { get => _matrix[4, 0]; set => _matrix[4, 0] = value; }
	/// <summary>Gets or sets the element at the 4,1 position.</summary>
	public float Matrix41 { get => _matrix[4, 1]; set => _matrix[4, 1] = value; }
	/// <summary>Gets or sets the element at the 4,2 position.</summary>
	public float Matrix42 { get => _matrix[4, 2]; set => _matrix[4, 2] = value; }
	/// <summary>Gets or sets the element at the 4,3 position.</summary>
	public float Matrix43 { get => _matrix[4, 3]; set => _matrix[4, 3] = value; }
	/// <summary>Gets or sets the element at the 4,4 position.</summary>
	public float Matrix44 { get => _matrix[4, 4]; set => _matrix[4, 4] = value; }

	/// <summary>
	///  Gets or sets the element at the specified row and column in the <see cref="ColorMatrix"/>.
	/// </summary>
	/// <param name="row">The row of the element.</param>
	/// <param name="column">The column of the element.</param>
	/// <returns>The element at the specified row and column.</returns>
	public float this[int row, int column]
	{
		get => _matrix[row, column];
		set => _matrix[row, column] = value;
	}
}
