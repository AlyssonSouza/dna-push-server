using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Infrastructure
{
    public class Logger
    {
        private TelemetryClient _appInsightsClient;

        public Logger()
        {
            _appInsightsClient = new TelemetryClient();
            _appInsightsClient.InstrumentationKey = ConfigurationManager.AppSettings["Insights_Key"];
        }

        public void Info(string message)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            _appInsightsClient.TrackEvent("Info", properties);
        }

        public void Warn(string message)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            _appInsightsClient.TrackEvent("Warn", properties);
        }

        public void Debug(string message)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            _appInsightsClient.TrackEvent("Debug", properties);
        }

        public void Error(string message, Exception ex)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            _appInsightsClient.TrackException(ex, properties);
        }

        public void Error(string message)
        {
            var properties = new Dictionary<string, string> { { "message", message } };
            Exception ex = new Exception(message);
            _appInsightsClient.TrackException(ex, properties);
        }

        public void Error(Exception ex)
        {
            _appInsightsClient.TrackException(ex);
        }
    }
}
