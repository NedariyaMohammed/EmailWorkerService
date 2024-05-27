using EmailWorkerService.Model;
using EmailWorkerService.Service;
using MessageStreamer.MessagingBrokerAdaptor;
using MessageStreamer.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailWorkerService
{
    public class Worker : BackgroundService
    {
        
        private EmailService _emailServiceconfig;
        private IConfiguration _configuration;
        private IServiceProvider _serviceprovider;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger,EmailService Serviceconfig, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceprovider = serviceProvider;
            _emailServiceconfig = Serviceconfig;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                
                string Topic = "emailsend";
                string MessageBroker = Convert.ToString(_configuration.GetSection("MessageBroker").Value);
                string BootstrapServer = JsonConvert.SerializeObject(_configuration.GetSection("kafka").Get<Dictionary<string, object>>());
                KafkaConfiguration kafkaConfiguration = JsonConvert.DeserializeObject<KafkaConfiguration>(BootstrapServer);

                BrokerAdptorFactory brokerAdptorFactory = _serviceprovider.GetRequiredService<BrokerAdptorFactory>();


                _emailServiceconfig = _serviceprovider.GetRequiredService<EmailService>();
                await Task.Run(async () =>
                {
                    await brokerAdptorFactory.GetBrokerAdptor(MessageBroker).GetConsumer(_emailServiceconfig, Topic, kafkaConfiguration);


                });
                Console.WriteLine("Consumer is Running");


                //calling Email Class
              //  var EmailBL = _serviceprovider.GetRequiredService<EmailBL>();
               // EmailBL.SendMail();
                await Task.Delay(10000, stoppingToken);
                Console.WriteLine("Service is Running");



               
            }
        }
    }
}
