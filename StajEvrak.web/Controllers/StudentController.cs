using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajEvrak.Infrastructure.Services;
using StajEvrak.Core.Entities;
using StajEvrak.web.Models;

namespace StajEvrak.web.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly DocumentService _docs;
    private readonly Ek16Service _ek16;

    public StudentController(DocumentService docs, Ek16Service ek16)
    {
        _docs = docs;
        _ek16 = ek16;
    }

    // Giriş yapan öğrencinin Id'si cookie'den okunur
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Upload() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file == null)
        {
            ModelState.AddModelError("", "Lütfen bir dosya seçin.");
            return View();
        }

        using var stream = file.OpenReadStream();
        var result = await _docs.UploadAsync(CurrentUserId, file.FileName, file.Length, stream);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error!);
            return View();
        }

        TempData["Message"] = "Belgeniz başarıyla yüklendi.";
        return RedirectToAction(nameof(Documents));
    }

    public async Task<IActionResult> Documents()
    {
        var docs = await _docs.GetStudentDocumentsAsync(CurrentUserId);
        return View(docs);
    }

    public async Task<IActionResult> Download(int id)
    {
        var (doc, path) = await _docs.GetFileAsync(id);

        if (doc == null || !System.IO.File.Exists(path)) return NotFound();
        if (doc.StudentId != CurrentUserId) return Forbid();   // Başkasının belgesi

        return PhysicalFile(path, DocumentService.GetContentType(doc.Extension), doc.OriginalFileName);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _docs.DeleteByStudentAsync(id, CurrentUserId);

        if (result.Success) TempData["Message"] = "Belge silindi.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Documents));
   
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var vm = await BuildDetailsAsync(id);
        return vm == null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Details(int id, Ek16FormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            var invalidVm = await BuildDetailsAsync(id);
            if (invalidVm == null) return NotFound();
            invalidVm.Form = form;   // Kullanıcının yazdıkları kaybolmasın
            return View(invalidVm);
        }

        var result = await _ek16.SaveManualAsync(CurrentUserId, id, form.ToEntity());
        if (result.Success) TempData["Message"] = "Bilgiler kaydedildi ve kontrol edildi.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<DocumentDetailsViewModel?> BuildDetailsAsync(int id)
    {
        var doc = await _docs.GetForStudentAsync(id, CurrentUserId);
        if (doc == null) return null;

        var vm = new DocumentDetailsViewModel { Document = doc };
        var manual = await _ek16.GetAsync(id, DataSource.Manual);
        if (manual != null)
        {
            vm.Form = Ek16FormViewModel.FromEntity(manual);
            vm.Report = await _ek16.BuildReportAsync(doc, manual);
        }
        return vm;
    }
}