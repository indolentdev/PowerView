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
        private const string MqttClientId = MqttPrefix + "ClientId";

        private const string defaultServer = "localhost";
        private const ushort defaultPort = 1883;
        private const string defaultClientId = "PowerView";
        private readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(15);

        private readonly TimeSpan? timeout;

        public MqttConfig(string server, ushort port, bool enabled, string clientId, TimeSpan? timeout = null)
        {
            if (string.IsNullOrEmpty(server))
            {
                server = defaultServer;
            }
            if (string.IsNullOrEmpty(clientId))
            {
                clientId = defaultClientId;
            }

            Server = server;
            Port = port;
            PublishEnabled = enabled;
            ClientId = clientId;
            this.timeout = timeout;
        }

        internal MqttConfig(ICollection<KeyValuePair<string, string>> mqttSettings)
        {
            ArgumentNullException.ThrowIfNull(mqttSettings);

            var server = defaultServer;
            var port = defaultPort;
            var enabled = false;
            var clientId = defaultClientId;

            var serverString = GetValue(mqttSettings, MqttServer);
            var portString = GetValue(mqttSettings, MqttPort);
            var enabledString = GetValue(mqttSettings, MqttPublishEnabled);
            var clientIdString = GetValue(mqttSettings, MqttClientId);

            if (!string.IsNullOrEmpty(serverString))
            {
                server = serverString;
            }
            if (!string.IsNullOrEmpty(portString))
            {
                ushort.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out port);
            }
            if (!string.IsNullOrEmpty(enabledString) && bool.TryParse(enabledString, out enabled))
            {
            }
            if (!string.IsNullOrEmpty(clientIdString))
            {
                clientId = clientIdString;
            }

            Server = server;
            Port = port;
            PublishEnabled = enabled;
            ClientId = clientId;
        }

        private static string GetValue(ICollection<KeyValuePair<string, string>> mqttSettings, string key)
        {
            return mqttSettings.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value;
        }

        public string Server { get; private set; }
        public ushort Port { get; private set; }
        public bool PublishEnabled { get; private set; }
        public string ClientId { get; private set; }
        public TimeSpan Timeout { get { return timeout != null ? timeout.Value : defaultTimeout; } }

        internal ICollection<KeyValuePair<string, string>> GetSettings()
        {
            var settings = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(MqttServer, Server),
        new KeyValuePair<string, string>(MqttPort, Port.ToString(CultureInfo.InvariantCulture)),
        new KeyValuePair<string, string>(MqttPublishEnabled, PublishEnabled.ToString()),
        new KeyValuePair<string, string>(MqttClientId, ClientId)
      };
            return settings;
        }

    }
}
