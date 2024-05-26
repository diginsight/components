//using Common.SmartCache;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application = Microsoft.Graph.Models.Application;
//using Application = System.Windows.Application;

namespace Common.PresentationBase
{
    public interface IGraphAPIClientHttp
    {
        Task<IEnumerable<Application>> GetUserApplicationsAsync(Identity identity, string tenantId, Guid clientId);
    }
}
