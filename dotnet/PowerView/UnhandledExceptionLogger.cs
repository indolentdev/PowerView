using Microsoft.Extensions.Logging;
using PowerView.Model;
using PowerView.Model.Repository;
using PowerView.Service.EventHub;

namespace PowerView
{
    internal static class UnhandledExceptionLogger
    {
        private static ILogger? appLogger;

        public static void SetApplicationLogger(ILogger logger)
        {
            appLogger = logger;
        }

        public static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (appLogger != null)
            {
                Log(appLogger, e);
            }
            else
            {
                using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Trace).AddConsole());
                var logger = loggerFactory.CreateLogger(typeof(UnhandledExceptionLogger));
                Log(logger, e);
            }
        }

        private static void Log(ILogger logger, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            logger.LogCritical(exception, $"Unhandled exception occurred. IsTerminating:{e.IsTerminating}");
        }

    }
}
