namespace StajEvrak.Core.Entities;

public enum DataSource { Manual, Extracted }

public class Ek16Data
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public DataSource Source { get; set; }

    // Öğrenci bilgileri
    public string? FullName { get; set; }
    public string? NationalId { get; set; }
    public string? Nationality { get; set; }
    public string? StudentNo { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? AcademicTerm { get; set; }
    public int? PreviousInternshipDays { get; set; }
    public string? HealthStatus { get; set; }          // Ek-16'daki kutucuk numarası: "1" - "6"

    // Staj bilgileri
    public DateTime? InternshipStart { get; set; }
    public DateTime? InternshipEnd { get; set; }
    public int? WorkDaysCount { get; set; }

    // İşyeri bilgileri
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyFax { get; set; }
    public string? CompanyEmail { get; set; }
    public int? EngineerCount { get; set; }

    public void CopyFrom(Ek16Data o)
    {
        FullName = o.FullName; NationalId = o.NationalId; Nationality = o.Nationality;
        StudentNo = o.StudentNo; BirthDate = o.BirthDate; Department = o.Department;
        Email = o.Email; Phone = o.Phone; Address = o.Address; AcademicTerm = o.AcademicTerm;
        PreviousInternshipDays = o.PreviousInternshipDays; HealthStatus = o.HealthStatus;
        InternshipStart = o.InternshipStart; InternshipEnd = o.InternshipEnd; WorkDaysCount = o.WorkDaysCount;
        CompanyName = o.CompanyName; CompanyAddress = o.CompanyAddress; CompanyPhone = o.CompanyPhone;
        CompanyFax = o.CompanyFax; CompanyEmail = o.CompanyEmail; EngineerCount = o.EngineerCount;
    }
}