using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PowerView.Model
{
  public class MqttConfig
  {
    internal const string MqttPrefix = "MQTT_";
    private const string MqttServer = MqttPrefix + "Server";
    private const string MqttPort = MqttPrefix + "Port";
    private const string MqttPublishEnabled = MqttPrefix + "PublishEnabled";

    private const string defaultServer = "localhost";
    private const ushort defaultPort = 1883;
    private readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(10);

    private readonly TimeSpan? timeout;

    public MqttConfig(string server, ushort port, bool enabled, TimeSpan? timeout = null)
    {
      if (string.IsNullOrEmpty(server))
      {
        server = defaultServer;
      }
      
      Server = server;
      Port = port;
      PublishEnabled = enabled;
      this.timeout = timeout;
    }

    internal MqttConfig(ICollection<KeyValuePair<string, string>> mqttSettings)
    {
      if (mqttSettings == null) throw new ArgumentNullException("mqttSettings");

      var server = defaultServer;
      var port = defaultPort;
      var enabled = false;

      var serverString = GetValue(mqttSettings, MqttServer);
      var portString = GetValue(mqttSettings, MqttPort);
      var enabledString = GetValue(mqttSettings, MqttPublishEnabled);

      if (!string.IsNullOrEmpty(serverString))
      {
        server = serverString;
      }
      if (!string.IsNullOrEmpty(portString))
      {
        ushort.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out port);
      }
      if (!string.IsNullOrEmpty(enabledString))
      {
        bool.TryParse(enabledString, out enabled);
      }

      Server = server;
      Port = port;
      PublishEnabled = enabled;
    }

    private static string GetValue(ICollection<KeyValuePair<string, string>> mqttSettings, string key)
    {
      return mqttSettings.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value;
    }

    public string Server { get; private set; }
    public ushort Port { get; private set; }
    public bool PublishEnabled { get; private set; }
    public TimeSpan Timeout { get { return timeout != null ? timeout.Value : defaultTimeout; } }

    internal ICollection<KeyValuePair<string, string>> GetSettings()
    {
      var settings = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(MqttServer, Server),
        new KeyValuePair<string, string>(MqttPort, Port.ToString(CultureInfo.InvariantCulture)),
        new KeyValuePair<string, string>(MqttPublishEnabled, PublishEnabled.ToString())
      };
      return settings;
    }

  }
}
