using Diginsight.Diagnostics;
using Diginsight.Diagnostics.AspNetCore;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Diginsight.Components.Configuration;

//internal sealed class DecoratorHttpHeadersMetricRecorderSettings : HttpHeadersMetricRecordingFilter
//{
//    private readonly IMetricRecordingFilter decoratee;

//    public DecoratorHttpHeadersMetricRecorderSettings(
//        IMetricRecordingFilter decoratee, IHttpContextAccessor httpContextAccessor
//    )
//        : base(httpContextAccessor)
//    {
//        this.decoratee = decoratee;
//    }

//    public override bool? ShouldRecord(Activity activity)
//    {
//        return base.ShouldRecord(activity) ?? decoratee.ShouldRecord(activity);
//    }

//    //public override IEnumerable<KeyValuePair<string, object?>> ExtractTags(Activity activity)
//    //{
//    //    return base.ExtractTags(activity).Concat(decoratee.ExtractTags(activity));
//    //}
//}
