using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace TelerikExportPoC;

internal class Program
{
    private static void Main()
    {
        var guid = Guid.NewGuid();
        var filePath = $"C:\\Users\\jiri.volesky\\Desktop\\TelerikPocOutput\\{guid}_RadTest.xlsx";
        var companies = GetMockCompanies();

        var workbook = new Workbook();
        var worksheet = workbook.Worksheets.Add();
        worksheet.Name = "Companies";

        var headers = new[] { "Name", "VATNo", "Created", "MoneyOwed", "Country" };
        WriteRow(worksheet, 0, headers);

        for (var i = 0; i < companies.Count; i++)
        {
            var c = companies[i];
            var values = new object[] { c.Name, c.VatNo, c.Created, c.MoneyOwed, c.Country };
            WriteRow(worksheet, i + 1, values);
        }

        var formatProvider = new XlsxFormatProvider();
        using var output = new FileStream(filePath, FileMode.Create);
        formatProvider.Export(workbook, output, TimeSpan.MaxValue);

        Console.WriteLine("Export ok: " + Path.GetFullPath(filePath));
    }

    private static void WriteRow(Worksheet worksheet, int rowIndex, IEnumerable<object> values)
    {
        int colIndex = 0;
        foreach (var value in values)
        {
            var cell = worksheet.Cells[rowIndex, colIndex];

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
                    cell.SetValue(Convert.ToDouble(dec));
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

            colIndex++;
        }
    }

    private static List<Company> GetMockCompanies()
    {
        var rnd = new Random();
        var countries = new[] { "GE", "FR", "CZ", "USA", "JAP" };
        return Enumerable.Range(1, 20)
            .Select(i => new Company(
                $"Company {i}",
                10000000 + i,
                DateTime.Today.AddDays(-i),
                (decimal)(rnd.NextDouble() * 10000),
                countries[rnd.Next(countries.Length)]
            )).ToList();
    }

    private sealed record Company(string Name, int VatNo, DateTime Created, decimal MoneyOwed, string Country);
}