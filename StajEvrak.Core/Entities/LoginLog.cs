namespace StajEvrak.Core.Entities;

public class LoginLog
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsSuccess { get; set; }
    public string? IpAddress { get; set; }
}