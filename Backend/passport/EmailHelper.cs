using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;

namespace passport
{
    public static class ArnGenerator
    {
        public static string GenerateARN()
        {
            // Example: ARN20251218153045123
            return "ARN" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
    public static class EmailHelper
    {
        public static void SendEmail(string toEmail, string subject, string body)
        {
            var fromEmail = "mdsuhail1502@gmail.com";
            var appPassword = "tqxx rjsj ikas twwl"; // App password

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            var mail = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };
            try
            {

                smtp.Send(mail);
            }
            catch(SmtpException ex)
            {
                throw new Exception("SMTP failed: " + ex.Message);
            }
        }

    }
}