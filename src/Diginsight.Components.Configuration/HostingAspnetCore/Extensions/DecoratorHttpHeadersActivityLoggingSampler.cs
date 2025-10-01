using Diginsight.Diagnostics;
using Diginsight.Diagnostics.AspNetCore;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Diginsight.Components.Configuration;

//internal sealed class DecoratedActivityLoggingSamplerMarker;

//internal sealed class DecoratorHttpHeadersActivityLoggingSampler : HttpHeadersActivityLoggingSampler
//{
//    private readonly IActivityLoggingSampler decoratee;

//    public DecoratorHttpHeadersActivityLoggingSampler(
//        IActivityLoggingSampler decoratee, IHttpContextAccessor httpContextAccessor
//    )
//        : base(httpContextAccessor)
//    {
//        this.decoratee = decoratee;
//    }

//    public override LogBehavior? GetLogBehavior(Activity activity)
//    {
//        return base.GetLogBehavior(activity) ?? decoratee.GetLogBehavior(activity);
//    }
//}
