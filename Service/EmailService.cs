using EmailWorkerService.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;

namespace EmailWorkerService.Service
{
    public class EmailService : IEmailService
    {
        
        private IConfiguration _configuration;
       

        //Constructor 
        public EmailService(IConfiguration configuration)
        { 
            
            this._configuration = configuration;
            
           
        }

        /// <summary>
        ///  It constructs an email message, connects to an SMTP server, authenticates with credentials, sends the email, and disconnects from the server.
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(EMail emaildata)
        {

            Smtp smtpconfig=new Smtp();
            string value = JsonConvert.SerializeObject(_configuration.GetSection("Smtp").Get<Dictionary<string, object>>());

            smtpconfig = JsonConvert.DeserializeObject<Smtp>(value);
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(smtpconfig.ConnectionUsername);
            email.To.Add(MailboxAddress.Parse(emaildata.ToEmailAddress));
            email.Subject = emaildata.Subject;
            var builder = new BodyBuilder();

            builder.HtmlBody = emaildata.BodyHTML;
            email.Body = builder.ToMessageBody();

            EMail smtpmail = new EMail();
            smtpmail.SmtpConfig = smtpconfig;
            
            //Smtp Connection
            using var smtpconnection = new SmtpClient();
            smtpconnection.Connect(smtpmail.SmtpConfig.ConnectionHost, smtpmail.SmtpConfig.ConnectionPort, SecureSocketOptions.StartTls);
            smtpconnection.Authenticate(smtpmail.SmtpConfig.ConnectionUsername, smtpmail.SmtpConfig.ConnectionXWD);
            await smtpconnection.SendAsync(email);
            
            smtpconnection.Disconnect(true);



       
        }
    }
}
