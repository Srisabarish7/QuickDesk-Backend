using Microsoft.AspNetCore.SignalR.Client;
using QuickDesk.Application.ResponseDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure
{
    public sealed class SignalRConnectionService : BackgroundService
    {
        private readonly ILogger<SignalRConnectionService> _logger;
        private readonly HubConnection _hubConnection;

        public SignalRConnectionService(ILogger<SignalRConnectionService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _hubConnection = new HubConnectionBuilder()
                            .WithUrl(configuration["SignalRConnection:HubUrl"] ?? throw new ArgumentNullException())
                            .WithAutomaticReconnect()
                            .Build();

            _hubConnection.Reconnecting += exception =>
            {
                _logger.LogWarning(exception, "SignalRConnection lost, Attempting to reconnect");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId =>
            {
                _logger.LogInformation("SignalR reconnected ConnectionId:{connectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.Closed += exception =>
            {
                _logger.LogWarning(exception, "SignalR Connection was closed.");
                return Task.CompletedTask;
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await EnsureConnectionAsync(stoppingToken);

                await Task.Delay(
                        TimeSpan.FromSeconds(3), stoppingToken);                    
            }
        }

        public async Task PublishJobUpdateAsync(JobUpdateDto updateDto, CancellationToken cancellationToken)
        {
            try
            {
                await EnsureConnectionAsync(cancellationToken);
                await _hubConnection.InvokeAsync("PublishJobUpdate", updateDto, cancellationToken);

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SignalRConnectionService:PublishJobUpdateAsync");
                throw;
            }                
        }

        private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StartAsync(cancellationToken);
                    _logger.LogInformation("Connected to signalR Hub");
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown in SignalRConnectionService:EnsureConnectionAsync");
                    throw;
                }
            }
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            if(_hubConnection.State != HubConnectionState.Disconnected)
            {
                await _hubConnection.StopAsync(cancellationToken);
            }
        }

        public override void Dispose()
        {
            _hubConnection.DisposeAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();

            base.Dispose();
        }
    }
}
