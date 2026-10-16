using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QuickDesk.Application.ResponseDtos;

namespace QuickDesk.Api.Hubs
{
    public class JobHub : Hub
    {
        private readonly ILogger<JobHub> _logger;

        public JobHub(ILogger<JobHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("SignalR Clinet Connected, ConnectionId {ConnectionId}", Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation(
                exception,
                "SignalR client disconnected. ConnectionId: {ConnectionId}",
                Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinJobGroup(long jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"JobId - {jobId}");
            _logger.LogInformation("SignalR client joined group for JobId {JobId}, ConnectionId {ConnectionId}", jobId, Context.ConnectionId);
        }

        public async Task LeaveJobGroup(long jobId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"JobId - {jobId}");
            _logger.LogInformation("SignalR client left group for JobId {JobId}, ConnectionId {ConnectionId}", jobId, Context.ConnectionId);
        }
        
        public async Task PublishJobUpdate(JobUpdateDto jobUpdateDto)
        {
            await Clients.Group($"JobId - {jobUpdateDto.JobId}").SendAsync("JobStatusUpdated", jobUpdateDto);
        }    
    }
}
