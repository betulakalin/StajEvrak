using Microsoft.EntityFrameworkCore;
using StajEvrak.Core.Entities;
using StajEvrak.Core.Interfaces;
using StajEvrak.Core.Services;
using StajEvrak.Infrastructure.Data;

namespace StajEvrak.Infrastructure.Services;

public class Ek16Report
{
    public List<ComparisonItem> DocumentComparison { get; set; } = new();
    public List<ComparisonItem> DatasetComparison { get; set; } = new();
    public List<string> Messages { get; } = new();   // Hata: belge onaylanmaz
    public List<string> Warnings { get; } = new();   // Bilgi: onayı engellemez

    public bool HasErrors => Messages.Any()
                             || DocumentComparison.Any(i => !i.IsMatch)
                             || DatasetComparison.Any(i => !i.IsMatch);
}

public class Ek16Service
{
    private readonly AppDbContext _db;
    private readonly DocumentService _docs;
    private readonly IEnumerable<IEk16Reader> _readers;

    public Ek16Service(AppDbContext db, DocumentService docs, IEnumerable<IEk16Reader> readers)
    {
        _db = db;
        _docs = docs;
        _readers = readers;
    }

    public Task<Ek16Data?> GetAsync(int documentId, DataSource source) =>
        _db.Ek16Data.FirstOrDefaultAsync(x => x.DocumentId == documentId && x.Source == source);

    public async Task<OperationResult> SaveManualAsync(int studentId, int documentId, Ek16Data input)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.StudentId == studentId);
        if (doc == null)
            return new OperationResult(false, "Belge bulunamadı.");

        if (doc.Status is DocumentStatus.Onaylandi or DocumentStatus.Tamamlandi)
            return new OperationResult(false, "Onaylanmış belgenin bilgileri değiştirilemez.");

        var existing = await GetAsync(documentId, DataSource.Manual);
        if (existing == null)
        {
            input.DocumentId = documentId;
            input.Source = DataSource.Manual;
            _db.Ek16Data.Add(input);
        }
        else
        {
            existing.CopyFrom(input);
        }

        // Statü akışı: (Hatalı → Yüklendi) → İnceleniyor → Hatalı/Eksik veya Onaylandı
        if (doc.Status == DocumentStatus.HataliEksik) doc.ChangeStatus(DocumentStatus.Yuklendi);
        doc.ChangeStatus(DocumentStatus.Inceleniyor);

        var report = await BuildReportAsync(doc, input);
        doc.ChangeStatus(report.HasErrors ? DocumentStatus.HataliEksik : DocumentStatus.Onaylandi);

        await _db.SaveChangesAsync();
        return new OperationResult(true);
    }

    public async Task<Ek16Report> BuildReportAsync(Document doc, Ek16Data manual)
    {
        var report = new Ek16Report();

        // 1) Belgeden okunanlar ile form
        var (extracted, error) = await GetOrExtractAsync(doc);
        if (extracted != null)
            report.DocumentComparison = Ek16Comparer.CompareWithDocument(manual, extracted);
        else if (error != null)
            report.Messages.Add(error);
        else
            report.Warnings.Add($"'{doc.Extension}' türündeki belgeler şu an otomatik okunamıyor. " +
                                "Otomatik karşılaştırma için belgeyi Word (.docx) olarak yükleyin.");

        // 2) Veri seti ile form
        var student = await _db.Students.FindAsync(doc.StudentId);
        var record = student == null
            ? null
            : await _db.StudentRecords.FirstOrDefaultAsync(r => r.StudentNo == student.StudentNo);

        if (record == null)
            report.Messages.Add("Öğrenci numaranız veri setinde bulunamadı. Lütfen yöneticiyle iletişime geçin.");
        else
            report.DatasetComparison = Ek16Comparer.CompareWithRecord(manual, record);

        return report;
    }

    // Belge bir kez okunur ve sonuç veritabanına kaydedilir; sonraki seferlerde oradan alınır
    private async Task<(Ek16Data? Data, string? Error)> GetOrExtractAsync(Document doc)
    {
        var existing = await GetAsync(doc.Id, DataSource.Extracted);
        if (existing != null) return (existing, null);

        // Çok biçimlilik: uzantıya uyan okuyucu çalışma anında seçilir
        var reader = _readers.FirstOrDefault(r => r.CanRead(doc.Extension));
        if (reader == null) return (null, null);

        var (_, path) = await _docs.GetFileAsync(doc.Id);
        if (!File.Exists(path)) return (null, "Belge dosyası sunucuda bulunamadı.");

        try
        {
            Ek16Data data;
            await using (var fs = File.OpenRead(path))
            {
                data = reader.Read(fs);
            }

            data.DocumentId = doc.Id;
            data.Source = DataSource.Extracted;
            _db.Ek16Data.Add(data);
            await _db.SaveChangesAsync();
            return (data, null);
        }
        catch (Exception ex)
        {
            return (null, "Belge okunamadı: " + ex.Message);
        }
    }
}