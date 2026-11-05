using System.Globalization;
using System.Text.RegularExpressions;
using StajEvrak.Core.Entities;
using StajEvrak.Core.Interfaces;

namespace StajEvrak.Infrastructure.Readers;

// Word ve PDF okuyucularının ortak kısmı: etiketlerden değerleri alıp Ek16Data'ya çevirmek
public abstract class Ek16ReaderBase : IEk16Reader
{
    private static readonly CultureInfo Tr = new("tr-TR");
    private static readonly string[] DateFormats =
        { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };

    public abstract bool CanRead(string extension);

    // Alt sınıflar sadece şunu bilir: "etiketi ver, yanındaki değeri bul"
    protected abstract (Func<string, string?> Student, Func<string, string?> Company) ReadFields(Stream stream);

    public Ek16Data Read(Stream stream)
    {
        var (s, c) = ReadFields(stream);

        // "10000000146 / T.C." → kimlik no + uyruk
        var idAndNationality = s("T.C. KİMLİK NO") ?? "";
        var nationalId = Regex.Match(idAndNationality, @"\d{11}");
        var nationality = Regex.Replace(idAndNationality, @"\d{11}", "").Trim(' ', '/', '-');

        // "ali@ornek.com / +90 532 111 22 33" → e-posta + telefon
        var emailAndPhone = s("E-POSTA") ?? "";
        var email = Regex.Match(emailAndPhone, @"[\w.+-]+@[\w-]+(\.[\w-]+)+");
        var phone = (email.Success ? emailAndPhone.Replace(email.Value, "") : emailAndPhone).Trim(' ', '/', ',', ';');

        // Sağlık statüsü: işaretli kutucuk "2-☒" gibi görünür (elle "2- X" yazılmış da olabilir)
        var health = Regex.Match(s("SAĞLIK YARDIM") ?? "", @"(\d)\s*-\s*[☒Xx✓✔]");

        return new Ek16Data
        {
            Source = DataSource.Extracted,

            NationalId = nationalId.Success ? nationalId.Value : null,
            Nationality = nationality.Length > 0 ? nationality : null,
            FullName = s("ADI SOYADI"),
            BirthDate = ParseDate(s("DOĞUM TARİHİ")),
            StudentNo = s("ÖĞRENCİ NUMARASI"),
            Address = s("İKAMETG"),
            Department = s("BÖLÜMÜ"),
            PreviousInternshipDays = ParseInt(s("DAHA ÖNCE YAPTIĞI STAJ")),
            AcademicTerm = s("ÖĞRETİM YILI"),
            Email = email.Success ? email.Value : null,
            Phone = phone.Length > 0 ? phone : null,
            HealthStatus = health.Success ? health.Groups[1].Value : null,

            CompanyName = c("ADI"),
            CompanyPhone = c("TELEFON NO"),
            CompanyFax = c("FAX NO"),
            CompanyEmail = c("E-POSTA ADRESİ")?.Replace(" ", ""),   // Dar hücrede satıra bölünmüş olabilir
            EngineerCount = ParseInt(c("MÜHENDİS SAYISI")),
            CompanyAddress = c("ADRES BİLGİSİ"),
            InternshipStart = ParseDate(c("STAJ BAŞLAMA TARİHİ")),
            InternshipEnd = ParseDate(c("STAJ BİTİŞ TARİHİ")),
            WorkDaysCount = ParseInt(c("STAJ GÜN SAYISI"))
        };
    }

    // Büyük/küçük harf, boşluk ve noktalama farkını yok sayar
    protected static string Normalize(string? s) =>
        new string((s ?? "").ToUpper(Tr).Where(char.IsLetterOrDigit).ToArray());

    private static DateTime? ParseDate(string? s) =>
        DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d : null;

    private static int? ParseInt(string? s)
    {
        var m = Regex.Match(s ?? "", @"\d+");
        return m.Success ? int.Parse(m.Value) : null;
    }
}