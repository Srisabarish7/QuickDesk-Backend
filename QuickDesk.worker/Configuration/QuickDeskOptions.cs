using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Configuration
{
    public sealed class QuickDeskOptions
    {
        public const string SectionName = "QuickDesk";
        public int BatchSize { get; set; } = 10;
        public int MaxDegreeOfParallelism { get; set; } = 4;
        public int PollingIntervalMs { get; set; } = 500;
        public int StuckItemTimeoutMinutes { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
        public int BaseRetryDelaySeconds { get; set; } = 2;
        public int ChannelCapacity { get; set; } = 1000;
        public ThroughputLoggingOptions ThroughputLogging { get; set; } = new();
    }

    public sealed class ThroughputLoggingOptions
    {
        public bool Enabled { get; set; } = true;
        public int IntervalSeconds { get; set; } = 60;
    }
}
