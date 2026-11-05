using System.Globalization;
using StajEvrak.Core.Entities;

namespace StajEvrak.Core.Services;

public record ComparisonItem(string Field, string? Expected, string? Actual, bool IsMatch);

public static class Ek16Comparer
{
    private static readonly CultureInfo Tr = new("tr-TR");

    // Form ↔ veri seti
    public static List<ComparisonItem> CompareWithRecord(Ek16Data form, StudentRecord record) => new()
    {
        Text("Ad Soyad", record.FullName, form.FullName),
        Text("Kimlik No", record.NationalId, form.NationalId),
        Text("Uyruk", record.Nationality, form.Nationality),
        Text("Öğrenci No", record.StudentNo, form.StudentNo),
        Date("Doğum Tarihi", record.BirthDate, form.BirthDate),
        Text("Bölüm", record.Department, form.Department),
        Text("E-posta", record.Email, form.Email),
        Text("Telefon", record.Phone, form.Phone),
        Text("Adres", record.Address, form.Address),
        Number("Önceki Staj Günü", record.PreviousInternshipDays, form.PreviousInternshipDays),
    };

    // Form ↔ belgeden okunan
    public static List<ComparisonItem> CompareWithDocument(Ek16Data form, Ek16Data doc) => new()
    {
        Text("Ad Soyad", doc.FullName, form.FullName),
        Text("Kimlik No", doc.NationalId, form.NationalId),
        Text("Uyruk", doc.Nationality, form.Nationality),
        Text("Öğrenci No", doc.StudentNo, form.StudentNo),
        Date("Doğum Tarihi", doc.BirthDate, form.BirthDate),
        Text("Bölüm", doc.Department, form.Department),
        Text("Öğretim Yılı", doc.AcademicTerm, form.AcademicTerm),
        Text("E-posta", doc.Email, form.Email),
        Text("Telefon", doc.Phone, form.Phone),
        Text("Adres", doc.Address, form.Address),
        Number("Önceki Staj Günü", doc.PreviousInternshipDays, form.PreviousInternshipDays),
        Text("Sağlık Statüsü", doc.HealthStatus, form.HealthStatus),
        Date("Staj Başlangıç", doc.InternshipStart, form.InternshipStart),
        Date("Staj Bitiş", doc.InternshipEnd, form.InternshipEnd),
        Number("Staj Gün Sayısı", doc.WorkDaysCount, form.WorkDaysCount),
        Text("Firma Adı", doc.CompanyName, form.CompanyName),
        Text("Firma Adresi", doc.CompanyAddress, form.CompanyAddress),
        Text("Firma Telefonu", doc.CompanyPhone, form.CompanyPhone),
        Text("Firma Faks", doc.CompanyFax, form.CompanyFax),
        Text("Firma E-posta", doc.CompanyEmail, form.CompanyEmail),
        Number("Mühendis Sayısı", doc.EngineerCount, form.EngineerCount),
    };

    private static string Normalize(string? s) =>
        new string((s ?? "").ToUpper(Tr).Where(char.IsLetterOrDigit).ToArray());

    private static ComparisonItem Text(string field, string? expected, string? actual) =>
        new(field, expected, actual, Normalize(expected) == Normalize(actual));

    private static ComparisonItem Date(string field, DateTime? expected, DateTime? actual) =>
        new(field, expected?.ToString("dd.MM.yyyy"), actual?.ToString("dd.MM.yyyy"),
            expected?.Date == actual?.Date);

    private static ComparisonItem Number(string field, int? expected, int? actual) =>
        new(field, expected?.ToString(), actual?.ToString(), expected == actual);
}