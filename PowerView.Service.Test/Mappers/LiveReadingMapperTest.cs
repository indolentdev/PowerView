using System;
using System.IO;
using System.Linq;
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
    public void MapThrows()
    {
      // Arrange
      var stream = new MemoryStream();
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Map(string.Empty, stream), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Map(null, stream), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => target.Map("whatnot", stream), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => target.Map(ApplicationJson, null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    [TestCase("Bad JSON", "Json invalid")]
    [TestCase("{}", "Items array absent")]
    [TestCase("{\"Items\":[{\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}]}]}", "Label property absent")]
    [TestCase("{\"Items\":[{\"Label\":null,\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}]}]}", "Label property null")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}]}]}", "SerialNumber property absent")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":null,\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}]}]}", "SerialNumber property null")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}]}]}", "Timestamp property absent")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07Q21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}]}]}", "Timestamp property invalid format")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}]}]}", "Timestamp property not UTC")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\"}]}", "RegisterValues property absent")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"Value\":2,\"Scale\":1,\"Unit\":\"watt\"}]}]}", "ObisCode property absent")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Scale\":1,\"Unit\":\"watt\"}]}]}", "Value property absent")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":\"BAD\",\"Scale\":1,\"Unit\":\"watt\"}]}]}", "Value property invalid")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Unit\":\"watt\"}]}]}", "Scale property absent")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":\"BAD\",\"Unit\":\"watt\"}]}]}", "Scale property invalid")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1}]}]}", "Unit property absent")]
    [TestCase("{\"Items\":[{\"Label\":\"lbl\",\"SerialNumber\":\"sn\",\"Timestamp\":\"2020-03-07T21:44:22Z\",\"RegisterValues\":[{\"ObisCode\":\"1.2.3.4.5.6\",\"Value\":2,\"Scale\":1,\"Unit\":\"BADUNIT\"}]}]}", "Unit property invalid")]
    public void MapInvalidJsonThrows(string json, string message)
    {
      // Arrange
      var stream = GetStream(json);
      var target = CreateTarget();

      // Act & Assert
      Assert.That(() => target.Map(ApplicationJson, stream).ToArray(), Throws.TypeOf<ArgumentOutOfRangeException>(), message);
    }

    [Test]
    public void Map()
    {
      // Arrange
      var rv1 = new RegisterValueDto { ObisCode = "1.2.3.4.5.6", Value = 2, Scale = 1, Unit = "watt" };
      var rv2 = new RegisterValueDto { ObisCode = "6.5.4.3.2.1", Value = 20, Scale = 10, Unit = "wattHOUR" };
      var rvs = new [] { rv1, rv2 };
      var lr = new LiveReadingDto { Label = "lbl", SerialNumber="4", Timestamp = DateTime.UtcNow, RegisterValues = rvs };
      var dto = new LiveReadingSetDto { Items = new [] { lr } };
      var stream = GetStream(dto);
      var target = CreateTarget();

      // Act
      var liveReadings = target.Map(ApplicationJson, stream).ToArray();

      // Assert
      Assert.That(liveReadings, Has.Length.EqualTo(1));
      var liveReading = liveReadings.First();
      Assert.That(liveReading.Label, Is.EqualTo(lr.Label));
      Assert.That(liveReading.SerialNumber, Is.EqualTo(lr.SerialNumber));
      Assert.That(liveReading.Timestamp, Is.EqualTo(lr.Timestamp));
      var registerValues = liveReading.GetRegisterValues();
      Assert.That(registerValues, Has.Length.EqualTo(rvs.Length));
      AssertRegisterValue(rv1, registerValues.First());
      AssertRegisterValue(rv2, registerValues.Last());
    }

    [Test]
    public void MapSerialNumberInteger()
    {
      // Arrange
      var rv1 = new RegisterValueDto { ObisCode = "1.2.3.4.5.6", Value = 2, Scale = 1, Unit = "watt" };
      var rvs = new[] { rv1 };
      var lr = new { Label = "lbl", SerialNumber=4, Timestamp = DateTime.UtcNow, RegisterValues = rvs };
      var dto = new  { Items = new[] { lr } };
      var stream = GetStream(dto);
      var target = CreateTarget();

      // Act
      var liveReadings = target.Map(ApplicationJson, stream).ToArray();

      // Assert
      Assert.That(liveReadings, Has.Length.EqualTo(1));
      var liveReading = liveReadings.First();
      Assert.That(liveReading.SerialNumber, Is.EqualTo(lr.SerialNumber.ToString()));
    }

    [Test]
    public void MapWithAdditionallyQualifiedContentType()
    {
      // Arrange
      var rvs = new [] { new RegisterValueDto { ObisCode = "6.5.4.3.2.1", Unit = "wattHOUR" } };
      var lr = new LiveReadingDto { Label = "lbl", SerialNumber = "5", Timestamp = DateTime.UtcNow, RegisterValues = rvs };
      var dto = new LiveReadingSetDto { Items = new [] { lr } };
      var stream = GetStream(dto);
      var target = CreateTarget();

      // Act
      var liveReadings = target.Map("application/json; charset=UTF-8", stream).ToArray();

      // Assert
      Assert.That(liveReadings, Has.Length.EqualTo(1));
    }

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
      Assert.That(liveReading.SerialNumber, Is.EqualTo(sn));
      Assert.That(liveReading.Timestamp, Is.EqualTo(new DateTime(2015, 3, 12, 19, 50, 0)));
      Assert.That(liveReading.Timestamp.Kind, Is.EqualTo(DateTimeKind.Utc));
      var registerValues = liveReading.GetRegisterValues();
      Assert.That(registerValues, Has.Length.EqualTo(2));
      AssertRegisterValue(new RegisterValueDto { ObisCode=ObisCode.ElectrActiveEnergyA23.ToString(), Value=123, Scale=0, Unit="watthour" }, registerValues.First());
      AssertRegisterValue(new RegisterValueDto { ObisCode=ObisCode.ElectrActualPowerP23.ToString(), Value=456, Scale=0, Unit="watt" }, registerValues.Last());
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
      Assert.That(liveReading.SerialNumber, Is.EqualTo(sn));
      Assert.That(liveReading.Timestamp, Is.EqualTo(new DateTime(2015,3,12,19,50,0)));
      Assert.That(liveReading.Timestamp.Kind, Is.EqualTo(DateTimeKind.Utc));
      var registerValues = liveReading.GetRegisterValues();
      Assert.That(registerValues, Has.Length.EqualTo(2));
      AssertRegisterValue(new RegisterValueDto{ ObisCode=ObisCode.ElectrActiveEnergyA23.ToString(),Value=123,Scale=0,Unit="watthour" }, registerValues.First());
      AssertRegisterValue(new RegisterValueDto{ ObisCode=ObisCode.ElectrActualPowerP23.ToString(),Value=456,Scale=0,Unit="watt" }, registerValues.Last());
    }

    [Test]
    public void MapPvOutputArgsSerialNumberInBody()
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
      Assert.That(liveReading.SerialNumber, Is.EqualTo("12345678"));
    }

    [Test]
    public void MapPvOutputArgsSerialNumberAbsent()
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
      Assert.That(registerValues, Has.Length.EqualTo(1));
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
      Assert.That(registerValues, Has.Length.EqualTo(1));
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
      Assert.That(registerValues, Has.Length.EqualTo(1));
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
      Assert.That(registerValues, Has.Length.EqualTo(1));
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
      Assert.That(registerValues, Has.Length.EqualTo(4));
      AssertRegisterValue(new RegisterValueDto { ObisCode=ObisCode.ElectrActualPowerP23L1.ToString(), Value=22, Scale=0, Unit="watt" }, registerValues[1]);
      AssertRegisterValue(new RegisterValueDto { ObisCode=ObisCode.ElectrActualPowerP23L2.ToString(), Value=33, Scale=0, Unit="watt" }, registerValues[2]);
      AssertRegisterValue(new RegisterValueDto { ObisCode=ObisCode.ElectrActualPowerP23L3.ToString(), Value=44, Scale=0, Unit="watt" }, registerValues[3]);
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
      Assert.That(registerValues, Has.Length.EqualTo(1));
    }

    private static void AssertRegisterValue(RegisterValueDto dto, RegisterValue rv)
    {
      Assert.That(rv.ObisCode.ToString(), Is.EqualTo(dto.ObisCode));
      Assert.That(rv.Value, Is.EqualTo(dto.Value));
      Assert.That(rv.Scale, Is.EqualTo(dto.Scale));
      Assert.That(rv.Unit.ToString().ToLower(), Is.EqualTo(dto.Unit.ToLower()));
    }

    private static LiveReadingMapper CreateTarget()
    {
      return new LiveReadingMapper();
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

