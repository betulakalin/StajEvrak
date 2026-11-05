using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajEvrak.Core.Entities;
using StajEvrak.Infrastructure.Data;
using StajEvrak.Infrastructure.Services;
using StajEvrak.web.Models;

namespace StajEvrak.web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly DatasetService _dataset;
    private readonly DocumentService _docs;
    private readonly Ek16Service _ek16;

    public AdminController(AppDbContext db, DatasetService dataset, DocumentService docs, Ek16Service ek16)
    {
        _db = db;
        _dataset = dataset;
        _docs = docs;
        _ek16 = ek16;
    }

    public IActionResult Index() => View();

    // ---------- Öğrenci hesapları ----------
    public async Task<IActionResult> Users()
    {
        var students = await _db.Students.OrderBy(s => s.Username).ToListAsync();
        return View(students);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Unlock();
        await _db.SaveChangesAsync();

        TempData["Message"] = $"{user.Username} hesabının kilidi açıldı.";
        return RedirectToAction(nameof(Users));
    }

    // ---------- Giriş logları ----------
    public async Task<IActionResult> LoginLogs()
    {
        var logs = await _db.LoginLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync();
        return View(logs);
    }

    // ---------- Veri seti ----------
    public async Task<IActionResult> Dataset()
    {
        var vm = new DatasetPageViewModel { Records = await _dataset.GetAllAsync() };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Dataset(IFormFile? file)
    {
        ImportResult result;

        if (file == null || file.Length == 0)
        {
            result = new ImportResult { Errors = { "Lütfen bir dosya seçin." } };
        }
        else
        {
            using var stream = file.OpenReadStream();
            result = await _dataset.ImportAsync(stream, file.FileName);
        }

        var vm = new DatasetPageViewModel
        {
            Records = await _dataset.GetAllAsync(),
            Result = result
        };
        return View(vm);
    }

    // ---------- Belgeler ----------
    public async Task<IActionResult> Documents(DocumentStatus? status)
    {
        ViewBag.CurrentStatus = status;
        return View(await _docs.GetAllAsync(status));
    }

    public async Task<IActionResult> DocumentDetails(int id)
    {
        var doc = await _docs.GetWithStudentAsync(id);
        if (doc == null) return NotFound();

        var vm = new AdminDocumentDetailsViewModel { Document = doc };
        vm.Manual = await _ek16.GetAsync(id, DataSource.Manual);
        if (vm.Manual != null)
            vm.Report = await _ek16.BuildReportAsync(doc, vm.Manual);

        return View(vm);
    }

    public async Task<IActionResult> DownloadDocument(int id)
    {
        var (doc, path) = await _docs.GetFileAsync(id);
        if (doc == null || !System.IO.File.Exists(path)) return NotFound();

        return PhysicalFile(path, DocumentService.GetContentType(doc.Extension), doc.OriginalFileName);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteDocument(int id)
    {
        var result = await _docs.CompleteAsync(id);
        if (result.Success) TempData["Message"] = "Belge Tamamlandı statüsüne geçirildi.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(DocumentDetails), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var result = await _docs.DeleteByAdminAsync(id);
        if (result.Success) TempData["Message"] = "Belge silindi.";
        else TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Documents));
    }
}