using Microsoft.Extensions.Options;
using QuickDesk.worker.Configuration;
using QuickDesk.worker.Infrastructure;
using QuickDesk.worker.Models;
using System.Diagnostics;
using System.Threading.Channels;

namespace QuickDesk.worker
{
    public sealed class QuickDeskWorker : BackgroundService
    {
        private readonly ILogger<QuickDeskWorker> _logger;
        private readonly QuickDeskOptions _options;
        private readonly ISqlQueueRepository _sqlQueueRepository;

        private readonly Channel<QuickDeskWorkerItem> _worker;
        private long _lastThroughputCount = 0;
        private TimeSpan _lastThroughputElapseTime;
        private readonly Stopwatch _throughputStopwatch = new Stopwatch();

        public QuickDeskWorker(ILogger<QuickDeskWorker> logger, IOptions<QuickDeskOptions> options, ISqlQueueRepository sqlQueueRepository)
        {
            _logger = logger;
            _options = options.Value;
            _sqlQueueRepository = sqlQueueRepository;

            if (_options.ThroughputLogging.Enabled)
            {
                _throughputStopwatch.Start();
                _lastThroughputCount = 0;
                _lastThroughputElapseTime = TimeSpan.Zero;
            }

            _worker = Channel.CreateBounded<QuickDeskWorkerItem>(
                new BoundedChannelOptions(_options.ChannelCapacity)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleWriter = false,
                    SingleReader = true
                });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QuickDesk Worker Started.");

            await ResetStuckItemsAsync(stoppingToken).ConfigureAwait(false);
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessQueueItemAsync(stoppingToken).ConfigureAwait(false);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(_options.PollingIntervalMs, stoppingToken);
            }
        }

        private async Task ResetStuckItemsAsync(CancellationToken cancellationToken)
        {
            try
            {
                int resetCount = await _sqlQueueRepository.ResetStuckItemsAsync(_options.StuckItemTimeoutMinutes, cancellationToken);
                if (resetCount > 0)
                {
                    _logger.LogInformation("Reset {resetCount} stuck items to pending status.", resetCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in QuickDeskWorker:ResetStuckItemsAsync");
            }
        }

        private async Task ProcessQueueItemAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Get Pending Items from the database
                var pendingItem = await _sqlQueueRepository.ClaimPendingItemsAsync(_options.BatchSize, cancellationToken);

                var pendingItemList = pendingItem.ToList();

                if(pendingItemList.Count == 0)
                {                    
                    _logger.LogDebug("No pending items found in the queue.");
                    return;
                }

                //Process the pending items
                if (pendingItemList.Count > 0) 
                {
                    foreach(var item in pendingItemList)
                    {
                        if(cancellationToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("Cancellation requested. Stopping processing of pending items.");
                            break;
                        }
                        // Process each item
                        _logger.LogInformation("Processing item with ID: {itemId}", item.JobId);
                        // Add your processing logic here
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in QuickDeskWorker:ProcessQueueItemAsync");
            }
        }
    }
}
