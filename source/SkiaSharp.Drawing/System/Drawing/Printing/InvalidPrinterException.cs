namespace System.Drawing.Printing
{
	/// <summary>
	///  Represents the exception that is thrown when you try to access a printer using printer settings that are not valid.
	/// </summary>
	public partial class InvalidPrinterException : System.SystemException
	{
		/// <summary>Initializes a new instance of the <see cref="InvalidPrinterException"/> class.</summary>
		/// <param name="settings">A <see cref="PrinterSettings"/> that specifies the settings for a printer.</param>
		public InvalidPrinterException(System.Drawing.Printing.PrinterSettings settings)
			: base(settings != null ? $"Settings to access printer '{settings.PrinterName}' are not valid." : "Printer settings are not valid.")
		{
		}

		/// <summary>Initializes a new instance of the <see cref="InvalidPrinterException"/> class with serialized data.</summary>
		protected InvalidPrinterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>Sets the <see cref="System.Runtime.Serialization.SerializationInfo"/> with information about the exception.</summary>
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}
