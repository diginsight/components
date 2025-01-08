using Diginsight.Diagnostics;
using Diginsight.Diagnostics.AspNetCore;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Diginsight.Components.Configuration;

internal sealed class DecoratorHttpHeadersSpanDurationMetricRecorderSettings : HttpHeadersSpanDurationMetricRecorderSettings
{
    private readonly ISpanDurationMetricRecorderSettings decoratee;

    public DecoratorHttpHeadersSpanDurationMetricRecorderSettings(
        ISpanDurationMetricRecorderSettings decoratee, IHttpContextAccessor httpContextAccessor
    )
        : base(httpContextAccessor)
    {
        this.decoratee = decoratee;
    }

    public override bool? ShouldRecord(Activity activity)
    {
        return base.ShouldRecord(activity) ?? decoratee.ShouldRecord(activity);
    }

    public override IEnumerable<KeyValuePair<string, object?>> ExtractTags(Activity activity)
    {
        return base.ExtractTags(activity).Concat(decoratee.ExtractTags(activity));
    }
}
