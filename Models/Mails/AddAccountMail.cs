namespace StudentsForStudentsAPI.Models.Mails;

public class AddAccountMail : DefaultMail
{
    public AddAccountMail(string subject, string? to, string? from, string[] values) :
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle($"Bonjour {Values[0]}", "\n\nNous vous souhaitons la bienvenue sur notre plateforme Students for Students !");
}