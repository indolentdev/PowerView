using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using PowerView.Model;
using PowerView.Service.Dtos;
using PowerView.Service.Mappers;

namespace PowerView.Service.Test.Mappers
{
    [TestFixture]
    public class LiveReadingMapperTest
    {
        private const string ApplicationJson = "application/json";
        private const string ApplicationFormUrlEncoded = "application/x-www-form-urlencoded";

        [Test]
        public void MapPvOutputArgsThrows()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string contentType = "text/plain";
            var body = new MemoryStream();
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            const string p23l1Param = "v7";
            const string p23l2Param = "v8";
            const string p23l3Param = "v9";
            var target = CreateTarget();

            // Act & Assert
            Assert.That(() => target.MapPvOutputArgs(null, contentType, body, deviceLabel, sn, snParam, p23l1Param, p23l2Param, p23l3Param), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.MapPvOutputArgs(uri, string.Empty, body, deviceLabel, sn, snParam, p23l1Param, p23l2Param, p23l3Param), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.MapPvOutputArgs(uri, null, body, deviceLabel, sn, snParam, p23l1Param, p23l2Param, p23l3Param), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.MapPvOutputArgs(uri, contentType, null, deviceLabel, sn, snParam, p23l1Param, p23l2Param, p23l3Param), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.MapPvOutputArgs(uri, contentType, body, string.Empty, sn, snParam, p23l1Param, p23l2Param, p23l3Param), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.MapPvOutputArgs(uri, contentType, body, null, sn, snParam, p23l1Param, p23l2Param, p23l3Param), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.MapPvOutputArgs(uri, contentType, body, deviceLabel, sn, null, p23l1Param, p23l2Param, p23l3Param), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => target.MapPvOutputArgs(uri, contentType, body, deviceLabel, sn, string.Empty, p23l1Param, p23l2Param, p23l3Param), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void MapPvOutputArgsFromUrlQuery()
        {
            // Arrange
            var uri = new Uri("http://localhost?d=20150312&t=20:50&v1=123&v2=456&c1=1");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, "text/plain", body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            Assert.That(liveReading.Label, Is.EqualTo(deviceLabel));
            Assert.That(liveReading.DeviceId, Is.EqualTo(sn));
            Assert.That(liveReading.Timestamp, Is.EqualTo(new DateTime(2015, 3, 12, 19, 50, 0)));
            Assert.That(liveReading.Timestamp.Kind, Is.EqualTo(DateTimeKind.Utc));
            var registerValues = liveReading.GetRegisterValues();
            Assert.That(registerValues, Has.Count.EqualTo(2));
            AssertRegisterValue(new RegisterValueDto { ObisCode = ObisCode.ElectrActiveEnergyA23.ToString(), Value = 123, Scale = 0, Unit = Unit.WattHour }, registerValues.First());
            AssertRegisterValue(new RegisterValueDto { ObisCode = ObisCode.ElectrActualPowerP23.ToString(), Value = 456, Scale = 0, Unit = Unit.Watt }, registerValues.Last());
        }

        [Test]
        public void MapPvOutputArgsFromBody()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v1=123&v2=456&c1=1");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            Assert.That(liveReading.Label, Is.EqualTo(deviceLabel));
            Assert.That(liveReading.DeviceId, Is.EqualTo(sn));
            Assert.That(liveReading.Timestamp, Is.EqualTo(new DateTime(2015, 3, 12, 19, 50, 0)));
            Assert.That(liveReading.Timestamp.Kind, Is.EqualTo(DateTimeKind.Utc));
            var registerValues = liveReading.GetRegisterValues();
            Assert.That(registerValues, Has.Count.EqualTo(2));
            AssertRegisterValue(new RegisterValueDto { ObisCode = ObisCode.ElectrActiveEnergyA23.ToString(), Value = 123, Scale = 0, Unit = Unit.WattHour }, registerValues.First());
            AssertRegisterValue(new RegisterValueDto { ObisCode = ObisCode.ElectrActualPowerP23.ToString(), Value = 456, Scale = 0, Unit = Unit.Watt }, registerValues.Last());
        }

        [Test]
        public void MapPvOutputArgsDeviceIdInBody()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v1=123&v2=456&c1=1&v12=12345678");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, null, snParam, null, null, null);

            // Assert
            Assert.That(liveReading.DeviceId, Is.EqualTo("12345678"));
        }

        [Test]
        public void MapPvOutputArgsDeviceIdAbsent()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v1=123&v2=456&c1=1");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, null, snParam, null, null, null);

