namespace StudentsForStudentsAPI.Services.MailService
{
    public interface IMailService
    {
        void SendMail(string to, string subject, string body, string? from = null);
    }
}
