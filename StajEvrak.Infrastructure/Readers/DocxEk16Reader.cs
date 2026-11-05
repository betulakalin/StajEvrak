using System.Globalization;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using StajEvrak.Core.Entities;
using StajEvrak.Core.Interfaces;

namespace StajEvrak.Infrastructure.Readers;

public class DocxEk16Reader : IEk16Reader
{
    private static readonly CultureInfo Tr = new("tr-TR");
    private static readonly string[] DateFormats =
        { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };

    public bool CanRead(string extension) => extension == ".docx";

    public Ek16Data Read(Stream stream)
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

        // "10000000146 / T.C." → kimlik no + uyruk
        var idAndNationality = Get(s, "T.C. KİMLİK NO");
        var nationalId = Regex.Match(idAndNationality ?? "", @"\d{11}");
        var nationality = Regex.Replace(idAndNationality ?? "", @"\d{11}", "").Trim(' ', '/', '-');

        // "ali@ornek.com / +90 532 111 22 33" → e-posta + telefon
        var emailAndPhone = Get(s, "E-POSTA") ?? "";
        var email = Regex.Match(emailAndPhone, @"[\w.+-]+@[\w-]+(\.[\w-]+)+");
        var phone = (email.Success ? emailAndPhone.Replace(email.Value, "") : emailAndPhone).Trim(' ', '/', ',', ';');

        // Sağlık statüsü: işaretli kutucuk "2-☒" gibi görünür (elle "2- X" yazılmış da olabilir)
        var health = Regex.Match(Get(s, "SAĞLIK YARDIM") ?? "", @"(\d)\s*-\s*[☒Xx✓✔]");

        return new Ek16Data
        {
            Source = DataSource.Extracted,

            NationalId = nationalId.Success ? nationalId.Value : null,
            Nationality = nationality.Length > 0 ? nationality : null,
            FullName = Get(s, "ADI SOYADI"),
            BirthDate = ParseDate(Get(s, "DOĞUM TARİHİ")),
            StudentNo = Get(s, "ÖĞRENCİ NUMARASI"),
            Address = Get(s, "İKAMETG"),
            Department = Get(s, "BÖLÜMÜ"),
            PreviousInternshipDays = ParseInt(Get(s, "DAHA ÖNCE YAPTIĞI STAJ")),
            AcademicTerm = Get(s, "ÖĞRETİM YILI"),
            Email = email.Success ? email.Value : null,
            Phone = phone.Length > 0 ? phone : null,
            HealthStatus = health.Success ? health.Groups[1].Value : null,

            CompanyName = Get(c, "ADI"),
            CompanyPhone = Get(c, "TELEFON NO"),
            CompanyFax = Get(c, "FAX NO"),
            CompanyEmail = Get(c, "E-POSTA ADRESİ"),
            EngineerCount = ParseInt(Get(c, "MÜHENDİS SAYISI")),
            CompanyAddress = Get(c, "ADRES BİLGİSİ"),
            InternshipStart = ParseDate(Get(c, "STAJ BAŞLAMA TARİHİ")),
            InternshipEnd = ParseDate(Get(c, "STAJ BİTİŞ TARİHİ")),
            WorkDaysCount = ParseInt(Get(c, "STAJ GÜN SAYISI"))
        };
    }

    // Etiketleri karşılaştırırken büyük/küçük harf, boşluk ve noktalama farkını yok sayıyoruz
    private static string Normalize(string? s) =>
        new string((s ?? "").ToUpper(Tr).Where(char.IsLetterOrDigit).ToArray());

    private static Table? FindTable(List<Table> tables, string label) =>
        tables.FirstOrDefault(t => Normalize(t.InnerText).Contains(Normalize(label)));

    private static string CellText(TableCell cell) =>
        Regex.Replace(string.Join(" ", cell.Descendants<Paragraph>().Select(p => p.InnerText)), @"\s+", " ").Trim();

    // Her satır "etiket | değer | etiket | değer" şeklinde: çift sıradaki hücre etiket, yanındaki değer
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

    private static DateTime? ParseDate(string? s) =>
        DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d : null;

    private static int? ParseInt(string? s)
    {
        var m = Regex.Match(s ?? "", @"\d+");
        return m.Success ? int.Parse(m.Value) : null;
    }
}