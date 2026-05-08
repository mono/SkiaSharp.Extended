using System.Drawing;
using System.Drawing.Printing;

namespace SkiaSharp.Extended.Drawing.Common.Tests;

public class PrintingTests
{
	[Fact]
	public void PrintDocument_CanCreate()
	{
		using var doc = new PrintDocument();
		Assert.Equal("document", doc.DocumentName);
	}

	[Fact]
	public void PrintDocument_HasDefaultSettings()
	{
		using var doc = new PrintDocument();
		Assert.NotNull(doc.PrinterSettings);
		Assert.NotNull(doc.DefaultPageSettings);
		Assert.True(doc.PrinterSettings.IsValid);
	}

	[Fact]
	public void PageSettings_DefaultValues()
	{
		var ps = new PageSettings();
		Assert.False(ps.Landscape);
		Assert.NotNull(ps.Margins);
		Assert.NotNull(ps.PaperSize);
		Assert.Equal(100, ps.Margins.Left); // 1 inch
	}

	[Fact]
	public void PageSettings_Bounds()
	{
		var ps = new PageSettings();
		var bounds = ps.Bounds;
		Assert.True(bounds.Width > 0);
		Assert.True(bounds.Height > 0);
	}

	[Fact]
	public void PageSettings_BoundsLandscape()
	{
		var ps = new PageSettings { Landscape = true };
		var bounds = ps.Bounds;
		// In landscape, width should be the paper height and vice versa
		Assert.Equal(1100, bounds.Width);
		Assert.Equal(850, bounds.Height);
	}

	[Fact]
	public void PrinterSettings_DefaultPrinter()
	{
		var settings = new PrinterSettings();
		Assert.True(settings.IsValid);
		Assert.Equal(1, settings.Copies);
	}

	[Fact]
	public void PrinterSettings_PaperSizes()
	{
		var settings = new PrinterSettings();
		Assert.True(settings.PaperSizes.Count > 0);
	}

	[Fact]
	public void PrinterSettings_InstalledPrinters()
	{
		Assert.True(PrinterSettings.InstalledPrinters.Count > 0);
	}

	[Fact]
	public void PrinterSettings_CreateMeasurementGraphics()
	{
		var settings = new PrinterSettings();
		using var g = settings.CreateMeasurementGraphics();
		Assert.NotNull(g);
	}

	[Fact]
	public void PrintDocument_PrintToPdf()
	{
		var pdfPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.pdf");
		try
		{
			using var doc = new PrintDocument();
			doc.PrinterSettings.PrintToFile = true;
			doc.PrinterSettings.PrintFileName = pdfPath;

			int pageCount = 0;
			doc.PrintPage += (sender, e) =>
			{
				pageCount++;
				e.Graphics!.DrawRectangle(Pens.Black, 10, 10, 200, 200);
				e.Graphics.DrawString("Hello PDF!", new Font("Arial", 24), Brushes.Black, 50, 50);
				e.HasMorePages = pageCount < 2; // 2 pages
			};

			doc.Print();

			Assert.Equal(2, pageCount);
			Assert.True(File.Exists(pdfPath));
			Assert.True(new FileInfo(pdfPath).Length > 0);
		}
		finally
		{
			if (File.Exists(pdfPath))
				File.Delete(pdfPath);
		}
	}

	[Fact]
	public void PrintDocument_MultiPage()
	{
		var pdfPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.pdf");
		try
		{
			using var doc = new PrintDocument();
			doc.PrinterSettings.PrintToFile = true;
			doc.PrinterSettings.PrintFileName = pdfPath;

			int page = 0;
			doc.PrintPage += (sender, e) =>
			{
				page++;
				e.Graphics!.DrawString($"Page {page}", new Font("Arial", 36), Brushes.Black, 100, 100);
				e.HasMorePages = page < 5;
			};

			doc.Print();
			Assert.Equal(5, page);
			Assert.True(new FileInfo(pdfPath).Length > 100);
		}
		finally
		{
			if (File.Exists(pdfPath))
				File.Delete(pdfPath);
		}
	}

	[Fact]
	public void PreviewPrintController_RendersPages()
	{
		using var doc = new PrintDocument();
		var preview = new PreviewPrintController();
		doc.PrintController = preview;

		doc.PrintPage += (sender, e) =>
		{
			e.Graphics!.FillRectangle(Brushes.Red, 10, 10, 100, 100);
			e.HasMorePages = false;
		};

		doc.Print();

		var pages = preview.GetPreviewPageInfo();
		Assert.NotNull(pages);
		Assert.Single(pages);
		Assert.NotNull(pages[0].Image);
	}

	[Fact]
	public void Margins_Properties()
	{
		var m = new Margins(50, 50, 100, 100);
		Assert.Equal(50, m.Left);
		Assert.Equal(50, m.Right);
		Assert.Equal(100, m.Top);
		Assert.Equal(100, m.Bottom);
	}

	[Fact]
	public void PaperSize_Standard()
	{
		var letter = new PaperSize("Letter", 850, 1100);
		Assert.Equal("Letter", letter.PaperName);
		Assert.Equal(850, letter.Width);
		Assert.Equal(1100, letter.Height);
	}

	[Fact]
	public void PageSettings_Clone()
	{
		var ps = new PageSettings
		{
			Landscape = true,
			Margins = new Margins(50, 50, 75, 75)
		};

		var clone = (PageSettings)ps.Clone();
		Assert.True(clone.Landscape);
		Assert.Equal(50, clone.Margins.Left);

		// Modifying clone should not affect original
		clone.Margins.Left = 200;
		Assert.Equal(50, ps.Margins.Left);
	}

	[Fact]
	public void PrinterSettings_Clone()
	{
		var settings = new PrinterSettings
		{
			Copies = 3,
			PrinterName = "TestPrinter"
		};

		var clone = (PrinterSettings)settings.Clone();
		Assert.Equal(3, clone.Copies);
		Assert.Equal("TestPrinter", clone.PrinterName);
	}

	[Fact]
	public void PrinterUnitConvert_DisplayToThousandths()
	{
		// Display units are hundredths of an inch
		// 100 hundredths = 1 inch = 1000 thousandths
		int result = PrinterUnitConvert.Convert(100, PrinterUnit.Display, PrinterUnit.ThousandthsOfAnInch);
		Assert.Equal(1000, result);
	}

	[Fact]
	public void PageSettings_PrintableArea()
	{
		var ps = new PageSettings
		{
			PaperSize = new PaperSize("Letter", 850, 1100),
			Margins = new Margins(100, 100, 100, 100)
		};

		var area = ps.PrintableArea;
		Assert.Equal(100, area.X);
		Assert.Equal(100, area.Y);
		Assert.Equal(650, area.Width);
		Assert.Equal(900, area.Height);
	}
}
