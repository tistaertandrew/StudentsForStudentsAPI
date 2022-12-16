namespace StudentsForStudentsAPI.Models.Mails;

public class UpdateAccountMail : DefaultMail
{
    public UpdateAccountMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle($"Bonjour {Values[0]},", $"\n\nVotre compte a été {(bool.Parse(Values[1]) ? "bloqué" : "débloqué")} par un administrateur.");
}