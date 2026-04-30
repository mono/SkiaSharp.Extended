namespace System.Drawing.Printing
{
    public abstract partial class PrintController
    {
        protected PrintController() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public virtual bool IsPreview { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public virtual void OnEndPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public virtual void OnEndPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public virtual System.Drawing.Graphics? OnStartPage(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintPageEventArgs e) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public virtual void OnStartPrint(System.Drawing.Printing.PrintDocument document, System.Drawing.Printing.PrintEventArgs e) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
    }
}
