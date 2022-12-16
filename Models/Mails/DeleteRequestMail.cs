namespace StudentsForStudentsAPI.Models.Mails;

public class DeleteRequestMail : DefaultMail
{
    public DeleteRequestMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle($"Bonjour {Values[0]},", $"\n\nVotre demande \"{Values[1]}\" a bien été supprimée.");
}