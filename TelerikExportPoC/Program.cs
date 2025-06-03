using Telerik.Documents.SpreadsheetStreaming;

namespace TelerikExportPoC;

internal class Program
{
    private static void Main()
    {
        var guid = Guid.NewGuid();
        var filePath = $"C:\\Users\\jiri.volesky\\Desktop\\TelerikPocOutput\\{guid}_Test.xlsx";
        var companies = GetMockCompanies();

        var headers = new[] { "Name", "VATNo", "Created", "MoneyOwed", "Country" };

        var rows = companies.Select(c => new object[]
        {
            c.Name,
            c.VatNo,
            c.Created,
            c.MoneyOwed,
            c.Country
        });

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var workbook =
                SpreadExporter.CreateWorkbookExporter(SpreadDocumentFormat.Xlsx, fileStream, SpreadExportMode.Create);
            using var worksheet = workbook.CreateWorksheetExporter("Companies");

            WriteRow(worksheet, headers);

            foreach (var row in rows)
            {
                WriteRow(worksheet, row);
            }

            Console.WriteLine("Export complete: " + Path.GetFullPath(filePath));
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            throw;
        }
    }

    private static void WriteRow(IWorksheetExporter worksheet, IEnumerable<object> values)
    {
        using var row = worksheet.CreateRowExporter();
        foreach (var value in values)
            WriteCell(row, value);
    }

    private static void WriteCell(IRowExporter row, object value)
    {
        using var cell = row.CreateCellExporter();

        switch (value)
        {
            case null:
                cell.SetValue(string.Empty);
                break;
            case string s:
                cell.SetValue(s);
                break;
            case int i:
                cell.SetValue(i);
                break;
            case double d:
                cell.SetValue(d);
                break;
            case decimal dec:
                cell.SetValue((double)dec);
                break;
            case bool b:
                cell.SetValue(b);
                break;
            case DateTime dt:
                cell.SetValue(dt);
                break;
            default:
                cell.SetValue(value.ToString());
                break;
        }
    }

    private static List<CompanyEntry> GetMockCompanies()
    {
        var rnd = new Random();
        var countries = new[] { "GE", "FR", "CZ", "USA", "JAP" };
        return Enumerable.Range(1, 20)
            .Select(i => new CompanyEntry(
                $"Company {i}",
                10000000 + i,
                DateTime.Today.AddDays(-i),
                (decimal)(rnd.NextDouble() * 10000),
                countries[rnd.Next(countries.Length)]
            )).ToList();
    }

    private sealed record CompanyEntry(string Name, int VatNo, DateTime Created, decimal MoneyOwed, string Country);
}