using Diginsight.Components.Azure;
using Diginsight.Components.Azure.Metrics;
using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Xunit;

namespace Diginsight.Components.Configuration.Tests;

public sealed class MetricRecorderTests
{
    [Fact]
    public void Standalone_recorders_construct_without_a_filter_or_enricher()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSpanDurationMetricRecorder();
        services.AddCosmosDbQueryCostMetricRecorder();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Assert.Null(serviceProvider.GetService<IMetricRecordingFilter>());
        Assert.Null(serviceProvider.GetService<IMetricRecordingEnricher>());
        Assert.NotNull(serviceProvider.GetRequiredService<SpanDurationMetricRecorder>());
        Assert.NotNull(serviceProvider.GetRequiredService<QueryCostMetricRecorder>());
    }

    [Fact]
    public void Recorders_emit_allowed_metrics_and_skip_denied_metrics_with_configured_tags()
    {
        Dictionary<string, string?> configuration = new()
        {
            ["Diginsight:Activities:SpanMeasuredActivityNames:SpanAllowed"] = "true",
            ["Diginsight:Activities:SpanMeasuredActivityNames:SpanDenied"] = "false",
            ["Diginsight:Activities:SpanMeasuredActivityNames:QueryAllowed"] = "false",
            ["Diginsight:Activities:SpanMeasuredActivityNames:QueryDenied"] = "true",
            ["Diginsight:Activities:MetricSpecificSpanMeasuredActivityNames:0:MetricName"] = "diginsight.query_cost",
            ["Diginsight:Activities:MetricSpecificSpanMeasuredActivityNames:0:ActivityNames:QueryAllowed"] = "true",
            ["Diginsight:Activities:MetricSpecificSpanMeasuredActivityNames:0:ActivityNames:QueryDenied"] = "false",
            ["Diginsight:Activities:MetricTags:0"] = "tenant",
            ["Diginsight:Activities:MetricSpecificTags:0:MetricName"] = "diginsight.query_cost",
            ["Diginsight:Activities:MetricSpecificTags:0:MetricTags:0"] = "query_dimension",
        };

        using ServiceProvider serviceProvider = MetricRecordingTestHost.BuildServiceProvider(configuration);
        SpanDurationMetricRecorder spanRecorder = serviceProvider.GetRequiredService<SpanDurationMetricRecorder>();
        QueryCostMetricRecorder queryCostRecorder = serviceProvider.GetRequiredService<QueryCostMetricRecorder>();
        List<Measurement> measurements = [];
        using MeterListener listener = CreateListener(measurements);

        using Activity spanAllowed = StartAndStopActivity(
            "SpanAllowed",
            activity => activity.SetTag("tenant", "tenant-a")
        );
        ((IActivityListenerLogic)spanRecorder).ActivityStopped(spanAllowed);

        using Activity spanDenied = StartAndStopActivity(
            "SpanDenied",
            activity => activity.SetTag("tenant", "tenant-b")
        );
        ((IActivityListenerLogic)spanRecorder).ActivityStopped(spanDenied);

        using Activity parent = new Activity("EntryMethod").Start();
        using Activity queryAllowed = StartAndStopActivity(
            "QueryAllowed",
            activity =>
            {
                activity.SetTag("query_cost", "2");
                activity.SetTag("tenant", "tenant-a");
                activity.SetTag("query_dimension", "point-read");
            }
        );
        ((IActivityListenerLogic)queryCostRecorder).ActivityStopped(queryAllowed);

        using Activity queryDenied = StartAndStopActivity(
            "QueryDenied",
            activity =>
            {
                activity.SetTag("query_cost", "3");
                activity.SetTag("tenant", "tenant-b");
                activity.SetTag("query_dimension", "cross-partition");
            }
        );
        ((IActivityListenerLogic)queryCostRecorder).ActivityStopped(queryDenied);

        Measurement spanMeasurement = Assert.Single(measurements, static measurement => measurement.InstrumentName == "diginsight.span_duration");
        Assert.Equal("SpanAllowed", spanMeasurement.Tags["span_name"]);
        Assert.Equal("tenant-a", spanMeasurement.Tags["tenant"]);

        Measurement queryMeasurement = Assert.Single(measurements, static measurement => measurement.InstrumentName == "diginsight.query_cost");
        Assert.Equal(2, queryMeasurement.Value);
        Assert.Equal("QueryAllowed", queryMeasurement.Tags["method"]);
        Assert.Equal("tenant-a", queryMeasurement.Tags["tenant"]);
        Assert.Equal("point-read", queryMeasurement.Tags["query_dimension"]);
    }

    private static MeterListener CreateListener(ICollection<Measurement> measurements)
    {
        MeterListener listener = new();
        listener.InstrumentPublished = static (instrument, meterListener) =>
        {
            if (instrument.Name is "diginsight.span_duration" or "diginsight.query_cost")
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<double>(
            (instrument, value, tags, _) => measurements.Add(
                new Measurement(
                    instrument.Name,
                    value,
                    tags.ToArray().ToDictionary(static tag => tag.Key, static tag => tag.Value)
                )
            )
        );
        listener.Start();
        return listener;
    }

    private static Activity StartAndStopActivity(string operationName, Action<Activity> configure)
    {
        Activity activity = new(operationName);
        activity.Start();
        configure(activity);
        activity.Stop();
        return activity;
    }

    private sealed record Measurement(
        string InstrumentName,
        double Value,
        IReadOnlyDictionary<string, object?> Tags
    );
}