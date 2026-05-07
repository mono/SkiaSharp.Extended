namespace System.Drawing.Printing;

/// <summary>
///  Represents the exception that is thrown when you try to access a printer using printer settings that are not valid.
/// </summary>
public partial class InvalidPrinterException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="InvalidPrinterException"/> class.</summary>
	/// <param name="settings">A <see cref="PrinterSettings"/> that specifies the settings for a printer.</param>
	public InvalidPrinterException(PrinterSettings settings)
		: base(settings != null ? $"Settings to access printer '{settings.PrinterName}' are not valid." : "Printer settings are not valid.")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="InvalidPrinterException"/> class with serialized data.</summary>
	protected InvalidPrinterException(Runtime.Serialization.SerializationInfo info, Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Sets the <see cref="Runtime.Serialization.SerializationInfo"/> with information about the exception.</summary>
	public override void GetObjectData(Runtime.Serialization.SerializationInfo info, Runtime.Serialization.StreamingContext context)
	{
		base.GetObjectData(info, context);
	}
}
