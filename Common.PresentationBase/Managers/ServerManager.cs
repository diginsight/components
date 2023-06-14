#region using
using Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
#endregion

namespace Common
{
    public enum Layer
    {
        Bizapi,
        Webapi
    }

    public class ServerManager
    {
        #region const
        public const string HTTPHEADER_REFERER = "Referer"; // Request-Id "OperationID"
        public const string HTTPHEADER_OPERATIONID = "RequestId"; // Request-Id "OperationID"
        public const string HTTPHEADER_SESSIONID = "SessionId"; // Session-Id "SessionId"
        public const string HTTPHEADER_OPERATIONDEPT = "RequestDept"; // Request-Id
        public const string HTTPHEADER_MAXAGE = "MaxAge"; // Request-Id
        public const string HTTPHEADER_MINIMUNCREATIONDATE = "MinCreationDate"; // Request-Id
        public const string HTTPHEADER_CACHETYPE = "CacheType"; // Request-Id
        public const string HTTPHEADER_USERNAME = "UserId"; // Request-Id
        //public const string HTTPHEADER_USERPO = "UserPO"; // Request-Id
        public const string HTTPHEADER_USERUO = "UserUO"; // Request-Id
        public const string HTTPHEADER_PROFILEURL = "ProfileUrl"; // Request-Id 
        #endregion
        #region internal state
        private ILogger<ServerManager> logger;
        bool _offline = false;
        Type _type = null;
        static bool _enableInprocessInvocation = true;
        static string _operationCompareKey = null;
        static string _operationSuffixKey = null;
        static bool _operationSuffixKeyReplaceDefaultKey = true;
        static bool _initialized = false;
        #endregion

        #region Initialize
        public static void Initialize(IList<KeyValuePair<string, string>> settings)
        {
            using (var tracer = TraceManager.GetCodeSection<ServerManager>(null, new { settings }))
            {
                _enableInprocessInvocation = ConfigurationHelper.GetSetting("Api.EnableInProcessInvocation", true); 
                _operationCompareKey = ConfigurationHelper.GetSetting("OperationCompareKey", "{System:MachineName}#{User:DomainName}#{User:UO}#{Business:PuntoOperativo}#{Business:Filiale}#{Message:MessageTypeName}#{Message:Action}#{Message:Key}"); 
                _operationSuffixKey = ConfigurationHelper.GetSetting("OperationSuffixKey", "{System:MachineName}#{User:DomainName}#{User:UO}#{Business:PuntoOperativo}#{Business:Filiale}#{Message:MessageTypeName}#{Message:Action}#{Message:Key}"); 
                _operationSuffixKeyReplaceDefaultKey = ConfigurationHelper.GetSetting("OpertionSuffixKeyReplaceDefaultKey", true); 

                _initialized = true;
            }
        }
        #endregion

        #region .ctor
        static ServerManager()
        {
        }
        public ServerManager(Type t)
        {
            _type = t;
        }
        #endregion

        #region Invoke<Request, Response>(Request request)
        /// <summary>invokes a server webapi</summary>
        /// <typeparam name="Request"></typeparam>
        /// <typeparam name="Response"></typeparam>
        /// <param name="request">request</param>
        /// <param name="offline"></param>
        /// <returns></returns>
        public Response Invoke<Request, Response>(Request request, Func<Request, Response> offline = null)
            where Response : class, new()
            where Request : class
        {
            using (var section = logger.BeginMethodScope(new { request = request.GetLogString() }))
            {
                var processName = TraceManager.ProcessName;
                var processNameParts = processName?.Split('.');
                var processLayerString = processNameParts != null && processNameParts.Length > 1 ? processNameParts?[1] : "Bizapi";
                Enum.TryParse<Layer>(processLayerString, out var processLayer);

                if (_offline == true) { return offline != null ? offline(request) : default(Response); }


                Response response = default(Response);
                return response;
            }
        }
        #endregion
        #region InvokeAsync<Request, Response>(Request request)
        virtual async public Task<Response> InvokeAsync<Request, Response>(Request request, Func<Request, Task<Response>> onlineAsync, Func<Request, Response> offline = null)
            where Response : new()
        {
            //if (_isDesignMode) { return default(Response); }

            try
            {
                return await onlineAsync(request);
            }
            finally
            {
                //try { OnInvokeCompleted(this, new InvokeCompletedArgs(target, null)); } catch (Exception) { }
            }
        }
        #endregion

        #region OnCompleted
        public virtual void OnCompleted()
        {
        }
        #endregion
        #region OnError
        public virtual void OnError(Exception error)
        {
        }
        #endregion

    }
}
