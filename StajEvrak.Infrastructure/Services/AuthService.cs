using Microsoft.EntityFrameworkCore;
using StajEvrak.Core.Entities;
using StajEvrak.Core.Interfaces;
using StajEvrak.Infrastructure.Data;

namespace StajEvrak.Infrastructure.Services;

public record AuthResult(bool Success, string? Error = null, User? User = null);

public class AuthService
{
    private const int MaxAttempts = 5;
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthService(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password, string studentNo)
    {
        if (await _db.Users.AnyAsync(u => u.Username == username))
            return new AuthResult(false, "Bu kullanıcı adı zaten alınmış.");

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return new AuthResult(false, "Bu e-posta zaten kayıtlı.");

        if (await _db.Students.AnyAsync(s => s.StudentNo == studentNo))
            return new AuthResult(false, "Bu öğrenci numarasıyla zaten hesap açılmış.");

        var student = new Student(username, email, _hasher.Hash(password), studentNo);
        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        return new AuthResult(true, User: student);
    }

    public async Task<AuthResult> LoginAsync(string usernameOrEmail, string password, string? ip)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

        if (user == null)
        {
            await LogAsync(usernameOrEmail, false, ip);
            return new AuthResult(false, "Kullanıcı adı veya şifre hatalı.");
        }

        if (user.IsLocked)
        {
            await LogAsync(usernameOrEmail, false, ip);
            return new AuthResult(false, "Hesabınız 5 hatalı deneme nedeniyle kilitlendi. Yöneticiyle iletişime geçin.");
        }

        if (!_hasher.Verify(password, user.PasswordHash))
        {
            user.RegisterFailedLogin(MaxAttempts);
            await LogAsync(usernameOrEmail, false, ip);

            return user.IsLocked
                ? new AuthResult(false, "5 hatalı deneme yapıldı, hesabınız kilitlendi.")
                : new AuthResult(false, $"Kullanıcı adı veya şifre hatalı. Kalan deneme hakkı: {MaxAttempts - user.FailedLoginCount}");
        }

        user.ResetFailedLogins();
        await LogAsync(usernameOrEmail, true, ip);
        return new AuthResult(true, User: user);
    }

    private async Task LogAsync(string username, bool success, string? ip)
    {
        _db.LoginLogs.Add(new LoginLog { Username = username, IsSuccess = success, IpAddress = ip });
        await _db.SaveChangesAsync();
    }
}