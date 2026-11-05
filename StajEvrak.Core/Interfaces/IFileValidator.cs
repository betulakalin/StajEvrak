namespace StajEvrak.Core.Interfaces;

public record FileValidationResult(bool IsValid, string? Error = null);

public interface IFileValidator
{
    FileValidationResult Validate(string fileName, long length, Stream content);
}