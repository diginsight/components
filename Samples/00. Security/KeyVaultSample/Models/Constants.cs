using Azure.Core;
using Microsoft.Identity.Client;
//using Microsoft.Identity.Client.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyVaultSample
{
    public static class Constants
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