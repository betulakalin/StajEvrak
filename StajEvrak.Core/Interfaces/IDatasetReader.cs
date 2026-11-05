using StajEvrak.Core.Entities;

namespace StajEvrak.Core.Interfaces;

public class DatasetReadResult
{
    public List<StudentRecord> Records { get; } = new();
    public List<string> Errors { get; } = new();
}

public interface IDatasetReader
{
    bool CanRead(string extension);
    DatasetReadResult Read(Stream stream);
}