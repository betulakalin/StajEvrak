using System.ComponentModel.DataAnnotations;
using StajEvrak.Core.Entities;
using StajEvrak.Infrastructure.Services;

namespace StajEvrak.web.Models;

public class Ek16FormViewModel
{
    private const string Req = "{0} zorunludur.";

    // Öğrenci
    [Required(ErrorMessage = Req), Display(Name = "Ad Soyad")] public string FullName { get; set; } = "";
    [Required(ErrorMessage = Req), Display(Name = "TC / Yabancı Kimlik No")] public string NationalId { get; set; } = "";
    [Required(ErrorMessage = Req), Display(Name = "Uyruk")] public string Nationality { get; set; } = "";
    [Required(ErrorMessage = Req), Display(Name = "Öğrenci No")] public string StudentNo { get; set; } = "";
    [Required(ErrorMessage = Req), DataType(DataType.Date), Display(Name = "Doğum Tarihi")] public DateTime? BirthDate { get; set; }
    [Required(ErrorMessage = Req), Display(Name = "Bölüm")] public string Department { get; set; } = "";
    [Required(ErrorMessage = Req), EmailAddress(ErrorMessage = "Geçerli bir e-posta girin."), Display(Name = "E-posta")] public string Email { get; set; } = "";
    [Required(ErrorMessage = Req), Display(Name = "Telefon")] public string Phone { get; set; } = "";
    [Required(ErrorMessage = Req), Display(Name = "İkametgâh Adresi")] public string Address { get; set; } = "";
    [Required(ErrorMessage = Req), Display(Name = "Öğretim Yılı / Dönem")] public string AcademicTerm { get; set; } = "";
    [Required(ErrorMessage = Req), Range(0, 1000, ErrorMessage = "Geçerli bir gün sayısı girin."), Display(Name = "Daha Önce Yapılan Staj Günü")] public int? PreviousInternshipDays { get; set; }
    [Required(ErrorMessage = Req), Display(Name = "Sağlık Yardım Statüsü")] public string HealthStatus { get; set; } = "";

    // Staj
    [Required(ErrorMessage = Req), DataType(DataType.Date), Display(Name = "Staj Başlangıç")] public DateTime? InternshipStart { get; set; }
    [Required(ErrorMessage = Req), DataType(DataType.Date), Display(Name = "Staj Bitiş")] public DateTime? InternshipEnd { get; set; }
    [Required(ErrorMessage = Req), Range(1, 365, ErrorMessage = "Geçerli bir gün sayısı girin."), Display(Name = "Staj Gün Sayısı")] public int? WorkDaysCount { get; set; }

    // İşyeri
    [Required(ErrorMessage = Req), Display(Name = "Firma Adı")] public string CompanyName { get; set; } = "";
    [Required(ErrorMessage = Req), Display(Name = "Firma Adresi")] public string CompanyAddress { get; set; } = "";
    [Required(ErrorMessage = Req), Display(Name = "Firma Telefonu")] public string CompanyPhone { get; set; } = "";
    [Display(Name = "Firma Faks (opsiyonel)")] public string? CompanyFax { get; set; }
    [Required(ErrorMessage = Req), EmailAddress(ErrorMessage = "Geçerli bir e-posta girin."), Display(Name = "Firma E-posta")] public string CompanyEmail { get; set; } = "";
    [Required(ErrorMessage = Req), Range(0, 10000, ErrorMessage = "Geçerli bir sayı girin."), Display(Name = "Mühendis Sayısı")] public int? EngineerCount { get; set; }

    public Ek16Data ToEntity() => new()
    {
        FullName = FullName,
        NationalId = NationalId,
        Nationality = Nationality,
        StudentNo = StudentNo,
        BirthDate = BirthDate,
        Department = Department,
        Email = Email,
        Phone = Phone,
        Address = Address,
        AcademicTerm = AcademicTerm,
        PreviousInternshipDays = PreviousInternshipDays,
        HealthStatus = HealthStatus,
        InternshipStart = InternshipStart,
        InternshipEnd = InternshipEnd,
        WorkDaysCount = WorkDaysCount,
        CompanyName = CompanyName,
        CompanyAddress = CompanyAddress,
        CompanyPhone = CompanyPhone,
        CompanyFax = CompanyFax,
        CompanyEmail = CompanyEmail,
        EngineerCount = EngineerCount
    };

    public static Ek16FormViewModel FromEntity(Ek16Data e) => new()
    {
        FullName = e.FullName ?? "",
        NationalId = e.NationalId ?? "",
        Nationality = e.Nationality ?? "",
        StudentNo = e.StudentNo ?? "",
        BirthDate = e.BirthDate,
        Department = e.Department ?? "",
        Email = e.Email ?? "",
        Phone = e.Phone ?? "",
        Address = e.Address ?? "",
        AcademicTerm = e.AcademicTerm ?? "",
        PreviousInternshipDays = e.PreviousInternshipDays,
        HealthStatus = e.HealthStatus ?? "",
        InternshipStart = e.InternshipStart,
        InternshipEnd = e.InternshipEnd,
        WorkDaysCount = e.WorkDaysCount,
        CompanyName = e.CompanyName ?? "",
        CompanyAddress = e.CompanyAddress ?? "",
        CompanyPhone = e.CompanyPhone ?? "",
        CompanyFax = e.CompanyFax,
        CompanyEmail = e.CompanyEmail ?? "",
        EngineerCount = e.EngineerCount
    };
}

public class DocumentDetailsViewModel
{
    public Document Document { get; set; } = null!;
    public Ek16FormViewModel Form { get; set; } = new();
    public Ek16Report? Report { get; set; }
    public bool CanEdit => Document.Status is not (DocumentStatus.Onaylandi or DocumentStatus.Tamamlandi);
}