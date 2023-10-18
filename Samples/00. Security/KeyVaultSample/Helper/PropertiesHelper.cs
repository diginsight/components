using Azure.Core;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyVaultSample
{
    internal static class PROPS
    {
        //public static Dictionary<string, object> GetD(Dictionary<string, object> props) { return props; }
        public static Dictionary<string, object> Get((string, object)[] props) { return props.ToDictionary(t => t.Item1, t => t.Item2); }
    }

    public static class StringConstants
    {
        public const string LOGGINGCONFIGSECTION = "Logging";
        public const string APPINSIGHTSCONFIGSECTION = "ApplicationInsights";
        public const string APPINSIGHTSINSTRUMENTATIONKEY = "ApplicationInsights:InstrumentationKey";
        public const string APPINSIGHTSCONNECTIONSTRING = "ApplicationInsights:ConnectionString";
        public const string ENABLEREQUESTTRACKINGTELEMETRYMODULE = "ApplicationInsights:EnableRequestTrackingTelemetryModule";
        public const string INCLUDEOPERATIONID = "ApplicationInsights:IncludeOperationId";
        public const string INCLUDEREQUESTBODY = "ApplicationInsights:IncludeRequestBody";
        public const string INCLUDEHEADERS = "ApplicationInsights:IncludeHeaders";
    }
}