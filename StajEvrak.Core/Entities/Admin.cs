namespace StajEvrak.Core.Entities;

public class Admin : User
{
    public override UserRole Role => UserRole.Admin;

    private Admin() { }
    public Admin(string username, string email, string passwordHash)
        : base(username, email, passwordHash) { }
}