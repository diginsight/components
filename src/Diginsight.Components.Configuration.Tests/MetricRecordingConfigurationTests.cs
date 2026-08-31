using Diginsight.Components.Azure.Metrics;
using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Xunit;

namespace Diginsight.Components.Configuration.Tests;

public sealed class MetricRecordingConfigurationTests
{
    [Fact]
    public void AddObservability_registers_one_standard_policy_and_both_recorders()
    {
        using ServiceProvider serviceProvider = MetricRecordingTestHost.BuildServiceProvider();

        Assert.IsType<OptionsBasedMetricRecordingFilter>(serviceProvider.GetRequiredService<IMetricRecordingFilter>());
        Assert.Single(serviceProvider.GetServices<IMetricRecordingFilter>());
        Assert.IsType<OptionsBasedMetricRecordingEnricher>(serviceProvider.GetRequiredService<IMetricRecordingEnricher>());
        Assert.Single(serviceProvider.GetServices<IMetricRecordingEnricher>());
        Assert.NotNull(serviceProvider.GetRequiredService<SpanDurationMetricRecorder>());
        Assert.NotNull(serviceProvider.GetRequiredService<QueryCostMetricRecorder>());
    }

    [Fact]
    public void AddObservability_preserves_prior_standard_policy_registrations()
    {
        IMetricRecordingFilter customFilter = Substitute.For<IMetricRecordingFilter>();
        IMetricRecordingEnricher customEnricher = Substitute.For<IMetricRecordingEnricher>();

        using ServiceProvider serviceProvider = MetricRecordingTestHost.BuildServiceProvider(
            configureBeforeObservability: services =>
            {
                services.AddSingleton(customFilter);
                services.AddSingleton(customEnricher);
            }
        );

        Assert.Same(customFilter, serviceProvider.GetRequiredService<IMetricRecordingFilter>());
        Assert.Single(serviceProvider.GetServices<IMetricRecordingFilter>());
        Assert.Same(customEnricher, serviceProvider.GetRequiredService<IMetricRecordingEnricher>());
        Assert.Single(serviceProvider.GetServices<IMetricRecordingEnricher>());
    }

