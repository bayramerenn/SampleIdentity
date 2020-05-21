using Microsoft.Extensions.Configuration;
using SampleIdentity.UI.Entities;
using SampleIdentity.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SampleIdentity.UI.Helper
{
    public class PasswordReset: IPasswordReset
    {
        private readonly IConfiguration _configuration;
        public PasswordReset(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        public PasswordReset()
        {

        }

        public bool SendEmail(string email, string link)
        {
            try
            {
                //MailServer mailServer = _configuration.GetSection("MailServer").Get<MailServer>();


                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(MailServer.Email, "Civil Giyim");

                mail.To.Add(email);//giden kullanıcı

                mail.Subject = "www.personelshift::Mail Doğrulama";
                mail.Body = "<h2>Mail adresinizi doğrulamak için aşağıdaki linke tıklayınız</h2><hr/>";
                mail.Body += $"<a href = '{link}'>mail doğrulama linki</a>";
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Port = MailServer.Port;
                smtp.Host = MailServer.Host;
                smtp.EnableSsl = true;

                smtp.Credentials = new NetworkCredential(MailServer.Email, MailServer.Password);

                smtp.Send(mail);

                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }

        public bool PasswordResetSendEmail(string email, string link)
        {
            try
            {
                //MailServer mailServer = _configuration.GetSection("MailServer").Get<MailServer>();

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(MailServer.Email, "Civil Giyim");

                mail.To.Add(email);//giden kullanıcı

                mail.Subject = "www.personelshift::şifre sıfırlama";
                mail.Body = "<h2>Şifrenizi yenilemek için lütfen aşağıdaki linke tıklayınız</h2><hr/>";
                mail.Body += $"<a href = '{link}'>şifre yenileme linki</a>";
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Port = MailServer.Port;
                smtp.Host = MailServer.Host;
                smtp.EnableSsl = true;

                smtp.Credentials = new NetworkCredential(MailServer.Email, MailServer.Password);

                smtp.Send(mail);

                return true;
            }
            catch (Exception)
            {

                return false;
            }
     
        }
    }
}
