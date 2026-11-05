using StajEvrak.Core.Entities;
using StajEvrak.Infrastructure.Services;

namespace StajEvrak.web.Models;

public class AdminDocumentDetailsViewModel
{
    public Document Document { get; set; } = null!;
    public Ek16Data? Manual { get; set; }
    public Ek16Report? Report { get; set; }
}