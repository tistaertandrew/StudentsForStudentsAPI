namespace StudentsForStudentsAPI.Models.Mails;

public class AddSyntheseMail : DefaultMail
{
    public AddSyntheseMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle($"Bonjour {Values[0]},", $"\n\nVotre synthèse \"{Values[1]}\" a été ajoutée avec succès. Cette dernière peut être consultée depuis la section \"Synthèses\" de l'application.");
}