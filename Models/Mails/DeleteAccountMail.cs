namespace StudentsForStudentsAPI.Models.Mails;

public class DeleteAccountMail : DefaultMail
{
    public DeleteAccountMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody() 
        => GetMailStyle($"Bonjour {Values[0]},", $"\n\nVotre compte a été supprimé par un administrateur.");
}