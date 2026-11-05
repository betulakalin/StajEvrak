namespace StajEvrak.Core.Entities;

public class StudentRecord
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string NationalId { get; set; } = null!;
    public string Nationality { get; set; } = null!;
    public string StudentNo { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public string Address { get; set; } = null!;
    public string Department { get; set; } = null!;
    public string SubmissionYear { get; set; } = null!;
    public int PreviousInternshipDays { get; set; }
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string HealthInsuranceStatus { get; set; } = null!;

    // Aynı öğrenci no tekrar yüklenince mevcut kaydı günceller
    public void UpdateFrom(StudentRecord other)
    {
        FullName = other.FullName;
        NationalId = other.NationalId;
        Nationality = other.Nationality;
        BirthDate = other.BirthDate;
        Address = other.Address;
        Department = other.Department;
        SubmissionYear = other.SubmissionYear;
        PreviousInternshipDays = other.PreviousInternshipDays;
        Email = other.Email;
        Phone = other.Phone;
        HealthInsuranceStatus = other.HealthInsuranceStatus;
    }
}