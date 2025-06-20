using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders;
using Telerik.Windows.Documents.Spreadsheet.Model;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Documents.Fixed.FormatProviders.Image.Skia;
using Telerik.Windows.Documents.Extensibility;
using Telerik.Windows.Documents.Spreadsheet.Extensibility;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.Spreadsheet.Model.Printing;
using Telerik.Documents.Primitives;
using Telerik.Windows.Documents.Flow.FormatProviders.Docx;
using Telerik.Windows.Documents.Extensibility;
using Telerik.Windows.Documents.Flow.FormatProviders.Docx;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;

namespace TelerikPreviewPoC;

internal class Program
{
    private static void Main()
    {
        var inputDocxPath = "C:\\Users\\jiri.volesky\\Desktop\\TelerikPocOutput\\DOCX_NEW.docx";
        var outputImageFromDocx = "C:\\Users\\jiri.volesky\\Desktop\\TelerikPreviewPoCOutput\\docx_test.jpg";

        var docxContent = File.ReadAllBytes(inputDocxPath);

        var docxFormatProvider = new Telerik.Windows.Documents.Flow.FormatProviders.Docx.DocxFormatProvider();
        var document = docxFormatProvider.Import(docxContent);

        var pdfFormatProvider = new Telerik.Windows.Documents.Flow.FormatProviders.Pdf.PdfFormatProvider();

        using var pdfStream = new MemoryStream();
        pdfFormatProvider.Export(document, pdfStream);

        // Load exported PDF into RadFixedDocument
        pdfStream.Position = 0;
        var fixedPdfProvider = new Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.PdfFormatProvider();
        var fixedDocument = fixedPdfProvider.Import(pdfStream);

        // Convert first page of PDF to image
        var imageProvider = new Telerik.Documents.Fixed.FormatProviders.Image.Skia.SkiaImageFormatProvider();

        foreach (var page in fixedDocument.Pages)
        {
            var imageBytes = imageProvider.Export(page);
            File.WriteAllBytes(outputImageFromDocx, imageBytes);
            break; // export only the first page
        }


        //var inputPath = "C:\\Users\\jiri.volesky\\Desktop\\TelerikPocOutput\\XLSX_NEW.xlsx";
        //var outputImage = "C:\\Users\\jiri.volesky\\Desktop\\TelerikPreviewPoCOutput\\test.jpg";
        //var debugPdf = "C:\\Users\\jiri.volesky\\Desktop\\TelerikPreviewPoCOutput\\preview_debug.pdf";

        ////.NET Standard Requirements
        //SpreadTextMeasurerBase fixedTextMeasurer = new SpreadFixedTextMeasurer();
        //SpreadExtensibilityManager.TextMeasurer = fixedTextMeasurer;

        ////Import Workbook
        //Workbook file;
        //IWorkbookFormatProvider xlsxFormatProvider = new XlsxFormatProvider();

        //using (Stream input = new FileStream(inputPath, FileMode.Open))
        //{
        //    file = xlsxFormatProvider.Import(input, TimeSpan.MaxValue);
        //}

        ////Set PDF export settings to your preference
        //var pageSetup = file.ActiveWorksheet.WorksheetPageSetup;
        //pageSetup.PageOrientation = PageOrientation.Landscape;
        //pageSetup.Margins = new PageMargins(0, 0, 0, 0);
        //pageSetup.PaperType =
        //    PaperTypes.A1; /*make sure the size of the PaperType is bigger than the dimentions of worksheetTableSizetableSize*/

        //var spreadPdfFormatProvider =
        //    new Telerik.Windows.Documents.Spreadsheet.FormatProviders.Pdf.PdfFormatProvider
        //    {
        //        ExportSettings = new Telerik.Windows.Documents.Spreadsheet.FormatProviders.Pdf.Export.PdfExportSettings(
        //            ExportWhat.ActiveSheet, true)
        //    };

        ////Export as PDF
        //using (Stream output = File.OpenWrite(debugPdf))
        //{
        //    spreadPdfFormatProvider.Export(file, output, TimeSpan.MaxValue);
        //}

        ////Import PDF 
        //RadFixedDocument pdfDocument;
        //var fixedPdfFormatProvider =
        //    new Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.PdfFormatProvider();

        //using (Stream input = File.OpenRead(debugPdf))
        //{
        //    pdfDocument = fixedPdfFormatProvider.Import(input, TimeSpan.MaxValue);
        //}

        ////Set new PDF page size
        //var worksheetTableSize = CalculateRangeSize(file.ActiveWorksheet, file.ActiveWorksheet.UsedCellRange);
        //RadFixedPage? first = null;
        //foreach (var page in pdfDocument.Pages)
        //{
        //    first = page;
        //    break;
        //}

        //if (first != null) first.Size = new Size(worksheetTableSize.Width, worksheetTableSize.Height);

        ////Export to image
        //var imageProvider = new SkiaImageFormatProvider();

        //foreach (RadFixedPage page in pdfDocument.Pages)
        //{
        //    var resultImage = imageProvider.Export(page, TimeSpan.MaxValue);
        //    File.WriteAllBytes(outputImage, resultImage);
        //}

        //Size CalculateRangeSize(Worksheet worksheet, CellRange range)
        //{
        //    double totalHeight = 0;
        //    double totalWidth = 0;

        //    for (int row = range.FromIndex.RowIndex; row <= range.ToIndex.RowIndex; row++)
        //    {
        //        double rowHeight = worksheet.Rows[row].GetHeight().Value.Value;
        //        totalHeight += rowHeight;
        //    }

        //    for (int column = range.FromIndex.ColumnIndex; column <= range.ToIndex.ColumnIndex; column++)
        //    {
        //        double columnWidth = worksheet.Columns[column].GetWidth().Value.Value;
        //        totalWidth += columnWidth;
        //    }

        //    return new Size(totalWidth, totalHeight);
        //}
    }
}