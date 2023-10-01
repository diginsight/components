#region using
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
#endregion

namespace Common
{
    public class SmartCacheRecorder : ILogger
    {
        private readonly List<TraceEntrySurrogate> traceEntries = new List<TraceEntrySurrogate>();

        public IDisposable BeginScope<TState>(TState state)
        {
            Debug.WriteLine(state);
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var isTraceEnty = state is TraceEntry;
            TraceEntry entry = default;

            if (isTraceEnty)
            {
                // reset action => reset


                // Get TraceEntrySurrogate corresponding to action
                // Accumulate traceEntrySurrogate action
                // if sequence closing action => send for training 
            }
        }
    }
    public class SmartCacheTrainingProvider : ILoggerProvider
    {
        public SmartCacheTrainingProvider() { }

        public ILogger CreateLogger(string categoryName)
        {
            var logger = new SmartCacheRecorder();
            return logger;
        }
        public void Dispose()
        {
            ;
        }
    }
}
