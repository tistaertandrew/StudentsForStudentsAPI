namespace StudentsForStudentsAPI.Models.Mails;

public class UpdateHandlerRequestMail : DefaultMail
{
    public UpdateHandlerRequestMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle($"Bonjour {Values[0]},", $"\n\nVous avez accepté la demande \"{Values[1]}\" de {Values[2]}. N'hésitez pas à vous rendre dans la section \"Mes demandes\" pour la consulter.");
}