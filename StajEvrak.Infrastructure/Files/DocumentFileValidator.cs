using StajEvrak.Core.Interfaces;

namespace StajEvrak.Infrastructure.Files;

public class DocumentFileValidator : IFileValidator
{
    public const long MaxSize = 10 * 1024 * 1024;   // 10 MB

    // Her uzantının dosya başında taşıması gereken byte'lar (magic number)
    private static readonly Dictionary<string, byte[]> Signatures = new()
    {
        [".pdf"] = new byte[] { 0x25, 0x50, 0x44, 0x46 },   // %PDF
        [".png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47 },   // .PNG
        [".jpg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".docx"] = new byte[] { 0x50, 0x4B, 0x03, 0x04 },   // PK.. (zip)
        [".doc"] = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }    // Eski Office formatı
    };

    public FileValidationResult Validate(string fileName, long length, Stream content)
    {
        // 1) Boş dosya
        if (length == 0)
            return new(false, "Dosya boş.");

        // 2) Boyut
        if (length > MaxSize)
            return new(false, "Dosya 10 MB'tan büyük olamaz.");

        // 3) Uzantı
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!Signatures.TryGetValue(ext, out var signature))
            return new(false, $"'{ext}' formatı kabul edilmiyor. İzin verilenler: Word (.doc, .docx), PDF, JPEG, PNG.");

        // 4) İçerik uzantıyla uyuşuyor mu?
        var header = new byte[signature.Length];
        int read = content.Read(header, 0, header.Length);
        content.Position = 0;   // Dosyayı kaydederken baştan okuyabilmek için

        if (read < signature.Length || !header.SequenceEqual(signature))
            return new(false, "Dosyanın içeriği uzantısıyla uyuşmuyor. Dosya bozuk olabilir veya uzantısı değiştirilmiş olabilir.");

        return new(true);
    }
}