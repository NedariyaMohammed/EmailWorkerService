using Confluent.Kafka;
using EmailWorkerService.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using MessageStreamer.IServices;
using MessageStreamer.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using MimeKitContentType = MimeKit.ContentType;

namespace EmailWorkerService.Service
{
    public class EmailService : IEmailService,IRequestID
    {
        private IConfiguration _configuration;
        private string pAttachment = "Attachment/12-Jul-2021-Grooming-Session1.txt";

        

        // Constructor 
        public EmailService(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        void IRequestID.RequestedID(string userInput)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<EMail>(userInput);
                SendEmailAsync(data);
                
            }
            catch (Exception ex) {

            }
            
        }

        Task<bool> IRequestID.ProcessData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs an email message, connects to an SMTP server, authenticates with credentials, sends the email, and disconnects from the server.
        /// </summary>
        /// <param name="emaildata">Email data containing recipient, subject, and body.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(EMail emaildata)
        {
            Smtp smtpconfig = new Smtp();
            string value = JsonConvert.SerializeObject(_configuration.GetSection("Smtp").Get<Dictionary<string, object>>());
            smtpconfig = JsonConvert.DeserializeObject<Smtp>(value);

            string[] emailAddressArray = emaildata.ToEmailAddress.Split(',');

            foreach (string email in emailAddressArray)
            {
                   var mimeMessage = new MimeMessage();
                    mimeMessage.Sender = MailboxAddress.Parse(smtpconfig.ConnectionUsername);
                    mimeMessage.Subject = emaildata.Subject;
                
                    mimeMessage.To.Add(MailboxAddress.Parse(email));

                var builder = new BodyBuilder
                {
                    HtmlBody = emaildata.BodyHTML
                };
                // var email = new MimeMessage();
                // email.Sender = MailboxAddress.Parse(smtpconfig.ConnectionUsername);
                //  email.To.Add(MailboxAddress.Parse(emaildata.ToEmailAddress));
                //  email.Subject = emaildata.Subject;

                /* var builder = new BodyBuilder
                 {
                     HtmlBody = emaildata.BodyHTML
                 };*/

                // Add attachments if specified
                if (!string.IsNullOrEmpty(pAttachment))
                {
                    foreach (string lAttachment in pAttachment.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var fileAttachment = AddFileAttachmentsO365(lAttachment);
                        if (fileAttachment != null)
                        {
                            try
                            {
                                var mimeContentType = ParseMimeType(fileAttachment.ContentType);
                                builder.Attachments.Add(fileAttachment.Name, fileAttachment.ContentBytes, mimeContentType);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to attach file {fileAttachment.Name}: {ex.Message}");
                            }
                        }
                    }
                }

                mimeMessage.Body = builder.ToMessageBody();
            
            EMail smtpmail = new EMail
            {
                SmtpConfig = smtpconfig
            };

            // SMTP Connection
            using var smtpconnection = new SmtpClient();
            smtpconnection.Connect(smtpmail.SmtpConfig.ConnectionHost, smtpmail.SmtpConfig.ConnectionPort, MailKit.Security.SecureSocketOptions.StartTls);
            smtpconnection.Authenticate(smtpmail.SmtpConfig.ConnectionUsername, smtpmail.SmtpConfig.ConnectionXWD);
            await smtpconnection.SendAsync(mimeMessage);
            smtpconnection.Disconnect(true);
         }
        }




        /// <summary>
        /// Reads the specified file and creates a FileAttachment object containing the file's content and metadata.
        /// </summary>
        private static FileAttachment AddFileAttachmentsO365(string lAttachment)
        {
            try
            {
                string contentType = GetContentType(lAttachment);
                byte[] contentBytes = File.ReadAllBytes(lAttachment);
                var requestBody = new FileAttachment
                {
                    OdataType = "#microsoft.graph.fileAttachment",
                    ContentBytes = contentBytes,
                    ContentType = contentType,
                    Name = Path.GetFileName(lAttachment),
                };
                return requestBody;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {lAttachment}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Determines the MIME content type of a file based on its extension.
        /// </summary>
        private static string GetContentType(string lAttachment)
        {
            try
            {
                string contentType;
                switch (Path.GetExtension(lAttachment).ToLower())
                {
                    case ".csv":
                        contentType = "text/csv";
                        break;
                    case ".pdf":
                        contentType = "application/pdf";
                        break;
                    case ".txt":
                        contentType = "text/plain";
                        break;
                    case ".zip":
                        contentType = "application/zip";
                        break;
                    case ".xls":
                        contentType = "application/vnd.ms-excel";
                        break;
                    default:
                        contentType = "application/octet-stream";
                        break;
                }
                return contentType;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error determining content type for {lAttachment}: {ex.Message}");
                return "application/octet-stream";
            }
        }

        /// <summary>
        /// Parses a MIME content type string into a MimeKit.ContentType object.
        /// </summary>
        private static MimeKitContentType ParseMimeType(string contentType)
        {
            var parts = contentType.Split('/');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid content type format");

            return new MimeKitContentType(parts[0], parts[1]);
        }

       
    }
}
