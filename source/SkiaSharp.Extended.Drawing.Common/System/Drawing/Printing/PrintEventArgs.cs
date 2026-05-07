namespace System.Drawing.Printing;

/// <summary>
///  Provides data for the <see cref="PrintDocument.BeginPrint"/> and <see cref="PrintDocument.EndPrint"/> events.
/// </summary>
public partial class PrintEventArgs : ComponentModel.CancelEventArgs
{
	private PrintAction _printAction;

	/// <summary>Initializes a new instance of the <see cref="PrintEventArgs"/> class.</summary>
	public PrintEventArgs() { _printAction = PrintAction.PrintToPrinter; }

	internal PrintEventArgs(PrintAction action) { _printAction = action; }

	/// <summary>Gets the <see cref="PrintAction"/> for the print job.</summary>
	public PrintAction PrintAction => _printAction;
}
