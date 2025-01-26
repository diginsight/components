using Diginsight.Options;

namespace AuthenticationSampleServerApi;

public class FeatureFlagOptions : IDynamicallyConfigurable
{
    public bool TraceRequestBody { get; set; }
    public bool TraceResponseBody { get; set; }

}
