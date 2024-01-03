using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using MQTTnet.Server;
using NUnit.Framework;
using PowerView.Model;

namespace PowerView.Service.Test.Mqtt
{
  internal class TestMqttServer : IDisposable
  {
    private MqttServer mqttServer;
    private List<InterceptingPublishEventArgs> published;
    private List<ValidatingConnectionEventArgs> connections;

    public readonly string ServerName = "localhost";
    public readonly string ClientId = "PWClientId";
    public int Port { get; private set; }
    public IList<InterceptingPublishEventArgs> Published { get { return published; } }
    public IList<ValidatingConnectionEventArgs> Connections { get { return connections; } }

    public TestMqttServer()
    {
      published = new List<InterceptingPublishEventArgs>();
      connections = new List<ValidatingConnectionEventArgs>();
      Port = FreeTcpPort();
    }

    internal void AssertConnectionCount(int count)
    {
      Assert.That(connections.Count, Is.EqualTo(count));
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
      return new MqttConfig(ServerName, (ushort)Port, true, ClientId, TimeSpan.FromSeconds(0.75));
    }

    public void Start()
    {
      var options = new MqttServerOptionsBuilder()
        .WithDefaultEndpoint()
        .WithDefaultEndpointPort(Port)
        .Build();

      mqttServer = new MqttFactory().CreateMqttServer(options);
      mqttServer.ValidatingConnectionAsync += e => { e.ReasonCode = MqttConnectReasonCode.Success; connections.Add(e); return Task.CompletedTask; };
      mqttServer.InterceptingPublishAsync += e => { published.Add(e); return Task.CompletedTask; };
      Assert.That(mqttServer.StartAsync().Wait(3000), Is.True, "MQTT Server failed to start");
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
            Assert.That(mqttServer.StopAsync().Wait(3000), Is.True, "MqttServer to did not stop timely");
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
