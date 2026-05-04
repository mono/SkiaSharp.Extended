namespace System.Drawing.Printing;

/// <summary>
///  Controls how a document is printed.
/// </summary>
public abstract partial class PrintController
{
	/// <summary>Initializes a new instance of the <see cref="PrintController"/> class.</summary>
	protected PrintController() { }

	/// <summary>Gets a value indicating whether this <see cref="PrintController"/> is used for print preview.</summary>
	public virtual bool IsPreview => false;

	/// <summary>When overridden in a derived class, completes the control sequence that determines when and how to print a page of a document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintPageEventArgs"/> that contains the event data.</param>
	public virtual void OnEndPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) { }

	/// <summary>When overridden in a derived class, completes the control sequence that determines when and how to print a document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintEventArgs"/> that contains the event data.</param>
	public virtual void OnEndPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) { }

	/// <summary>When overridden in a derived class, begins the control sequence that determines when and how to print a page of a document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintPageEventArgs"/> that contains the event data.</param>
	/// <returns>A <see cref="Graphics"/> that represents a page from a <see cref="PrintDocument"/>.</returns>
	public virtual System.Drawing.Graphics? OnStartPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) { return null; }

	/// <summary>When overridden in a derived class, begins the control sequence that determines when and how to print a document.</summary>
	/// <param name="document">The <see cref="PrintDocument"/> currently being printed.</param>
	/// <param name="e">A <see cref="PrintEventArgs"/> that contains the event data.</param>
	public virtual void OnStartPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) { }
}
