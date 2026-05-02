namespace System.Drawing
{
	/// <summary>
	///  Provides access to the main buffered graphics context object for the application domain.
	/// </summary>
	public static partial class BufferedGraphicsManager
	{
		private static readonly BufferedGraphicsContext _current = new BufferedGraphicsContext();

		/// <summary>Gets the <see cref="BufferedGraphicsContext"/> for the current application domain.</summary>
		public static System.Drawing.BufferedGraphicsContext Current => _current;
	}
}
