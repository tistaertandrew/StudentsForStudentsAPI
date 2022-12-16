namespace StudentsForStudentsAPI.Models.Mails;

public class UpdateSenderRequestMail : DefaultMail
{
    public UpdateSenderRequestMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle($"Bonjour {Values[0]},", $"\n\nVotre demande \"{Values[1]}\" a été acceptée par {Values[2]}. N'hésitez pas à vous rendre dans la section \"Mes demandes\" pour la consulter.");
}