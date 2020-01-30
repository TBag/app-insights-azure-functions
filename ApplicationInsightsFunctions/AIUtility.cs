using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;

namespace ApplicationInsightsFunctions
{
    static class AIUtility
    {
        public static void TrackEvent(string name, Dictionary<string, string> properties, string instrumentationKey)
        {
            var telemetryClient = new TelemetryClient();
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                telemetryClient.InstrumentationKey = instrumentationKey;
            }
            telemetryClient.TrackEvent(name, properties);
        }

        public static void TrackPageView(string name, string instrumentationKey)
        {
            var telemetryClient = new TelemetryClient();
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                telemetryClient.InstrumentationKey = instrumentationKey;
            }
            telemetryClient.TrackPageView(name);
        }

        public static void TrackException(string source, string message, string instrumentationKey, Dictionary<string, string> properties)
        {
            var exceptionTelemetry = new ExceptionTelemetry();

            var powerAppsException = new PowerAppsException(message);
            powerAppsException.Source = source;

            exceptionTelemetry.Exception = powerAppsException;

            var telemetryClient = new TelemetryClient();
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                telemetryClient.InstrumentationKey = instrumentationKey;
            }

            telemetryClient.TrackException(powerAppsException, properties);
        }
    }

    public class PowerAppsException : Exception
    {
        public PowerAppsException() { }
        public PowerAppsException(string message) : base(message) { }
        public PowerAppsException(string message, Exception inner) : base(message, inner) { }
        protected PowerAppsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
