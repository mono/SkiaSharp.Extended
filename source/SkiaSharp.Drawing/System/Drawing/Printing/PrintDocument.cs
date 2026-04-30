namespace System.Drawing.Printing
{
    [System.ComponentModel.DefaultEventAttribute("PrintPage")]
    [System.ComponentModel.DefaultPropertyAttribute("DocumentName")]
    public partial class PrintDocument : System.ComponentModel.Component
    {
        public PrintDocument() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Printing.PageSettings DefaultPageSettings { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        [System.ComponentModel.DefaultValueAttribute("document")]
        public string DocumentName { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool OriginAtMargins { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Printing.PrintController PrintController { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Printing.PrinterSettings PrinterSettings { get { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } set { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public event System.Drawing.Printing.PrintEventHandler BeginPrint { add { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } remove { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public event System.Drawing.Printing.PrintEventHandler EndPrint { add { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } remove { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public void Print() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        public event System.Drawing.Printing.PrintPageEventHandler PrintPage { add { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } remove { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public event System.Drawing.Printing.QueryPageSettingsEventHandler QueryPageSettings { add { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } remove { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); } }
        public override string ToString() { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        protected internal virtual void OnBeginPrint(System.Drawing.Printing.PrintEventArgs e) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        protected internal virtual void OnEndPrint(System.Drawing.Printing.PrintEventArgs e) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        protected internal virtual void OnPrintPage(System.Drawing.Printing.PrintPageEventArgs e) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
        protected internal virtual void OnQueryPageSettings(System.Drawing.Printing.QueryPageSettingsEventArgs e) { throw new System.PlatformNotSupportedException("Not yet implemented in SkiaSharp.Drawing"); }
    }
}
