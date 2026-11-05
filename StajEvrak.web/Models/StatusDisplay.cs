using StajEvrak.Core.Entities;

namespace StajEvrak.web.Models;

public static class StatusDisplay
{
    public static (string Css, string Text) Get(DocumentStatus status) => status switch
    {
        DocumentStatus.Yuklendi => ("bg-secondary", "Yüklendi"),
        DocumentStatus.Inceleniyor => ("bg-info", "İnceleniyor"),
        DocumentStatus.HataliEksik => ("bg-danger", "Hatalı / Eksik"),
        DocumentStatus.Onaylandi => ("bg-primary", "Onaylandı"),
        DocumentStatus.Tamamlandi => ("bg-success", "Tamamlandı"),
        _ => ("bg-light", status.ToString())
    };
}