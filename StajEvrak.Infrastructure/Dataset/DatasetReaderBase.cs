using System.Globalization;
using StajEvrak.Core.Entities;
using StajEvrak.Core.Interfaces;

namespace StajEvrak.Infrastructure.Dataset;

public abstract class DatasetReaderBase : IDatasetReader
{
    protected const int ColumnCount = 12;

    private static readonly string[] DateFormats =
        { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };

    public abstract bool CanRead(string extension);

    // Alt sınıflar sadece dosyadan ham satırları okumayı bilir
    protected abstract IEnumerable<string[]> ReadRows(Stream stream);

    // Ortak algoritma: her satırı doğrula ve StudentRecord'a çevir
    public DatasetReadResult Read(Stream stream)
    {
        var result = new DatasetReadResult();
        var seenStudentNos = new HashSet<string>();
        int rowNo = 1;

        foreach (var raw in ReadRows(stream).Skip(1))   // İlk satır başlık
        {
            rowNo++;

            if (raw.All(string.IsNullOrWhiteSpace)) continue;   // Boş satırı atla

            if (raw.Length < ColumnCount)
            {
                result.Errors.Add($"Satır {rowNo}: {ColumnCount} sütun bekleniyordu, {raw.Length} bulundu.");
                continue;
            }

            var c = raw.Select(x => x?.Trim() ?? "").ToArray();

            var emptyCols = Enumerable.Range(0, ColumnCount)
                .Where(i => string.IsNullOrEmpty(c[i]))
                .Select(i => i + 1)
                .ToList();
            if (emptyCols.Any())
            {
                result.Errors.Add($"Satır {rowNo}: boş hücre var (sütun {string.Join(", ", emptyCols)}).");
                continue;
            }

            if (!DateTime.TryParseExact(c[4], DateFormats, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var birthDate))
            {
                result.Errors.Add($"Satır {rowNo}: doğum tarihi okunamadı ({c[4]}). Format: gg.aa.yyyy");
                continue;
            }

            if (!int.TryParse(c[8], out var prevDays) || prevDays < 0)
            {
                result.Errors.Add($"Satır {rowNo}: önceki staj gün sayısı geçersiz ({c[8]}).");
                continue;
            }

            if (!seenStudentNos.Add(c[3]))
            {
                result.Errors.Add($"Satır {rowNo}: öğrenci no {c[3]} dosyada birden fazla kez geçiyor.");
                continue;
            }

            result.Records.Add(new StudentRecord
            {
                FullName = c[0],
                NationalId = c[1],
                Nationality = c[2],
                StudentNo = c[3],
                BirthDate = birthDate,
                Address = c[5],
                Department = c[6],
                SubmissionYear = c[7],
                PreviousInternshipDays = prevDays,
                Email = c[9],
                Phone = c[10],
                HealthInsuranceStatus = c[11]
            });
        }

        return result;
    }
}