using System;
using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics.Logger;

namespace PowerView.Service.Mqtt;

internal class MqttNetLogger : IMqttNetLogger
{
    private readonly ILogger logger;

    public MqttNetLogger(ILogger logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsEnabled => true;

    public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
    {
        logger.Log(MapLevel(logLevel), exception, message, parameters);
    }

    private static LogLevel MapLevel(MqttNetLogLevel level)
    {
        switch (level)
        {
            case MqttNetLogLevel.Verbose: return LogLevel.Debug;
            case MqttNetLogLevel.Info: return LogLevel.Information;
            case MqttNetLogLevel.Warning: return LogLevel.Warning;
            case MqttNetLogLevel.Error: return LogLevel.Error;
            default: return LogLevel.Information;
        }
    }
}