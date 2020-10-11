using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using log4net;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using NUnit.Framework;
using PowerView.Model;

namespace PowerView.Service.Test.Mqtt
{
  internal class TestMqttServer : IDisposable
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private IMqttServer mqttServer;
    private List<MqttApplicationMessageReceivedEventArgs> published;

    public readonly string ServerName = "localhost";
    public int Port { get; private set; }
    public IList<MqttApplicationMessageReceivedEventArgs> Published { get { return published; } }

    public TestMqttServer()
    {
      published = new List<MqttApplicationMessageReceivedEventArgs>();
      Port = FreeTcpPort();
    }

    internal void AssertPublishCount(int count)
    {
      Assert.That(published.Count, Is.EqualTo(count));
    }

    private static int FreeTcpPort()
    {
      TcpListener l = new TcpListener(IPAddress.Loopback, 0);
      l.Start();
      int port = ((IPEndPoint)l.LocalEndpoint).Port;
      l.Stop();
      return port;
    }

    public MqttConfig GetClientConfig()
    {
      return new MqttConfig(ServerName, (ushort)Port, true);
    }

    public void Start()
    {
      var options = new MqttServerOptionsBuilder()
        .WithDefaultEndpointPort(Port)
        .WithDefaultEndpointBoundIPAddress(IPAddress.Loopback)
        .WithDefaultEndpointBoundIPV6Address(IPAddress.None)
        .Build();

      var mqttNetLogger = new MqttNetLogger();
      mqttNetLogger.LogMessagePublished += (sender, e) => log.Debug(e.LogMessage);
      mqttServer = new MqttFactory().CreateMqttServer(mqttNetLogger);
      mqttServer.UseApplicationMessageReceivedHandler(e => published.Add(e));
      Assert.That(mqttServer.StartAsync(options).Wait(3000), Is.True, "MQTT Server failed to start");
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          if (mqttServer != null)
          { 
            Assert.IsTrue(mqttServer.StopAsync().Wait(3000), "MqttServer to did not stop timely"); 
          }
          mqttServer = null;
        }

        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }

}
