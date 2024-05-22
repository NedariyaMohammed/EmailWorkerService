using EmailWorkerService;


using EmailWorkerService.Model;
using EmailWorkerService.Service;
using Microsoft.AspNetCore.Hosting;
using NLog.Extensions.Logging;




class program
{
    static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    //Common Config path 
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                string configPath = Environment.GetEnvironmentVariable("COMMON_CONFIG", EnvironmentVariableTarget.Machine);
                config.AddJsonFile(configPath + "/common-config.json", optional: false, reloadOnChange: true);
                config.AddJsonFile(configPath + "/nlog-config.json");
                

            }) //NLog Config
           .ConfigureLogging((hostContext, logBuilder) =>
            {
                logBuilder.ClearProviders();
                logBuilder.AddNLog(new NLogLoggingConfiguration(hostContext.Configuration.GetSection("NLog")));
            })
        //Services Config
        .ConfigureServices((hostContext, services) =>
            {

                services.AddTransient<IEmailService,EmailService>();
                services.AddHostedService<Worker>();
                services.AddTransient<EmailBL>();
               
                
            });
}




















