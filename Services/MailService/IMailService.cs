using StudentsForStudentsAPI.Models.Mails;

namespace StudentsForStudentsAPI.Services.MailService
{
    public interface IMailService
    {
        void SendMail(DefaultMail defaultMail);
    }
}
