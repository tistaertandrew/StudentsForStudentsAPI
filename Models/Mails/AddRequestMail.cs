namespace StudentsForStudentsAPI.Models.Mails;

public class AddRequestMail : DefaultMail
{
    public AddRequestMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle($"Bonjour {Values[0]},", $"\n\nVotre demande \"{Values[1]}\" a bien été créée. N'hésitez pas à vous rendre dans la section \"Mes demandes\" pour la consulter.");
}