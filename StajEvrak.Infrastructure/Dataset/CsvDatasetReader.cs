using System.Text;

namespace StajEvrak.Infrastructure.Dataset;

public class CsvDatasetReader : DatasetReaderBase
{
    public override bool CanRead(string extension) => extension == ".csv";

    protected override IEnumerable<string[]> ReadRows(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string? line;
        while ((line = reader.ReadLine()) != null)
            yield return line.Split(';');   // Türkçe Excel CSV'yi ; ile ayırır
    }
}