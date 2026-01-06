using Diginsight.Diagnostics;
using Diginsight.Diagnostics.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Diginsight.Components.Configuration;

internal sealed class DecoratorHttpHeadersSpanDurationMetricRecordingFilter : HttpHeadersSpanDurationMetricRecordingFilter, IMetricRecordingFilter
{
    private readonly IMetricRecordingFilter decoratee;

    public DecoratorHttpHeadersSpanDurationMetricRecordingFilter(
        IMetricRecordingFilter decoratee, 
        IHttpContextAccessor httpContextAccessor,
        IOptions<DiginsightActivitiesOptions> activitiesOptions
    )
        : base(httpContextAccessor, activitiesOptions)
    {
        this.decoratee = decoratee;
    }

    public override bool? ShouldRecord(Activity activity, Instrument instrument)
    {
        return base.ShouldRecord(activity, instrument) ?? decoratee.ShouldRecord(activity, instrument);
    }

    //public override IEnumerable<KeyValuePair<string, object?>> ExtractTags(Activity activity)
    //{
    //    return base.ExtractTags(activity).Concat(decoratee.ExtractTags(activity));
    //}
}
