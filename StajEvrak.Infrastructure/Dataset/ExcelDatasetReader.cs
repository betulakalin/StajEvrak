using ClosedXML.Excel;

namespace StajEvrak.Infrastructure.Dataset;

public class ExcelDatasetReader : DatasetReaderBase
{
    public override bool CanRead(string extension) => extension == ".xlsx";

    protected override IEnumerable<string[]> ReadRows(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(1);

        foreach (var row in sheet.RowsUsed())
        {
            yield return Enumerable.Range(1, ColumnCount)
                .Select(i => CellToString(row.Cell(i)))
                .ToArray();
        }
    }

    private static string CellToString(IXLCell cell) =>
        cell.DataType == XLDataType.DateTime
            ? cell.GetDateTime().ToString("dd.MM.yyyy")
            : cell.GetString();
}