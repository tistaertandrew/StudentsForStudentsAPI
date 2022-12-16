namespace StudentsForStudentsAPI.Models.Mails;

public class EditAccountMail : DefaultMail
{
    public EditAccountMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle($"Bonjour {Values[0]},", $"\n\nVotre compte a été modifié par un administrateur.");
}