using Microsoft.Extensions.Options;
using QuickDesk.worker.Application.Interfaces;
using QuickDesk.worker.Configuration;
using QuickDesk.worker.Infrastructure.Interfaces;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly Channel<QuickDeskWorkerItem> _worker;
        private long _lastThroughputCount = 0;
        private TimeSpan _lastThroughputElapseTime;
        private readonly Stopwatch _throughputStopwatch = new Stopwatch();
        private readonly List<Task> _workers = [];

        public QuickDeskWorker(ILogger<QuickDeskWorker> logger, IOptions<QuickDeskOptions> options, ISqlQueueRepository sqlQueueRepository, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _options = options.Value;
            _sqlQueueRepository = sqlQueueRepository;
            _serviceScopeFactory = serviceScopeFactory;

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

            StartWorker(stoppingToken);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessQueueItemAsync(stoppingToken).ConfigureAwait(false);

                    await Task.Delay(_options.PollingIntervalMs, stoppingToken);
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    }
                    await Task.Delay(_options.PollingIntervalMs, stoppingToken);
                }
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Queue polling failed");
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
            finally
            {
                _worker.Writer.Complete();

                await Task.WhenAll(_workers);
                _logger.LogInformation("Quick Desk Worker has Stopped.");
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
            var sw = Stopwatch.StartNew();
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
                        await _worker.Writer.WriteAsync(item, cancellationToken);

                        _logger.LogInformation("Processing item with ID: {itemId}", item.JobId);
                        // Add your processing logic here
                    }
                }

                sw.Stop();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in QuickDeskWorker:ProcessQueueItemAsync");
            }
        }

        private void StartWorker(CancellationToken cancellationToken)
        {
            for(int i=0; i< _options.MaxDegreeOfParallelism; i++)
            {
                var workerId = i + 1;
                _workers.Add(
                    Task.Run(
                        () => ConsumeAsync(workerId, cancellationToken), cancellationToken));
            }
        }

        private async Task ConsumeAsync(int workerId, CancellationToken cancellationToken)
        {
            await foreach(var item in _worker.Reader.ReadAllAsync(cancellationToken))
            {
                await ProcessItemAsync(workerId, item, cancellationToken);
            }
        }

        private async Task ProcessItemAsync(int workerId, QuickDeskWorkerItem item, CancellationToken cancellation)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var useCase = scope.ServiceProvider.GetRequiredService<IProcessJobItemUseCase>();

                var result = await useCase.ExecuteAsync(item, cancellation);

                sw.Stop();

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Job Item Processed successfully | DurationMs: {ElapsedMs}ms | JobId: {JobId} | JobName: {JobName} | JobStatus: {JobStatus}",
                        sw.Elapsed.TotalMilliseconds,
                        item.JobId,
                        item.JobName,
                        item.JobStatus);
                }
                else
                {
                     _logger.LogError("Job Item Failed | DurationMs: {ElapsedMs}ms | JobId: {JobId} | JobName: {JobName} | JobStatus: {JobStatus}",
                        sw.Elapsed.TotalMilliseconds,
                        item.JobId,
                        item.JobName,
                        item.JobStatus);
                }
            }catch(Exception ex)
            {
                sw.Stop();
                var inputParams = new { workerId, item };
                _logger.LogError(ex, "Exception thrown in QuickDeskWorker: ProcessItemAsync Input Parameters: {inputParams}", inputParams);
            }
        }
    }
}
