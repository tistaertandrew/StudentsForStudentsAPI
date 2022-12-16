namespace StudentsForStudentsAPI.Models.Mails;

public class ContactToUserMail : DefaultMail
{
    public ContactToUserMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody() 
        => GetMailStyle("Bonjour,", $"\n\nVotre prise de contact a bien été prise en compte. Nous vous répondrons dans les plus brefs délais.");
}