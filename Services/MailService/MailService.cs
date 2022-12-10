﻿using MailKit.Net.Smtp;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using StudentsForStudentsAPI.Models;
using System.Net;

namespace StudentsForStudentsAPI.Services.MailService
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendMail(string subject, string[] values, string type, string? to = null, string? from = null)
        {
            var mail = new MimeMessage();
            mail.From.Add(MailboxAddress.Parse(from ?? _config["MailSettings:ServerFrom"]));
            mail.To.Add(MailboxAddress.Parse(to ?? _config["MailSettings:ServerAdmin"]));
            mail.Subject = subject;
            mail.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = GetMailBody(type, values) };

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

        private static string GetMailBody(string type, string[] values)
        {
            string? body = null;

            switch (type)
            {
                case "AddAccount":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nNous vous souhaitons la bienvenue sur notre application Students for Students !");
                    break;
                case "DeleteAccount":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nVotre compte a été supprimé par un administrateur.");
                    break;
                case "EditAccount":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nVotre compte a été modifié par un administrateur.");
                    break;
                case "UpdateAccount":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nVotre compte a été {(bool.Parse(values[1]) ? "bloqué" : "débloqué")} par un administrateur.");
                    break;
                case "ContactToUser":
                    body = GetMailStyle("Bonjour,", $"\n\nVotre prise de contact a bien été prise en compte. Nous vous répondrons dans les plus brefs délais.");
                    break;
                case "ContactToAdmin":
                    body = GetMailStyle("Prise de contact d'un membre de l'application :", values[0]);
                    break;
                case "AddSynthese":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nVotre synthèse \"{values[1]}\" a été ajoutée avec succès. Cette dernière peut être consultée depuis la section \"Synthèses\" de l'application.");
                    break;
                case "DeleteSynthese":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nVotre synthèse \"{values[1]}\" a été supprimée avec succès.");
                    break;
                case "DeleteRequest":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nVotre demande \"{values[1]}\" a bien été supprimée.");
                    break;
                case "AddRequest":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nVotre demande \"{values[1]}\" a bien été créée. N'hésitez pas à vous rendre dans la section \"Mes demandes\" pour la consulter.");
                    break;
                case "UpdateSenderRequest":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nVotre demande \"{values[1]}\" a été acceptée par {values[2]}. N'hésitez pas à vous rendre dans la section \"Mes demandes\" pour la consulter.");
                    break;
                case "UpdateHandlerRequest":
                    body = GetMailStyle($"Bonjour {values[0]},", $"\n\nVous avez accepté la demande \"{values[1]}\" de {values[2]}. N'hésitez pas à vous rendre dans la section \"Mes demandes\" pour la consulter.");
                    break;
            }

            return body;
        }

        private static string GetMailStyle(string header, string message)
        {
            return $"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" style=\"width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;padding:0;Margin:0\">\r\n <head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta content=\"width=device-width, initial-scale=1\" name=\"viewport\">\r\n  <meta name=\"x-apple-disable-message-reformatting\">\r\n  <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\r\n  <meta content=\"telephone=no\" name=\"format-detection\">\r\n  <title>Nouveau modC3A8le de courrier C3A9lectronique 2022-12-08</title><!--[if (mso 16)]>\r\n    <style type=\"text/css\">\r\n    a {{text-decoration: none;}}\r\n    </style>\r\n    <![endif]--><!--[if gte mso 9]><style>sup {{ font-size: 100% !important; }}</style><![endif]--><!--[if gte mso 9]>\r\n<xml>\r\n    <o:OfficeDocumentSettings>\r\n    <o:AllowPNG></o:AllowPNG>\r\n    <o:PixelsPerInch>96</o:PixelsPerInch>\r\n    </o:OfficeDocumentSettings>\r\n</xml>\r\n<![endif]--><!--[if !mso]><!-- -->\r\n  <link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,400i,700,700i\" rel=\"stylesheet\"><!--<![endif]-->\r\n  <style type=\"text/css\">\r\n#outlook a {{\r\n\tpadding:0;\r\n}}\r\n.ExternalClass {{\r\n\twidth:100%;\r\n}}\r\n.ExternalClass,\r\n.ExternalClass p,\r\n.ExternalClass span,\r\n.ExternalClass font,\r\n.ExternalClass td,\r\n.ExternalClass div {{\r\n\tline-height:100%;\r\n}}\r\n.es-button {{\r\n\tmso-style-priority:100!important;\r\n\ttext-decoration:none!important;\r\n}}\r\na[x-apple-data-detectors] {{\r\n\tcolor:inherit!important;\r\n\ttext-decoration:none!important;\r\n\tfont-size:inherit!important;\r\n\tfont-family:inherit!important;\r\n\tfont-weight:inherit!important;\r\n\tline-height:inherit!important;\r\n}}\r\n.es-desk-hidden {{\r\n\tdisplay:none;\r\n\tfloat:left;\r\n\toverflow:hidden;\r\n\twidth:0;\r\n\tmax-height:0;\r\n\tline-height:0;\r\n\tmso-hide:all;\r\n}}\r\n[data-ogsb] .es-button {{\r\n\tborder-width:0!important;\r\n\tpadding:15px 30px 15px 30px!important;\r\n}}\r\n@media only screen and (max-width:600px) {{p, ul li, ol li, a {{ line-height:150%!important }} h1, h2, h3, h1 a, h2 a, h3 a {{ line-height:120%!important }} h1 {{ font-size:32px!important; text-align:left }} h2 {{ font-size:26px!important; text-align:left }} h3 {{ font-size:20px!important; text-align:left }} h1 a {{ text-align:left }} .es-header-body h1 a, .es-content-body h1 a, .es-footer-body h1 a {{ font-size:36px!important }} h2 a {{ text-align:left }} .es-header-body h2 a, .es-content-body h2 a, .es-footer-body h2 a {{ font-size:30px!important }} h3 a {{ text-align:left }} .es-header-body h3 a, .es-content-body h3 a, .es-footer-body h3 a {{ font-size:18px!important }} .es-menu td a {{ font-size:16px!important }} .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a {{ font-size:16px!important }} .es-content-body p, .es-content-body ul li, .es-content-body ol li, .es-content-body a {{ font-size:16px!important }} .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a {{ font-size:16px!important }} .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a {{ font-size:12px!important }} *[class=\"gmail-fix\"] {{ display:none!important }} .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 {{ text-align:center!important }} .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 {{ text-align:right!important }} .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 {{ text-align:left!important }} .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img {{ display:inline!important }} .es-button-border {{ display:inline-block!important }} a.es-button, button.es-button {{ font-size:16px!important; display:inline-block!important; border-width:15px 30px 15px 30px!important }} .es-btn-fw {{ border-width:10px 0px!important; text-align:center!important }} .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right {{ width:100%!important }} .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header {{ width:100%!important; max-width:600px!important }} .es-adapt-td {{ display:block!important; width:100%!important }} .adapt-img {{ width:100%!important; height:auto!important }} .es-m-p0 {{ padding:0px!important }} .es-m-p0r {{ padding-right:0px!important }} .es-m-p0l {{ padding-left:0px!important }} .es-m-p0t {{ padding-top:0px!important }} .es-m-p0b {{ padding-bottom:0!important }} .es-m-p20b {{ padding-bottom:20px!important }} .es-mobile-hidden, .es-hidden {{ display:none!important }} tr.es-desk-hidden, td.es-desk-hidden, table.es-desk-hidden {{ width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important }} tr.es-desk-hidden {{ display:table-row!important }} table.es-desk-hidden {{ display:table!important }} td.es-desk-menu-hidden {{ display:table-cell!important }} .es-menu td {{ width:1%!important }} table.es-table-not-adapt, .esd-block-html table {{ width:auto!important }} table.es-social {{ display:inline-block!important }} table.es-social td {{ display:inline-block!important }} .es-desk-hidden {{ display:table-row!important; width:auto!important; overflow:visible!important; max-height:inherit!important }} }}\r\n</style>\r\n </head>\r\n <body style=\"width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;padding:0;Margin:0\">\r\n  <div class=\"es-wrapper-color\" style=\"background-color:#EEEEEE\"><!--[if gte mso 9]>\r\n\t\t\t<v:background xmlns:v=\"urn:schemas-microsoft-com:vml\" fill=\"t\">\r\n\t\t\t\t<v:fill type=\"tile\" color=\"#eeeeee\"></v:fill>\r\n\t\t\t</v:background>\r\n\t\t<![endif]-->\r\n   <table class=\"es-wrapper\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#EEEEEE\">\r\n     <tr style=\"border-collapse:collapse\">\r\n      <td valign=\"top\" style=\"padding:0;Margin:0\">\r\n       <table class=\"es-content\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%\">\r\n         <tr style=\"border-collapse:collapse\"></tr>\r\n         <tr style=\"border-collapse:collapse\">\r\n          <td align=\"center\" style=\"padding:0;Margin:0\">\r\n           <table class=\"es-header-body\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#044767;width:600px\" cellspacing=\"0\" cellpadding=\"0\" bgcolor=\"#044767\" align=\"center\">\r\n             <tr style=\"border-collapse:collapse\">\r\n              <td align=\"left\" bgcolor=\"#5D7052\" style=\"Margin:0;padding-top:35px;padding-left:35px;padding-right:35px;padding-bottom:40px;background-color:#5d7052\">\r\n               <table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n                 <tr style=\"border-collapse:collapse\">\r\n                  <td valign=\"top\" align=\"center\" style=\"padding:0;Margin:0;width:530px\">\r\n                   <table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n                     <tr style=\"border-collapse:collapse\">\r\n                      <td class=\"es-m-txt-c\" align=\"center\" style=\"padding:0;Margin:0\"><h1 style=\"Margin:0;line-height:36px;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;font-size:36px;font-style:normal;font-weight:bold;color:#ffffff\">Students for Students</h1></td>\r\n                     </tr>\r\n                   </table></td>\r\n                 </tr>\r\n               </table></td>\r\n             </tr>\r\n           </table></td>\r\n         </tr>\r\n       </table>\r\n       <table class=\"es-content\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%\">\r\n         <tr style=\"border-collapse:collapse\">\r\n          <td align=\"center\" style=\"padding:0;Margin:0\">\r\n           <table class=\"es-content-body\" cellspacing=\"0\" cellpadding=\"0\" bgcolor=\"#ffffff\" align=\"center\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:#FFFFFF;width:600px\">\r\n             <tr style=\"border-collapse:collapse\">\r\n              <td align=\"left\" bgcolor=\"#ffffff\" style=\"Margin:0;padding-left:20px;padding-right:20px;padding-top:40px;padding-bottom:40px;background-color:#ffffff\">\r\n               <table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n                 <tr style=\"border-collapse:collapse\">\r\n                  <td valign=\"top\" align=\"center\" style=\"padding:0;Margin:0;width:560px\">\r\n                   <table width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" role=\"presentation\" style=\"mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px\">\r\n                     <tr style=\"border-collapse:collapse\">\r\n                      <td align=\"left\" style=\"padding:0;Margin:0;padding-bottom:5px;padding-left:40px\"><h3 style=\"Margin:0;line-height:22px;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;font-size:18px;font-style:normal;font-weight:bold;color:#333333\">{header}</h3></td>\r\n                     </tr>\r\n                     <tr style=\"border-collapse:collapse\">\r\n                      <td align=\"left\" style=\"padding:0;Margin:0;padding-left:40px\"><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;line-height:24px;color:#777777;font-size:16px\">{message}</p></td>\r\n                     </tr>\r\n                     <tr style=\"border-collapse:collapse\">\r\n                      <td align=\"left\" style=\"padding:0;Margin:0;padding-top:40px;padding-left:40px\"><p style=\"Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;line-height:24px;color:#777777;font-size:16px\">Cordialement,</p></td>\r\n                     </tr>\r\n                     <tr style=\"border-collapse:collapse\">\r\n                      <td align=\"left\" style=\"padding:0;Margin:0;padding-left:40px\"><h3 style=\"Margin:0;line-height:22px;mso-line-height-rule:exactly;font-family:'open sans', 'helvetica neue', helvetica, arial, sans-serif;font-size:18px;font-style:normal;font-weight:bold;color:#333333\">Students for Students.</h3></td>\r\n                     </tr>\r\n                   </table></td>\r\n                 </tr>\r\n               </table></td>\r\n             </tr>\r\n           </table></td>\r\n         </tr>\r\n       </table></td>\r\n     </tr>\r\n   </table>\r\n  </div>\r\n </body>\r\n</html>";
        }
    }
}
