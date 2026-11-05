using Microsoft.EntityFrameworkCore;
using StajEvrak.Core.Entities;
using StajEvrak.Core.Interfaces;
using StajEvrak.Infrastructure.Data;

namespace StajEvrak.Infrastructure.Services;

public class ImportResult
{
    public int Added { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class DatasetService
{
    private readonly AppDbContext _db;
    private readonly IEnumerable<IDatasetReader> _readers;

    // Sisteme kayıtlı bütün okuyucular buraya liste olarak gelir
    public DatasetService(AppDbContext db, IEnumerable<IDatasetReader> readers)
    {
        _db = db;
        _readers = readers;
    }

    public async Task<ImportResult> ImportAsync(Stream stream, string fileName)
    {
        var result = new ImportResult();
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        // Çok biçimlilik: hangi okuyucunun çalışacağı uzantıya göre çalışma anında seçilir
        var reader = _readers.FirstOrDefault(r => r.CanRead(ext));
        if (reader == null)
        {
            result.Errors.Add("Sadece .xlsx ve .csv dosyaları kabul edilir.");
            return result;
        }

        DatasetReadResult read;
        try
        {
            read = reader.Read(stream);
        }
        catch (Exception ex)
        {
            result.Errors.Add("Dosya okunamadı: " + ex.Message);
            return result;
        }

        result.Errors.AddRange(read.Errors);

        foreach (var record in read.Records)
        {
            var existing = await _db.StudentRecords
                .FirstOrDefaultAsync(r => r.StudentNo == record.StudentNo);

            if (existing == null)
            {
                _db.StudentRecords.Add(record);
                result.Added++;
            }
            else
            {
                existing.UpdateFrom(record);
                result.Updated++;
            }
        }

        await _db.SaveChangesAsync();
        return result;
    }

    public Task<List<StudentRecord>> GetAllAsync() =>
        _db.StudentRecords.OrderBy(r => r.StudentNo).ToListAsync();
}