using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;

namespace StajEvrak.Infrastructure.Readers;

public class PdfEk16Reader : Ek16ReaderBase
{
    // Sayfadaki bir dikdörtgen (PDF'te y ekseni yukarı doğru artar)
    private record Box(double L, double R, double B, double T)
    {
        public double Cx => (L + R) / 2;
        public double Cy => (B + T) / 2;
    }

    private record PdfWord(string Text, Box Box);

    public override bool CanRead(string extension) => extension == ".pdf";

    protected override (Func<string, string?> Student, Func<string, string?> Company) ReadFields(Stream stream)
    {
        using var pdf = PdfDocument.Open(stream);
        var page = pdf.GetPage(1);

        var words = page.GetWords()
            .Select(w => new PdfWord(w.Text, new Box(w.BoundingBox.Left, w.BoundingBox.Right,
                                                     w.BoundingBox.Bottom, w.BoundingBox.Top)))
            .ToList();

        if (words.Count == 0)
            throw new InvalidDataException("PDF'te metin bulunamadı (taranmış belge olabilir). Taranmış belgeler finalde OCR ile okunacak.");

        var (vertical, horizontal) = ReadLines(page);
        double pageWidth = page.Width;

        // Bölüm başlıkları: aynı etiket ("Adı Soyadı" gibi) birden fazla tabloda geçtiği için aramayı bölüme sınırlıyoruz
        var studentHeader = FindLabel(words, "ÖĞRENCİNİN", double.MaxValue, double.MinValue);
        var companyHeader = FindLabel(words, "STAJ YAPILAN YERİN", double.MaxValue, double.MinValue);
        var employerHeader = FindLabel(words, "İŞVEREN VEYA YETKİLİNİN", double.MaxValue, double.MinValue);
        if (studentHeader == null || companyHeader == null || employerHeader == null)
            throw new InvalidDataException("PDF, Ek-16 formatında değil (bölüm başlıkları bulunamadı).");

        Func<string, string?> student = label =>
            ValueOf(words, vertical, horizontal, pageWidth, label, studentHeader.B, companyHeader.T);
        Func<string, string?> company = label =>
            ValueOf(words, vertical, horizontal, pageWidth, label, companyHeader.B, employerHeader.T);

        return (student, company);
    }

    // Tablo çizgilerini dikey ve yatay olarak ayırır
    private static (List<Box> Vertical, List<Box> Horizontal) ReadLines(Page page)
    {
        var segments = new List<Box>();

        foreach (var path in page.Paths)
            foreach (var subpath in path)
            {
                PdfRectangle? r = subpath.GetBoundingRectangle();
                if (r == null) continue;

                var b = new Box(r.Value.Left, r.Value.Right, r.Value.Bottom, r.Value.Top);

                if (b.R - b.L < 2 || b.T - b.B < 2)
                {
                    segments.Add(b);                      // İnce çizgi
                }
                else
                {                                         // Dikdörtgen olarak çizilmiş kenarlık: 4 kenarını al
                    segments.Add(b with { T = b.B });
                    segments.Add(b with { B = b.T });
                    segments.Add(b with { R = b.L });
                    segments.Add(b with { L = b.R });
                }
            }

        var vertical = segments.Where(s => s.R - s.L < 2 && s.T - s.B > 5).ToList();
        var horizontal = segments.Where(s => s.T - s.B < 2 && s.R - s.L > 5).ToList();
        return (vertical, horizontal);
    }

    // Etiketi oluşturan ardışık kelimeleri bulur ve kapladığı alanı döndürür
    private static Box? FindLabel(List<PdfWord> words, string label, double yMax, double yMin)
    {
        var key = Normalize(label);

        for (int i = 0; i < words.Count; i++)
        {
            var top = words[i].Box.T;
            if (top > yMax || top <= yMin) continue;

            var first = Normalize(words[i].Text);
            if (first.Length == 0 || !(key.StartsWith(first) || first.StartsWith(key))) continue;

            var acc = "";
            int j = i;
            while (j < words.Count && acc.Length < key.Length)
                acc += Normalize(words[j++].Text);

            if (acc.StartsWith(key))
            {
                var group = words.GetRange(i, j - i);
                return new Box(group.Min(w => w.Box.L), group.Max(w => w.Box.R),
                               group.Min(w => w.Box.B), group.Max(w => w.Box.T));
            }
        }
        return null;
    }

    // Etiketin hücresini çizgilerden bulur, sağındaki hücrenin içindeki kelimeleri döndürür
    private static string? ValueOf(List<PdfWord> words, List<Box> vertical, List<Box> horizontal,
                                   double pageWidth, string label, double yMax, double yMin)
    {
        var b = FindLabel(words, label, yMax, yMin);
        if (b == null) return null;

        // Etiketin hizasından geçen dikey çizgilerin x'leri, etiketin üstünden geçen yatay çizgilerin y'leri
        var xs = vertical.Where(s => s.B - 1 <= b.Cy && b.Cy <= s.T + 1).Select(s => s.Cx).ToList();
        var ys = horizontal.Where(s => s.L - 1 <= b.Cx && b.Cx <= s.R + 1).Select(s => s.Cy).ToList();

        var above = ys.Where(y => y > b.Cy).ToList();
        var below = ys.Where(y => y < b.Cy).ToList();
        var rightOfLabel = xs.Where(x => x >= b.R - 1).ToList();
        if (above.Count == 0 || below.Count == 0 || rightOfLabel.Count == 0) return null;

        double cellTop = above.Min();
        double cellBottom = below.Max();
        double valueLeft = rightOfLabel.Min();              // Etiket hücresinin sağ kenarı = değer hücresinin sol kenarı
        var further = xs.Where(x => x > valueLeft + 1).ToList();
        double valueRight = further.Count > 0 ? further.Min() : pageWidth;

        var text = string.Join(" ", words
            .Where(w => w.Box.Cx > valueLeft && w.Box.Cx < valueRight &&
                        w.Box.Cy > cellBottom && w.Box.Cy < cellTop)
            .OrderByDescending(w => Math.Round(w.Box.T))    // Yukarıdan aşağı
            .ThenBy(w => w.Box.L)                           // Soldan sağa
            .Select(w => w.Text)).Trim();

        return text.Length > 0 ? text : null;
    }
}