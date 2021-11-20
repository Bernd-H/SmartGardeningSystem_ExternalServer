using Autofac;
using ExternalServer.BusinessLogic;
using ExternalServer.BusinessLogic.Managers;
using ExternalServer.Common.Configuration;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using ExternalServer.Common.Specifications.Managers;
using ExternalServer.DataAccess.Repositories;
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

            /// business logic
            // managers
            containerBuilder.RegisterType<SettingsManager>().As<ISettingsManager>();
            containerBuilder.RegisterType<CertificateManager>().As<ICertificateManager>();

            // cryptography

            /// data access
            // repositories
            containerBuilder.RegisterType<CertificateRepository>().As<ICertificateRepository>().SingleInstance();

            // communication
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
