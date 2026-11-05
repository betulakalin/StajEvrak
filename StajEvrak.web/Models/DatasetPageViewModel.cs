using StajEvrak.Core.Entities;
using StajEvrak.Infrastructure.Services;

namespace StajEvrak.web.Models;

public class DatasetPageViewModel
{
    public List<StudentRecord> Records { get; set; } = new();
    public ImportResult? Result { get; set; }
}