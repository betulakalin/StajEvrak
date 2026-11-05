using StajEvrak.Core.Entities;

namespace StajEvrak.Core.Interfaces;

public interface IEk16Reader
{
    bool CanRead(string extension);
    Ek16Data Read(Stream stream);
}