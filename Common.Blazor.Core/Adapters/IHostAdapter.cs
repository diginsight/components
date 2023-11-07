#region using
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks; 
#endregion

namespace Common.Core.Blazor
{
    public class WebAssemblyIHostAdapter : IHost
    {
        WebAssemblyHost _host;

        public WebAssemblyIHostAdapter(WebAssemblyHost host) { _host = host; }

        public IServiceProvider Services
        {
            get
            {
                if (_host == null) { return null; }
                return _host.Services;
            }
        }

        public void Dispose()
        {
            if (_host == null) { return; }

            _host.DisposeAsync();
            _host = null;
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_host == null) { return Task.CompletedTask; }
            return _host.RunAsync();
        }
        public Task StopAsync(CancellationToken cancellationToken = default) { return Task.CompletedTask; }
    }
}
