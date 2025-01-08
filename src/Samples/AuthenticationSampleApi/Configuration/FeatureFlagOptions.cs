using Diginsight.Options;

namespace AuthenticationSampleApi;

public class FeatureFlagOptions : IDynamicallyConfigurable
{
    public bool TraceRequestBody { get; set; }
    public bool TraceResponseBody { get; set; }

}
