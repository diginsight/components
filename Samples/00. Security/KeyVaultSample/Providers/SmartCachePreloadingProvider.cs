#region using
using Azure.ResourceManager.Resources;
using Common.PresentationBase;
//using Common.SmartCache;
using DotNext;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xmasdev2022.DolcettoCarbone.Common;
using Xmasdev2022.DolcettoCarbone.Models;
#endregion

namespace Common
{
    //public class SmartCachePreloading : ILogger
    //{
    //    public IHost Host { get; set; }
    //    private readonly List<TraceEntrySurrogate> traceEntries = new List<TraceEntrySurrogate>();

    //    //private SmartCachePreloadingProvider Provider = null;
    //    private ModelInput modelInput = new();
    //    private readonly Predictor predictor;
    //    private string filePath = $"{Directory.GetCurrentDirectory()}\\Models\\classification.mdl";
    //    private IParallelService parallelService;
    //    private AuthenticationService authenticationService;
    //    private IGraphAPIClientHttp graphAPIClientHttp;
    //    private IArmAPIClient armAPIClient;

    //    static Reference<bool> _lockInitialize = new Reference<bool>();

    //    public SmartCachePreloading(SmartCachePreloadingProvider provider)
    //    {
    //        if (_lockInitialize.Value) { return; }
    //        using var switchLocal = new SwitchOnDispose(_lockInitialize, true);

    //        this.Host = TraceLogger.Host;
    //        if (this.Host == null) { return; }
    //        this.Provider = provider;

    //        this.predictor = provider.Predictor;


    //        var parallelService = this.Host.Services.GetService<IParallelService>();
    //        this.parallelService = parallelService;
    //        var authenticationService = this.Host.Services.GetService<AuthenticationService>();
    //        this.authenticationService = authenticationService;
    //        var graphAPIClientHttp = this.Host.Services.GetService<IGraphAPIClientHttp>();
    //        this.graphAPIClientHttp = graphAPIClientHttp;
    //        var armAPIClient = this.Host.Services.GetService<IArmAPIClient>();
    //        this.armAPIClient = armAPIClient;
    //    }


    //    public IDisposable BeginScope<TState>(TState state) { return null; }
    //    public bool IsEnabled(LogLevel logLevel) { return true; }

    //    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    //    {
    //        if (this.Host == null) { return; }
    //        if (graphAPIClientHttp == null) { return; }
    //        if (armAPIClient == null) { return; }

    //        //var entry = default(TraceEntry);
    //        //var isTraceEnty = ;
    //        if (state is not TraceEntry entry) { return; }
    //        if (entry.Properties == null) { return; }
    //        var ok = entry.Properties.TryGetValue("Tags", out var tagsValue);
    //        if (tagsValue == null) { return; }
    //        var tagsString = tagsValue as string;
    //        var tags = tagsString.Split(',');

    //        // reset action => reset
    //        if (tags.Contains("Reset")) { traceEntries.Clear(); return; }

    //        var surrogate = this.Provider.GetTraceSurrogate(entry);
    //        traceEntries.Add(surrogate);

    //        var shouldPredict = tags.Contains("Predict");
    //        if (entry.TraceEventType == TraceEventType.Stop) { shouldPredict = false; }

    //        if (shouldPredict)
    //        {
    //            var modelInput = GetModelInput(traceEntries);
    //            var prediction = predictor.Predict(filePath, modelInput);
    //            if (prediction.PredictedLabel)
    //            {
    //                ApplicationBase.Current.Dispatcher.Invoke(async () =>
    //                {
    //                    var cacheContext = new CacheContext() { Enabled = true, MaxAge = 600 };

    //                    var tenantObjects = await armAPIClient.GetTenantsAsync(cacheContext: cacheContext);
    //                    var tenants = new List<TenantData>();
    //                    foreach (var tenantObj in tenantObjects)
    //                    {
    //                        var tenantItem = tenantObj.Data;
    //                        TraceLogger.LogDebug($"Id:{tenantItem.Id},TenantId:{tenantItem.TenantId},DisplayName:{tenantItem.DisplayName},DefaultDomain:{tenantItem.DefaultDomain},TenantCategory:{tenantItem.TenantCategory},TenantType:{tenantItem.TenantType},Country:{tenantItem.Country},CountryCode:{tenantItem.CountryCode}");
    //                        tenants.Add(tenantItem);
    //                    }

