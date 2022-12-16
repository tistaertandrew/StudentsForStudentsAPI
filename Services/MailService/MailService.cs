using MailKit.Net.Smtp;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using StudentsForStudentsAPI.Models;
using System.Net;
using StudentsForStudentsAPI.Models.Mails;

namespace StudentsForStudentsAPI.Services.MailService
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendMail(DefaultMail defaultMail)
        {
            var mail = new MimeMessage();
            mail.From.Add(MailboxAddress.Parse(defaultMail.From ?? _config["MailSettings:ServerFrom"]));
            mail.To.Add(MailboxAddress.Parse(defaultMail.To ?? _config["MailSettings:ServerAdmin"]));
            mail.Subject = defaultMail.Subject;
            mail.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = defaultMail.GetMailBody() };

            new Thread(() =>
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
            }).Start();
        }
    }
}
