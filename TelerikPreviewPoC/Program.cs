using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Docnet.Core;
using Docnet.Core.Models;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.Pdf;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace TelerikPreviewPoC;

internal class Program
{
    static void Main(string[] args)
    {
        var inputPath = "C:\\Users\\jiri.volesky\\Desktop\\TelerikPocOutput\\XLSX_NEW.xlsx";
        var outputImage = "C:\\Users\\jiri.volesky\\Desktop\\TelerikPreviewPoCOutput\\test.jpg";
        var debugPdf = "C:\\Users\\jiri.volesky\\Desktop\\TelerikPreviewPoCOutput\\preview_debug.pdf";

        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"Not found: {inputPath}");
            return;
        }

        try
        {
            var xlsxProvider = new XlsxFormatProvider();
            Workbook workbook;
            using (var stream = File.OpenRead(inputPath))
            {
                workbook = xlsxProvider.Import(stream, TimeSpan.MaxValue);
            }

            var sheet = workbook.Worksheets[0];
            sheet.WorksheetPageSetup.FitToPagesWide = 1;
            sheet.WorksheetPageSetup.FitToPagesTall = 1;
            sheet.WorksheetPageSetup.PrintOptions.PrintGridlines = true;

            var pdfProvider = new PdfFormatProvider();
            var pdfBytes = pdfProvider.Export(workbook, TimeSpan.MaxValue);

            File.WriteAllBytes(debugPdf, pdfBytes);
            Console.WriteLine($"PDF debug: {debugPdf}");

            var docLib = DocLib.Instance;
            using var docReader = docLib.GetDocReader(pdfBytes, new PageDimensions(150, 150));
            using var pageReader = docReader.GetPageReader(0);

            var rawBytes = pageReader.GetImage();
            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();

            using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(rawBytes, 0, bmpData.Scan0, rawBytes.Length);
            bmp.UnlockBits(bmpData);

            bmp.Save(outputImage, ImageFormat.Jpeg);
            Console.WriteLine($"PDF saved: {outputImage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error");
            Console.WriteLine(ex);
        }
    }
}