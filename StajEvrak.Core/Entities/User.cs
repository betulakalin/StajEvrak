namespace StajEvrak.Core.Entities;

public abstract class User
{
    public int Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public int FailedLoginCount { get; private set; }
    public bool IsLocked { get; private set; }

    public abstract UserRole Role { get; }   // Her alt sınıf kendi rolünü söyler

    protected User() { }                     // EF Core için

    protected User(string username, string email, string passwordHash)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
    }

    // Kilitleme kuralı dışarıdan değil, nesnenin kendi içinden yönetilir (kapsülleme)
    public void RegisterFailedLogin(int maxAttempts = 5)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= maxAttempts)
            IsLocked = true;
    }

    public void ResetFailedLogins() => FailedLoginCount = 0;

    public void Unlock()
    {
        IsLocked = false;
        FailedLoginCount = 0;
    }
}