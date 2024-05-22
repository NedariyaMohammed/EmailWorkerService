using EmailWorkerService.Service;
using Microsoft.Extensions.Configuration;

namespace EmailWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IEmailService emailService;
        private IConfiguration _configuration;
        private IServiceProvider _serviceprovider;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, IEmailService service, IConfiguration configuration,IServiceProvider serviceProvider)
        {
            _logger = logger;
            emailService = service;
            _configuration = configuration;
            _serviceprovider= serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                //calling Email Class
                var EmailBL = _serviceprovider.GetRequiredService<EmailBL>();
                EmailBL.SendMail();
                await Task.Delay(10000, stoppingToken);
                Console.WriteLine("Service is Running");
            }
        }
    }
}