    [Theory]
    [InlineData("MetricSpecificSpanMeasuredActivityNames")]
    [InlineData("MetricSpecificTags")]
    public void AddObservability_rejects_a_blank_metric_name(string sectionName)
    {
        Dictionary<string, string?> configuration = new()
        {
            [$"Diginsight:Activities:{sectionName}:0:MetricName"] = "   ",
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => MetricRecordingTestHost.CreateServices(configuration)
        );

        Assert.Contains($"Diginsight:Activities:{sectionName}", exception.Message, StringComparison.Ordinal);
        Assert.Contains("MetricName", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Filter_uses_metric_specific_rules_before_default_rules()
    {
        Dictionary<string, string?> configuration = new()
        {
            ["Diginsight:Activities:SpanMeasuredActivityNames:DefaultAllowed"] = "true",
            ["Diginsight:Activities:SpanMeasuredActivityNames:DefaultDenied"] = "false",
            ["Diginsight:Activities:SpanMeasuredActivityNames:SpecificAllowed"] = "false",
            ["Diginsight:Activities:SpanMeasuredActivityNames:SpecificDenied"] = "true",
            ["Diginsight:Activities:MetricSpecificSpanMeasuredActivityNames:0:MetricName"] = "diginsight.query_cost",
            ["Diginsight:Activities:MetricSpecificSpanMeasuredActivityNames:0:ActivityNames:SpecificAllowed"] = "true",
            ["Diginsight:Activities:MetricSpecificSpanMeasuredActivityNames:0:ActivityNames:SpecificDenied"] = "false",
            ["Diginsight:Activities:MetricSpecificSpanMeasuredActivityNames:0:ActivityNames:Unrelated"] = "true",
        };

        using ServiceProvider serviceProvider = MetricRecordingTestHost.BuildServiceProvider(configuration);
        IMetricRecordingFilter filter = serviceProvider.GetRequiredService<IMetricRecordingFilter>();
        IOptionsMonitor<OptionsBasedMetricRecordingFilterOptions> optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<OptionsBasedMetricRecordingFilterOptions>>();
        using Meter meter = new("MetricRecordingConfigurationTests.Filter");
        Counter<long> queryCost = meter.CreateCounter<long>("diginsight.query_cost");
        Counter<long> otherMetric = meter.CreateCounter<long>("other_metric");

        Assert.True(optionsMonitor.CurrentValue.ActivityNames["DefaultAllowed"]);
        Assert.True(optionsMonitor.Get("diginsight.query_cost").ActivityNames["SpecificAllowed"]);
        Assert.True(filter.ShouldRecord(new Activity("DefaultAllowed"), queryCost));
        Assert.False(filter.ShouldRecord(new Activity("DefaultDenied"), queryCost));
        Assert.True(filter.ShouldRecord(new Activity("SpecificAllowed"), queryCost));
        Assert.False(filter.ShouldRecord(new Activity("SpecificDenied"), queryCost));
        Assert.False(filter.ShouldRecord(new Activity("NoMatch"), queryCost));
        Assert.False(filter.ShouldRecord(new Activity("SpecificAllowed"), otherMetric));
    }

    [Fact]
    public void Enricher_unions_specific_and_default_tags_without_leakage_or_duplicates()
    {
        Dictionary<string, string?> configuration = new()
        {
            ["Diginsight:Activities:MetricTags:0"] = "default_tag",
            ["Diginsight:Activities:MetricTags:1"] = "shared_tag",
            ["Diginsight:Activities:MetricTags:2"] = "missing_tag",
            ["Diginsight:Activities:MetricSpecificTags:0:MetricName"] = "diginsight.query_cost",
            ["Diginsight:Activities:MetricSpecificTags:0:MetricTags:0"] = "specific_tag",
            ["Diginsight:Activities:MetricSpecificTags:0:MetricTags:1"] = "shared_tag",
            ["Diginsight:Activities:MetricSpecificTags:1:MetricName"] = "other_metric",
            ["Diginsight:Activities:MetricSpecificTags:1:MetricTags:0"] = "other_tag",
        };

        using ServiceProvider serviceProvider = MetricRecordingTestHost.BuildServiceProvider(configuration);
        IMetricRecordingEnricher enricher = serviceProvider.GetRequiredService<IMetricRecordingEnricher>();
        IOptionsMonitor<OptionsBasedMetricRecordingEnricherOptions> optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<OptionsBasedMetricRecordingEnricherOptions>>();
        using Meter meter = new("MetricRecordingConfigurationTests.Enricher");
        Counter<long> queryCost = meter.CreateCounter<long>("diginsight.query_cost");
        Counter<long> otherMetric = meter.CreateCounter<long>("other_metric");
        using Activity parent = new Activity("Parent").Start();
        parent.SetTag("default_tag", "from-parent");
        parent.SetTag("shared_tag", "shared-from-parent");
        using Activity child = new Activity("Child").Start();
        child.SetTag("specific_tag", "from-child");
        child.SetTag("shared_tag", "shared-from-child");
        child.SetTag("other_tag", "other-from-child");

        Dictionary<string, object?> queryTags = enricher.ExtractTags(child, queryCost)
            .ToDictionary(static tag => tag.Key, static tag => tag.Value);
        Dictionary<string, object?> otherTags = enricher.ExtractTags(child, otherMetric)
            .ToDictionary(static tag => tag.Key, static tag => tag.Value);

        Assert.Contains("default_tag", optionsMonitor.CurrentValue.MetricTags);
        Assert.Contains("specific_tag", optionsMonitor.Get("diginsight.query_cost").MetricTags);
        Assert.Equal("from-parent", queryTags["default_tag"]);
        Assert.Equal("from-child", queryTags["specific_tag"]);
        Assert.Equal("shared-from-child", queryTags["shared_tag"]);
        Assert.DoesNotContain("missing_tag", queryTags.Keys);
        Assert.DoesNotContain("other_tag", queryTags.Keys);
        Assert.Equal("other-from-child", otherTags["other_tag"]);
        Assert.DoesNotContain("specific_tag", otherTags.Keys);
    }
}