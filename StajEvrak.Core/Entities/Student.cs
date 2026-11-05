namespace StajEvrak.Core.Entities;

public class Student : User
{
    public string StudentNo { get; private set; } = null!;

    public override UserRole Role => UserRole.Student;

    private Student() { }
    public Student(string username, string email, string passwordHash, string studentNo)
        : base(username, email, passwordHash)
    {
        StudentNo = studentNo;
    }
}