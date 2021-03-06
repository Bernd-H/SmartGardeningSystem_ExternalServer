using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ExternalServer.Common.Models.Entities;
using ExternalServer.Common.Specifications;
using ExternalServer.Common.Specifications.DataAccess.Repositories;
using ExternalServer.Common.Specifications.DataObjects;
using NLog;

namespace ExternalServer.DataAccess.Repositories {

    /// <inheritdoc/>
    public class CertificateRepository : ICertificateRepository {

        private IDictionary<string, ICachedObject> cachedCertificates;


        private ILogger Logger;

        public CertificateRepository(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<CertificateRepository>();
            cachedCertificates = new Dictionary<string, ICachedObject>();
        }

        /// <inheritdoc/>
        public X509Certificate2 GetCertificate(string filePath) {
            lock (cachedCertificates) {
                if (cachedCertificates.ContainsKey(filePath)) {
                    // check lifespan
                    if (cachedCertificates[filePath].Lifetime.TotalDays < 5) {
                        return cachedCertificates[filePath].Object as X509Certificate2;
                    }
                    else {
                        // delete cached object and load cert from store
                        cachedCertificates.Remove(filePath);
                        return GetCertificate(filePath);
                    }
                }
                else {
                    Logger.Info($"[GetCertificate]Loading certificate from {filePath}.");
                    var cert = getCertificate(filePath);
                    if (cert != null) {
                        //lock (cachedCertificates) {
                        cachedCertificates.Add(filePath, new CachedObject(cert));
                        //}
                    }

                    return cert;
                }
            }
        }

        /// <summary>
        /// Gets certificate with specified certThumbprint from the specified StoreLocation
        /// </summary>
        private static X509Certificate2 getCertificate(string filePath) {
            //try {
                X509Certificate2 cert = new X509Certificate2(filePath, "6IBI7pm2huL4");
                return cert;
            //} catch (Exception) {
            //    return null;
            //}
        }
    }
}
