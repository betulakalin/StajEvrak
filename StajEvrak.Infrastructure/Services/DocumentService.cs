using Microsoft.EntityFrameworkCore;
using StajEvrak.Core.Entities;
using StajEvrak.Core.Interfaces;
using StajEvrak.Infrastructure.Data;

namespace StajEvrak.Infrastructure.Services;

public record OperationResult(bool Success, string? Error = null);

public class DocumentService
{
    private readonly AppDbContext _db;
    private readonly IFileValidator _validator;
    private readonly string _uploadRoot;

    public DocumentService(AppDbContext db, IFileValidator validator, string uploadRoot)
    {
        _db = db;
        _validator = validator;
        _uploadRoot = uploadRoot;
    }

    public async Task<OperationResult> UploadAsync(int studentId, string fileName, long length, Stream stream)
    {
        var check = _validator.Validate(fileName, length, stream);
        if (!check.IsValid)
            return new OperationResult(false, check.Error);

        // Tamamlanmış belgesi olan öğrenci yeni belge yükleyemez
        if (await _db.Documents.AnyAsync(d => d.StudentId == studentId && d.Status == DocumentStatus.Tamamlandi))
            return new OperationResult(false, "Tamamlandı statüsünde bir belgeniz var, yeni yükleme yapamazsınız.");

        // Dosyayı rastgele bir isimle kaydet (aynı isimli dosyalar çakışmasın, isimden saldırı yapılamasın)
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid()}{ext}";
        Directory.CreateDirectory(_uploadRoot);

        await using (var fs = File.Create(Path.Combine(_uploadRoot, storedName)))
        {
            await stream.CopyToAsync(fs);
        }

        _db.Documents.Add(new Document(studentId, Path.GetFileName(fileName), storedName, ext));
        await _db.SaveChangesAsync();

        return new OperationResult(true);
    }

    public Task<List<Document>> GetStudentDocumentsAsync(int studentId) =>
        _db.Documents
           .Where(d => d.StudentId == studentId)
           .OrderByDescending(d => d.UploadedAt)
           .ToListAsync();
    public Task<Document?> GetForStudentAsync(int documentId, int studentId) =>
    _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.StudentId == studentId);

    public async Task<(Document? Doc, string Path)> GetFileAsync(int documentId)
    {
        var doc = await _db.Documents.FindAsync(documentId);
        return (doc, doc == null ? "" : Path.Combine(_uploadRoot, doc.StoredFileName));
    }

    // ---------- Öğrenci ----------
    public async Task<OperationResult> DeleteByStudentAsync(int documentId, int studentId)
    {
        var doc = await _db.Documents.FindAsync(documentId);

        if (doc == null || doc.StudentId != studentId)
            return new OperationResult(false, "Belge bulunamadı.");

        if (doc.IsCompleted)
            return new OperationResult(false, "Tamamlandı statüsündeki belgeler silinemez.");

        await RemoveAsync(doc);
        return new OperationResult(true);
    }

    // ---------- Admin ----------
    public Task<List<Document>> GetAllAsync(DocumentStatus? status)
    {
        var query = _db.Documents.Include(d => d.Student).AsQueryable();

        if (status != null)
            query = query.Where(d => d.Status == status);

        return query.OrderByDescending(d => d.UploadedAt).ToListAsync();
    }

    public Task<Document?> GetWithStudentAsync(int documentId) =>
        _db.Documents.Include(d => d.Student).FirstOrDefaultAsync(d => d.Id == documentId);

    // Admin, Tamamlandı dahil her belgeyi silebilir (forum 3. soru)
    public async Task<OperationResult> DeleteByAdminAsync(int documentId)
    {
        var doc = await _db.Documents.FindAsync(documentId);
        if (doc == null) return new OperationResult(false, "Belge bulunamadı.");

        await RemoveAsync(doc);
        return new OperationResult(true);
    }

    public async Task<OperationResult> CompleteAsync(int documentId)
    {
        var doc = await _db.Documents.FindAsync(documentId);
        if (doc == null) return new OperationResult(false, "Belge bulunamadı.");

        try
        {
            doc.ChangeStatus(DocumentStatus.Tamamlandi);   // Sadece Onaylandı'dan geçilebilir
        }
        catch (InvalidOperationException)
        {
            return new OperationResult(false, "Sadece Onaylandı statüsündeki belgeler tamamlanabilir.");
        }

        await _db.SaveChangesAsync();
        return new OperationResult(true);
    }

    // ---------- Ortak ----------
    private async Task RemoveAsync(Document doc)
    {
        var path = Path.Combine(_uploadRoot, doc.StoredFileName);
        if (File.Exists(path)) File.Delete(path);

        _db.Documents.Remove(doc);   // Ek16Data kayıtları da otomatik silinir (cascade)
        await _db.SaveChangesAsync();
    }

    public static string GetContentType(string ext) => ext switch
    {
        ".pdf" => "application/pdf",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".doc" => "application/msword",
        _ => "application/octet-stream"
    };
}