    //                    var options = new ParallelOptions() { MaxDegreeOfParallelism = parallelService.MediumConcurrency };
    //                    var clientId = authenticationService.ApplicationId;
    //                    await parallelService.ForEachAsync(tenants ?? new List<TenantData>(), options, async (tenant) =>
    //                    {
    //                        var identity = ApplicationBase.Current.Properties["Identity"] as Identity;
    //                        //application = await GetOwnedApplicationAsync(identity, applicationTenantId, clientId);
    //                        var applicationTenantId = tenant.TenantId.ToString();
    //                        cacheContext = new CacheContext() { Enabled = true, MaxAge = 600 };
    //                        var apps = await graphAPIClientHttp.GetUserApplicationsAsync(identity, applicationTenantId, Guid.Parse(clientId), cacheContext);
    //                        var application = apps?.FirstOrDefault(app => app.AppId == clientId);
    //                        if (application != null) { var breakException = new BreakLoopException(); breakException.Data["item"] = tenant; throw breakException; }
    //                    });
    //                });
    //                //Task.Run(async () =>
    //                //{
    //                //});
    //            }
    //            traceEntries.Clear();
    //        }
    //    }

    //    private static ModelInput GetModelInput(IList<TraceEntrySurrogate> entries)
    //    {
    //        return new ModelInput()
    //        {
    //            GiocattoliRotti = 12,
    //            MediaVoti = 4,
    //            Note = 1,
    //            Parolacce = 2,
    //            VisiteNonni = 122
    //        };
    //    }

    //}

    //public class SmartCachePreloadingProvider : ILoggerProvider, IFormatTraceEntry
    //{
    //    #region const
    //    public const string CONFIGSETTING_CRREPLACE = "CRReplace"; public const string CONFIGDEFAULT_CRREPLACE = "\\r";
    //    public const string CONFIGSETTING_LFREPLACE = "LFReplace"; public const string CONFIGDEFAULT_LFREPLACE = "\\n";
    //    public const string CONFIGSETTING_TIMESTAMPFORMAT = "TimestampFormat"; public const string CONFIGDEFAULT_TIMESTAMPFORMAT = "HH:mm:ss.fff"; // dd/MM/yyyy 
    //    public const string CONFIGSETTING_FLUSHONWRITE = "FlushOnWrite"; public const bool CONFIGDEFAULT_FLUSHONWRITE = false;
    //    public const string CONFIGSETTING_WRITESTARTUPENTRIES = "WriteStartupEntries"; public const bool CONFIGDEFAULT_WRITESTARTUPENTRIES = true;
    //    public const string CONFIGSETTING_MERGEMESSAGEANDPAYLOAD = "MergeMessageAndPayload"; public const bool CONFIGDEFAULT_MERGEMESSAGEANDPAYLOAD = true;
    //    #endregion
    //    #region internal state
    //    private static Type T = typeof(TraceLoggerJsonProvider);
    //    private static readonly string _traceSourceName = "TraceSource";
    //    public static Func<string, string> CRLF2Space = (string s) => { return s?.Replace("\r", " ")?.Replace("\n", " "); };
    //    public static Func<string, string> CRLF2Encode = (string s) => { return s?.Replace("\r", "\\r")?.Replace("\n", "\\n"); };
    //    public string Name { get; set; }
    //    public string ConfigurationSuffix { get; set; }
    //    bool _lastWriteContinuationEnabled;
    //    public string _CRReplace, _LFReplace;
    //    public string _timestampFormat;
    //    public bool _showNestedFlow, _showTraceCost, _flushOnWrite;
    //    public string _traceDeltaDefault;
    //    public bool _writeStartupEntries;
    //    public bool _mergeMessageAndPayload;
    //    public TraceEventType _allowedEventTypes = TraceEventType.Critical | TraceEventType.Error | TraceEventType.Warning | TraceEventType.Information | TraceEventType.Verbose | TraceEventType.Start | TraceEventType.Stop | TraceEventType.Suspend | TraceEventType.Resume | TraceEventType.Transfer;
    //    TraceEntry lastWrite = default(TraceEntry);
    //    ILoggerProvider _provider;

