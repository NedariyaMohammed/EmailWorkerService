using EmailWorkerService.Model;

namespace EmailWorkerService.Service
{

    /// <summary>
    /// Email service interface 
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(EMail emaildata);
    }
}
