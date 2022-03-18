using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using ExternalServer.Common.Specifications;
using ExternalServer.DataAccess.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

namespace Test.DataAccess
{
    [TestClass]
    public class TestWeatherRepository
    {
        [TestMethod]
        public async Task RequestWeatherData_Hollabrunn() {
            using (var mock = AutoMock.GetLoose((cb) => {
            })) {
                // Arrange
                mock.Mock<ILogger>().Setup(x => x.Info(It.IsAny<string>())).Callback<string>((s) => {
                    Debug.WriteLine("Log catched: " + s);
                });
                mock.Mock<ILoggerService>().Setup(x => x.GetLogger<WeatherRepository>()).Returns(mock.Create<ILogger>());

                var weatherRepo = mock.Create<WeatherRepository>();

                // Act
                var data = await weatherRepo.GetWeatherForecastAndHistory("hollabrunn");

                // Assert
                Assert.IsNotNull(data);
            }
        }
    }
}
