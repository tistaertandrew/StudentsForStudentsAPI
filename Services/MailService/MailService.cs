using MailKit.Net.Smtp;
using MimeKit;

namespace StudentsForStudentsAPI.Services.MailService
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendMail(string to, string subject, string body, string? from = null)
        {
            var mail = new MimeMessage();
            mail.From.Add(MailboxAddress.Parse(from ?? _config["MailSettings:ServerFrom"]));
            mail.To.Add(MailboxAddress.Parse(to));
            mail.Subject = subject;
            mail.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = body };
            
            new Thread(new ThreadStart(() =>
            {
                var smtp = new SmtpClient();
                try
                {
                    smtp.Connect(_config["MailSettings:ServerName"],
                        int.Parse(_config["MailSettings:ServerPort"]),
                        MailKit.Security.SecureSocketOptions.None);
                    smtp.Send(mail);
                }
                catch (Exception) { }
                finally { smtp.Disconnect(true); }
            })).Start();
        }
    }
}
