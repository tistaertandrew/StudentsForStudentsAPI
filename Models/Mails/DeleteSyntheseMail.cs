namespace StudentsForStudentsAPI.Models.Mails;

public class DeleteSyntheseMail : DefaultMail
{
    public DeleteSyntheseMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle($"Bonjour {Values[0]},", $"\n\nVotre synthèse \"{Values[1]}\" a été supprimée avec succès.");
}