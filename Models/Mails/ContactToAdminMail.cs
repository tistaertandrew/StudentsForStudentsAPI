namespace StudentsForStudentsAPI.Models.Mails;

public class ContactToAdminMail : DefaultMail
{
    public ContactToAdminMail(string subject, string? to, string? from, string[] values) : 
        base(subject, to, from, values) { }

    public override string GetMailBody()
        => GetMailStyle("Prise de contact d'un membre de l'application :", Values[0]);
}