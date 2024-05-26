#region using
using Azure.Messaging.EventHubs;
using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
#endregion

namespace KustoSample
{
    public partial class LogstringHelper
    {
        // Azure.Messaging.EventHubs
        // EventData, EventHubConnection, EventHubConnectionOptions, EventHubProperties, EventHubsConnectionStringProperties, EventHubsModelFactory, EventHubsRetryOptions, EventHubsRetryPolicy, PartitionProperties
        public static string ToLogStringInternal(EventData pthis)
        {
            string logString = $"{{{pthis.GetType().Name}:{{EnqueuedTime:{pthis.EnqueuedTime},PartitionKey:{pthis.PartitionKey},Offset:{pthis.Offset},EventBody:{pthis.EventBody},SequenceNumber:{pthis.SequenceNumber},SystemProperties:{pthis.SystemProperties}}}}}";
            return logString;
        }

        // Azure.Messaging.EventHubs.Consumer
        // EventHubConsumerClient, EventHubConsumerClientOptions, EventPosition, LastEnqueuedEventProperties, PartitionContext, PartitionEvent, ReadEventOptions
        public static string ToLogStringInternal(EventProcessorClient pthis)
        {
            string logString = $"{{{pthis.GetType().Name}:{{EventHubName:{pthis.EventHubName},ConsumerGroup:{pthis.ConsumerGroup},FullyQualifiedNamespace:{pthis.FullyQualifiedNamespace},IsRunning:{pthis.IsRunning},Identifier:{pthis.Identifier}}}}}";
            return logString;
        }

        // Azure.Messaging.EventHubs.Primitives 
        // EventProcessorClientOptions, EventProcessor<TPartition>, EventProcessorCheckpoint, EventProcessorOptions, EventProcessorPartition, EventProcessorPartitionOwnership, PartitionReceiver, PartitionContext, PartitionEvent, ReadEventOptions, PartitionReceiverOptions

        // Azure.Messaging.EventHubs.Processor
        // PartitionClosingEventArgs, PartitionInitializingEventArgs, ProcessErrorEventArgs, ProcessEventArgs

        // Azure.Messaging.EventHubs.Producer
        //CreateBatchOptions, EventDataBatch, EventHubProducerClient, EventHubProducerClientOptions, SendEventOptions

    }
}
