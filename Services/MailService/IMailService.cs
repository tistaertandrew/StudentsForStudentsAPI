namespace StudentsForStudentsAPI.Services.MailService
{
    public interface IMailService
    {
        void SendMail(string subject, string body, string? to = null, string? from = null);
    }
}
