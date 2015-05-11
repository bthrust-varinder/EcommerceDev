using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace HRMGMTEF.Services.PDFGenerator
{
    public  class MailSystem
    {
        public bool SendMail(string body, string Subject)
        {
            try
            {
                string FromMail = ConfigurationManager.AppSettings["UserName"];
                MailMessage mm = new MailMessage(FromMail, FromMail);
                mm.CC.Add(FromMail);
                mm.Subject = Subject;
                mm.Body = body;
                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = ConfigurationManager.AppSettings["Host"];
                smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                NetworkCredential NetworkCred = new NetworkCredential();
                NetworkCred.UserName = FromMail;
                NetworkCred.Password = ConfigurationManager.AppSettings["Password"];
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                smtp.Send(mm);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
