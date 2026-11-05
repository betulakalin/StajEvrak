namespace StajEvrak.Core.Entities;

public class Document
{
    // Dokümandaki statü akışı: hangi statüden hangisine geçilebilir
    private static readonly Dictionary<DocumentStatus, DocumentStatus[]> AllowedTransitions = new()
    {
        [DocumentStatus.Yuklendi] = new[] { DocumentStatus.Inceleniyor },
        [DocumentStatus.Inceleniyor] = new[] { DocumentStatus.HataliEksik, DocumentStatus.Onaylandi },
        [DocumentStatus.HataliEksik] = new[] { DocumentStatus.Yuklendi },
        [DocumentStatus.Onaylandi] = new[] { DocumentStatus.Tamamlandi },
        [DocumentStatus.Tamamlandi] = Array.Empty<DocumentStatus>()
    };

    public int Id { get; private set; }
    public int StudentId { get; private set; }
    public Student Student { get; private set; } = null!;
    public string OriginalFileName { get; private set; } = null!;
    public string StoredFileName { get; private set; } = null!;
    public string Extension { get; private set; } = null!;
    public DocumentStatus Status { get; private set; } = DocumentStatus.Yuklendi;
    public DateTime UploadedAt { get; private set; } = DateTime.Now;

    // Tamamlandı belgeyi öğrenci değiştiremez/silemez
    public bool IsCompleted => Status == DocumentStatus.Tamamlandi;

    private Document() { }
    public Document(int studentId, string originalName, string storedName, string ext)
    {
        StudentId = studentId;
        OriginalFileName = originalName;
        StoredFileName = storedName;
        Extension = ext;
    }

    public void ChangeStatus(DocumentStatus newStatus)
    {
        if (!AllowedTransitions[Status].Contains(newStatus))
            throw new InvalidOperationException($"{Status} → {newStatus} geçişi yapılamaz.");
        Status = newStatus;
    }
}