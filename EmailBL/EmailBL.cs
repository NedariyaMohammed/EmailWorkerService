
using EmailWorkerService.Model;
using EmailWorkerService.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace EmailWorkerService
{

    public class EmailBL 
    {
     
          private readonly IEmailService emailService;
          private IConfiguration _configuration;
          private readonly ILogger<EmailService> _logger;
        public EmailBL(IEmailService service, IConfiguration configuration,ILogger<EmailService> logger)
        {
        
            this.emailService = service;
            this._configuration = configuration;
            this._logger = logger;  
        }


        
        public async Task<Boolean> SendMail() 
        {

            try {

                //Email config
                EMail emaildata = new EMail();
                string emailaddress = _configuration["EMail:ToEmailAddress"];
                string subjectdata = _configuration["EMail:Subject"];
                string bodydata = _configuration["EMail:BodyHTML"];

                emaildata.ToEmailAddress = emailaddress;
                emaildata.Subject = subjectdata;
                emaildata.BodyHTML = bodydata;                

                //sending mail 
                 await emailService.SendEmailAsync(emaildata);
                _logger.LogInformation("emaildata");
                 return true;
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
                
            }

        }
    }
}
