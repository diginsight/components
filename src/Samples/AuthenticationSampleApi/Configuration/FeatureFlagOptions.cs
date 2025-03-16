using Diginsight.Options;

namespace AuthenticationSampleApi;

public class FeatureFlagOptions : IDynamicallyConfigurable
{
    public int? MaxAge { get; set; }
}