    //    public static ConcurrentDictionary<string, object> Properties { get; set; } = new ConcurrentDictionary<string, object>();
    //    public IList<ILogger> Listeners { get; } = new List<ILogger>();
    //    #endregion

    //    //public IHost Host { get; set; }
    //    public Predictor Predictor { get; set; }

    //    public SmartCachePreloadingProvider() { }
    //    public SmartCachePreloadingProvider(IConfiguration configuration)
    //    {
    //        TraceLogger.InitConfiguration(configuration);
    //    }

    //    public ILogger CreateLogger(string categoryName)
    //    {
    //        if (Predictor == null) { Predictor = new Predictor(); }

    //        var logger = new SmartCachePreloading(this);
    //        return logger;
    //    }
    //    public void Dispose()
    //    {
    //        ;
    //    }

    //    public string FormatTraceEntry(TraceEntry entry, Exception ex)
    //    {
    //        var traceSurrogate = GetTraceSurrogate(entry);
    //        var entryJson = SerializationHelper.SerializeJson(traceSurrogate);
    //        return entryJson;
    //    }
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public TraceEntrySurrogate GetTraceSurrogate(TraceEntry entry)
    //    {
    //        var codeSection = entry.CodeSectionBase;
    //        var message = entry.GetMessage != null ? entry.GetMessage() : entry.Message;
    //        if (_mergeMessageAndPayload && entry.MessageObject != null)
    //        {
    //            var payloadJson = entry.MessageObject.GetLogString();
    //            message = payloadJson;
    //        }
    //        var payload = codeSection.Payload is Func<object> func ? func() : codeSection.Payload;
    //        if (_mergeMessageAndPayload && payload != null)
    //        {
    //            var payloadJson = payload.GetLogString();
    //            payload = payloadJson;
    //        }

    //        return new TraceEntrySurrogate()
    //        {
    //            TraceEventType = entry.TraceEventType,
    //            TraceEventTypeDesc = entry.TraceEventType.ToString(),
    //            TraceSourceName = entry.TraceSource?.Name,
    //            LogLevel = entry.LogLevel,
    //            Message = message,
    //            MessageObject = _mergeMessageAndPayload == false ? entry.MessageObject : null,
    //            Properties = entry.Properties,
    //            Source = entry.Source,
    //            Category = entry.Category,
    //            SourceLevel = entry.SourceLevel,
    //            ElapsedMilliseconds = entry.ElapsedMilliseconds,
    //            Timestamp = entry.Timestamp,
    //            Exception = entry.Exception,
    //            ThreadID = entry.ThreadID,
    //            ApartmentState = entry.ApartmentState,
    //            DisableCRLFReplace = entry.DisableCRLFReplace,
    //            CodeSection = codeSection != null ? new CodeSectionSurrogate()
    //            {
    //                NestingLevel = codeSection.NestingLevel,
    //                OperationDept = codeSection.OperationDept,
    //                Payload = payload,
    //                Result = codeSection.Result,
    //                Name = codeSection.Name,
    //                MemberName = codeSection.MemberName,
    //                SourceFilePath = codeSection.SourceFilePath,
    //                SourceLineNumber = codeSection.SourceLineNumber,
    //                DisableStartEndTraces = codeSection.DisableStartEndTraces,
    //                TypeName = codeSection.T?.Name,
    //                TypeFullName = codeSection.T?.FullName,
    //                AssemblyName = codeSection.Assembly?.GetName()?.Name,
    //                AssemblyFullName = codeSection.Assembly?.FullName,
    //                TraceSourceName = codeSection.TraceSource?.Name,
    //                TraceEventType = codeSection.TraceEventType,
    //                SourceLevel = codeSection.SourceLevel,
    //                LogLevel = codeSection.LogLevel,
    //                Properties = codeSection.Properties,
    //                Source = codeSection.Source,
    //                Category = codeSection.Category,
    //                CallStartMilliseconds = codeSection.CallStartMilliseconds,
    //                SystemStartTime = codeSection.SystemStartTime,
    //                OperationID = codeSection.OperationID,
    //                IsInnerScope = codeSection.IsInnerScope
    //            } : null
    //        };
    //    }
    //}
}
