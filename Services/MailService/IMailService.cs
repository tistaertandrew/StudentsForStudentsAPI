namespace StudentsForStudentsAPI.Services.MailService
{
    public interface IMailService
    {
        void SendMail(string subject, string[] values, string type, string? to = null, string? from = null);
    }
}
