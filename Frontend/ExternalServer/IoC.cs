using Autofac;
using ExternalServer.BusinessLogic;
using ExternalServer.BusinessLogic.Cryptography;
using ExternalServer.BusinessLogic.Managers;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.Cryptography;
using ExternalServer.Common.Specifications.DataAccess.Communication;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using ExternalServer.Common.Specifications.Jobs;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.DataAccess.Communication;
using ExternalServer.DataAccess.Repositories;
using ExternalServer.Jobs;
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

            /// jobs
            containerBuilder.RegisterType<ConnectorJob>().As<IConnectorJob>().SingleInstance();

            /// business logic
            // managers
            containerBuilder.RegisterType<SettingsManager>().As<ISettingsManager>();
            containerBuilder.RegisterType<CertificateManager>().As<ICertificateManager>();
            containerBuilder.RegisterType<ConnectionsManager>().As<IConnectionsManager>().SingleInstance();
            containerBuilder.RegisterType<RelayInitManager>().As<IRelayInitManager>().SingleInstance();
            containerBuilder.RegisterType<RelayManager>().As<IRelayManager>().SingleInstance();

            // cryptography
            containerBuilder.RegisterType<PasswordHasher>().As<IPasswordHasher>();

            /// data access
            // repositories
            containerBuilder.RegisterType<CertificateRepository>().As<ICertificateRepository>().SingleInstance();
            containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().SingleInstance();

            // communication
            containerBuilder.RegisterType<SslListener>().As<ISslListener>();
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
