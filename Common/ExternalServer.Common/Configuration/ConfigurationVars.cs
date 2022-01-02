namespace ExternalServer.Common.Configuration {
    public static class ConfigurationVars {
        public static string APPLICATIONSETTINGS_FILENAME = "applicationSettings_fileName";

        public static string CERTIFICATE_FILEPATH = "certificate_filePath";

        /// <summary>
        /// Delete closed connections interval in hours.
        /// </summary>
        public static string CONNECTORJOB_CLEANUPINTERVAL_H = "connectorJob_CleanupInterval_h";

        // GardeningSystem.RestAPI
        // Authentication
        public static string JWT_ISSUER = "rest_api_jwt:issuer";

        // Services
        public static string CONNECTORJOB_ENABLED = "connectorJob_enabled";

        // Client Relay
        public static string MOBILEAPPRELAYSERVICE_PORT = "mobileAppRelayService_port";
        public static string BASESTATIONCONNECTIONSERVICE_PORT = "basetationConnectionService_port";
        public static string BASESTATIONRELAYTUNNEL_PORT = "basestationRelayTunnel_port";

        // Test Environment
        public static string IS_TEST_ENVIRONMENT = "isTestEnvironment";
    }
}
