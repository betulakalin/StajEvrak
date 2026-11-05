using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace StajEvrak.Infrastructure.Readers;

public class DocxEk16Reader : Ek16ReaderBase
{
    public override bool CanRead(string extension) => extension == ".docx";

    protected override (Func<string, string?> Student, Func<string, string?> Company) ReadFields(Stream stream)
    {
        using var word = WordprocessingDocument.Open(stream, false);
        var body = word.MainDocumentPart?.Document?.Body
                   ?? throw new InvalidDataException("Belgenin içeriği okunamadı.");

        var tables = body.Descendants<Table>().ToList();

        // Tabloları sıralarına göre değil, içerdikleri etiketlere göre buluyoruz
        var studentTable = FindTable(tables, "ÖĞRENCİ NUMARASI");
        var companyTable = FindTable(tables, "STAJ BAŞLAMA TARİHİ");
        if (studentTable == null || companyTable == null)
            throw new InvalidDataException("Belge Ek-16 formatında değil (öğrenci veya staj yeri tablosu bulunamadı).");

        var s = ReadLabelValues(studentTable);
        var c = ReadLabelValues(companyTable);

        Func<string, string?> student = label => Get(s, label);
        Func<string, string?> company = label => Get(c, label);
        return (student, company);
    }

    private static Table? FindTable(List<Table> tables, string label) =>
        tables.FirstOrDefault(t => Normalize(t.InnerText).Contains(Normalize(label)));

    private static string CellText(TableCell cell) =>
        Regex.Replace(string.Join(" ", cell.Descendants<Paragraph>().Select(p => p.InnerText)), @"\s+", " ").Trim();

    // Her satır "etiket | değer | etiket | değer" şeklinde
    private static Dictionary<string, string> ReadLabelValues(Table table)
    {
        var map = new Dictionary<string, string>();
        foreach (var row in table.Elements<TableRow>())
        {
            var cells = row.Elements<TableCell>().ToList();
            for (int i = 0; i + 1 < cells.Count; i += 2)
            {
                var key = Normalize(CellText(cells[i]));
                if (key.Length > 0 && !map.ContainsKey(key))
                    map[key] = CellText(cells[i + 1]);
            }
        }
        return map;
    }

    // Önce birebir eşleşme ("ADI" ile "ADI SOYADI" karışmasın), yoksa "ile başlayan" eşleşme
    private static string? Get(Dictionary<string, string> map, string label)
    {
        var key = Normalize(label);
        if (!map.TryGetValue(key, out var value))
            value = map.FirstOrDefault(kv => kv.Key.StartsWith(key)).Value;

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}