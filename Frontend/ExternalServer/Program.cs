using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.RestAPI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace ExternalServer {
    public class Program {
        public static void Main(string[] args) {
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            try {
                IoC.Init();
                // get local certificate to use for HTTPS on the rest api
                var cert = IoC.Get<ICertificateManager>().GetCertificate();

                // development setup
                if (Convert.ToBoolean(ConfigurationContainer.Configuration[ConfigurationVars.IS_TEST_ENVIRONMENT])) {
                    logger.Info("Setting up test development/test enviroment.");
                    //IoC.Get<IDevelopmentSetuper>().SetupTestEnvironment();
                }

                CreateHostBuilder(args, logger, cert).Build().Run();
            }
            catch (Exception exception) {
                //NLog: catch setup errors
                logger.Fatal(exception, "[Main]Stopped program because of exception.");
                throw;
            }
            finally {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, NLog.ILogger logger, X509Certificate2 x509Certificate) =>
            Host.CreateDefaultBuilder(args)
                // configure logging
                .ConfigureLogging(config => {
                    config.ClearProviders(); // remove default logging
                    config.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog()
                // configure autofac (dependency injection framework)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder => {
                    //registering services in the Autofac ContainerBuilder
                    IoC.RegisterToContainerBuilder(ref builder);
                })
                // configure services
                .ConfigureWebHostDefaults(webBuilder => { // rest api
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(opts => {
                        // Bind directly to a socket handle or Unix socket
                        opts.ListenAnyIP(5001, opts => {
                            if (x509Certificate != null && x509Certificate.HasPrivateKey) {
                                // Configure Kestrel to use a certificate from a local .PFX file for hosting HTTPS
                                opts.UseHttps(x509Certificate);
                            } else {
                                logger.Warn("[CreateHostBuilder]No local certificate for the rest api.");
                                opts.UseHttps();
                            }

                        });
                        opts.ListenAnyIP(5000);
                    });
                })
                .ConfigureServices((hostContext, services) => {
                    //// timed jobs
                    //if (Convert.ToBoolean(ConfigurationContainer.Configuration[ConfigurationVars.WATERINGJOB_ENABLED]))
                    //{
                    //    services.AddHostedService<WateringJob>();
                    //}
                    //// other services
                    //if (Convert.ToBoolean(ConfigurationContainer.Configuration[ConfigurationVars.COMMUNICATIONJOB_ENABLED]))
                    //{
                    //    services.AddHostedService<CommunicationJob>();
                    //}
                });
    }
}
