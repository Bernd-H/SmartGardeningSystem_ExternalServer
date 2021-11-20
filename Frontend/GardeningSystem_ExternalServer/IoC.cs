﻿using Autofac;
using ExternalServer.BusinessLogic;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Specifications;
using Microsoft.Extensions.Configuration;

namespace ExternalServer {
    public static class IoC {

        private static IContainer applicationContext;


        private static ContainerBuilder builder;

        /// <summary>
        /// Registers all types to a internal containerbuilder.
        /// Does not build the cointainer yet.
        /// </summary>
        public static void Init() {
            if (builder == null) {
                builder = new ContainerBuilder();

                RegisterToContainerBuilder(ref builder);
            }
        }

        /// <summary>
        /// Also needed to register all types to an external container builder.
        /// Used in GardeningSystem.RestAPI -> Startup
        /// </summary>
        /// <param name="containerBuilder"></param>
        public static void RegisterToContainerBuilder(ref ContainerBuilder containerBuilder) {
            // Register individual components
            containerBuilder.RegisterType<LoggerService>().As<ILoggerService>();
            containerBuilder.Register(c => ConfigurationContainer.Configuration).As<IConfiguration>();
            //containerBuilder.RegisterType<DevelopmentSetuper>().As<IDevelopmentSetuper>();

            ///// jobs
            //containerBuilder.RegisterType<WateringJob>().AsSelf();
            //containerBuilder.RegisterType<CommunicationJob>().AsSelf();

            ///// business logic
            //// managers
            //containerBuilder.RegisterType<WateringManager>().As<IWateringManager>();
            //containerBuilder.RegisterType<ModuleManager>().As<IModuleManager>();
            //containerBuilder.RegisterType<SettingsManager>().As<ISettingsManager>();
            //containerBuilder.RegisterType<LocalMobileAppDiscoveryManager>().As<ILocalMobileAppDiscoveryManager>();
            //containerBuilder.RegisterType<AesKeyExchangeManager>().As<IAesKeyExchangeManager>();
            //containerBuilder.RegisterType<CommandManager>().As<ICommandManager>();

            //// cryptography
            //containerBuilder.RegisterType<PasswordHasher>().As<IPasswordHasher>();
            //containerBuilder.RegisterType<AesEncrypterDecrypter>().As<IAesEncrypterDecrypter>();
            //containerBuilder.RegisterType<CertificateHandler>().As<ICertificateHandler>();

            ///// data access
            //// repositories
            //containerBuilder.RegisterType<FileRepository>().As<IFileRepository>();
            //containerBuilder.RegisterGeneric(typeof(SerializedFileRepository<>)).As(typeof(ISerializedFileRepository<>)).InstancePerDependency();
            //containerBuilder.RegisterType<ModulesRepository>().As<IModulesRepository>().SingleInstance();
            //containerBuilder.RegisterType<RfCommunicator>().As<IRfCommunicator>().SingleInstance();
            //containerBuilder.RegisterType<WeatherRepository>().As<IWeatherRepository>();
            //containerBuilder.RegisterType<CertificateRepository>().As<ICertificateRepository>().SingleInstance();

            //// communication
            //containerBuilder.RegisterType<WifiConfigurator>().As<IWifiConfigurator>();
            //containerBuilder.RegisterType<LocalMobileAppDiscovery>().As<ILocalMobileAppDiscovery>();
            //containerBuilder.RegisterType<SocketSender>().As<ISocketSender>();
            //var aesKeyExchangePort = Convert.ToInt32(ConfigurationContainer.Configuration[ConfigurationVars.AESKEYEXCHANGE_LISTENPORT]);
            //containerBuilder.RegisterType<SslListener>().As<ISslListener>()
            //    .WithParameter("listenerEndPoint", new IPEndPoint(IPAddress.Any, aesKeyExchangePort));
            //var commandListenerPort = Convert.ToInt32(ConfigurationContainer.Configuration[ConfigurationVars.COMMANDLISTENER_LISTENPORT]);
            //containerBuilder.RegisterType<AesTcpListener>().As<IAesTcpListener>()
            //    .WithParameter("listenerEndPoint", new IPEndPoint(IPAddress.Any, commandListenerPort));
        }

        /// <summary>
        /// Resolves a specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : class {
            if (applicationContext == null) {
                applicationContext = builder.Build();
            }

            return applicationContext.Resolve<T>();
        }

        public static IContainer GetContainer() {
            if (applicationContext == null) {
                applicationContext = builder.Build();
            }

            return applicationContext;
        }


        public static ContainerBuilder GetContainerBuilder() {
            if (applicationContext != null)
                throw new System.Exception("Cannot return a containerbuilder, because a container got already build.");

            return builder;
        }
    }
}