            // Assert
            Assert.That(liveReading, Is.Null);
        }

        [Test]
        public void MapPvOutputArgsFromBodyOnlyV1()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v1=123&c1=1");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            var registerValues = liveReading.GetRegisterValues();
            Assert.That(registerValues, Has.Count.EqualTo(1));
            Assert.That(registerValues.First().ObisCode, Is.EqualTo(ObisCode.ElectrActiveEnergyA23));
        }

        [Test]
        public void MapPvOutputArgsFromBodyOnlyV2()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v2=456");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            var registerValues = liveReading.GetRegisterValues();
            Assert.That(registerValues, Has.Count.EqualTo(1));
            Assert.That(registerValues.First().ObisCode, Is.EqualTo(ObisCode.ElectrActualPowerP23));
        }

        [Test]
        public void MapPvOutputArgsFromBodyAdditionalArgumentsAreIgnored()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v2=456&v3=789");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            var registerValues = liveReading.GetRegisterValues();
            Assert.That(registerValues, Has.Count.EqualTo(1));
        }

        [Test]
        public void MapPvOutputArgsFromBodyTolerantContentType()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v1=123&c1=1");
            var target = CreateTarget();
            var contentType = ApplicationFormUrlEncoded.ToLower() + "; charset=utf8";

            // Act
            var liveReading = target.MapPvOutputArgs(uri, contentType, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            var registerValues = liveReading.GetRegisterValues();
            Assert.That(registerValues, Has.Count.EqualTo(1));
        }

        [Test]
        public void MapPvOutputArgsFromBodyAbsentDate()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("t=20:50&v1=123&c1=1");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            Assert.That(liveReading, Is.Null);
        }

        [Test]
        public void MapPvOutputArgsFromBodyAbsentTime()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&v1=123&c1=1");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            Assert.That(liveReading, Is.Null);
        }

        [Test]
        public void MapPvOutputArgsFromBodyAbsentC1()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v1=123");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            Assert.That(liveReading, Is.Null);
        }

        [Test]
        public void MapPvOutputArgsFromBodyC1Not1()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v1=123&c1=0");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            Assert.That(liveReading, Is.Null);
        }

        [Test]
        public void MapPvOutputArgsFromBodyAbsentValues()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, null, null, null);

            // Assert
            Assert.That(liveReading, Is.Null);
        }

        [Test]
        public void MapPvOutputArgsPhasePowerFromBody()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v2=456&v7=22&v8=33&v9=44");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, "v7", "v8", "v9");

            // Assert
            var registerValues = liveReading.GetRegisterValues();
            Assert.That(registerValues, Has.Count.EqualTo(4));
            AssertRegisterValue(new RegisterValueDto { ObisCode = ObisCode.ElectrActualPowerP23L1.ToString(), Value = 22, Scale = 0, Unit = Unit.Watt }, registerValues[1]);
            AssertRegisterValue(new RegisterValueDto { ObisCode = ObisCode.ElectrActualPowerP23L2.ToString(), Value = 33, Scale = 0, Unit = Unit.Watt }, registerValues[2]);
            AssertRegisterValue(new RegisterValueDto { ObisCode = ObisCode.ElectrActualPowerP23L3.ToString(), Value = 44, Scale = 0, Unit = Unit.Watt }, registerValues[3]);
        }

        [Test]
        public void MapPvOutputArgsPhasePowerNotPresentFromBody()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            const string deviceLabel = "dl";
            const string sn = "1";
            const string snParam = "v12";
            var body = GetStream("d=20150312&t=20:50&v2=456");
            var target = CreateTarget();

            // Act
            var liveReading = target.MapPvOutputArgs(uri, ApplicationFormUrlEncoded, body, deviceLabel, sn, snParam, "v7", "v8", "v9");

            // Assert
            var registerValues = liveReading.GetRegisterValues();
            Assert.That(registerValues, Has.Count.EqualTo(1));
        }

        private static void AssertRegisterValue(RegisterValueDto dto, RegisterValue rv)
        {
            Assert.That(rv.ObisCode.ToString(), Is.EqualTo(dto.ObisCode));
            Assert.That(rv.Value, Is.EqualTo(dto.Value));
            Assert.That(rv.Scale, Is.EqualTo(dto.Scale));
            Assert.That(rv.Unit.ToString().ToLower(), Is.EqualTo(dto.Unit.Value.ToString().ToLower()));
        }

        private static LiveReadingMapper CreateTarget()
        {
            return new LiveReadingMapper(new NullLogger<LiveReadingMapper>());
        }

        private static Stream GetStream(object dto)
        {
            return GetStream(Newtonsoft.Json.JsonConvert.SerializeObject(dto));
        }

        private static Stream GetStream(string s)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(s);
            var stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;
            return stream;
        }

    }
}

