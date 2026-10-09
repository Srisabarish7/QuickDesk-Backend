using Newtonsoft.Json.Linq;
using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Models;
using QuickDesk.worker.Models.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Dispatching
{
    public sealed class ProcessorDispatcher
    {
        private readonly IProcessorRegistry _processorRegistry;
        private readonly ILogger<ProcessorDispatcher> _logger;

        public ProcessorDispatcher(IProcessorRegistry processorRegistry, ILogger<ProcessorDispatcher> logger)
        {
            _processorRegistry = processorRegistry;
            _logger = logger;
        }

        public async Task<bool> DispatchAsync(QuickDeskWorkerItem jobItem, CancellationToken cancellationToken)
        {
            var jobType = Classify(jobItem.JobTypeId);
            if(jobType == JobTypeEnum.Unknown)
            {
                _logger.LogWarning("Unknown job type {JobTypeId} for job {JobId}", jobItem.JobTypeId, jobItem.JobId);
                return false;
            }
            var processor = _processorRegistry.GetProcessor(jobType);
            if (processor is null)
            {
                _logger.LogWarning("No processor found for job type {JobTypeId}", jobItem.JobTypeId);
                return false;
            }

            var context = new ProcessingContext(jobItem)
            {
                JobType = jobType,
                Details = TryParseDetails(jobItem.DetailsJson),
            };
            return await processor.ProcessJobAsync(context, cancellationToken);
        }

        private static JobTypeEnum Classify(long JobTypeId)
        {
            if (IsEmail(JobTypeId)) return JobTypeEnum.SendEmail;
            if (IsRemainder(JobTypeId)) return JobTypeEnum.Remainder;
            if (IsProcessingImage(JobTypeId)) return JobTypeEnum.ProcessingImage;
            if (IsExportEmail(JobTypeId)) return JobTypeEnum.ExportEmail;
            return JobTypeEnum.Unknown;
        }

        private static bool IsEmail(long jobTypeId) => jobTypeId == (long)JobTypeEnum.SendEmail;
        private static bool IsRemainder(long jobTypeId) => jobTypeId == (long)JobTypeEnum.Remainder;
        private static bool IsProcessingImage(long jobTypeId) => jobTypeId == (long)JobTypeEnum.ProcessingImage;
        private static bool IsExportEmail(long jobTypeId) => jobTypeId == (long)JobTypeEnum.ExportEmail;

        private static JObject TryParseDetails(string? detailsJson)
        {
            if (string.IsNullOrWhiteSpace(detailsJson))
            {
                return new JObject();
            }
            try
            {
                return JObject.Parse(detailsJson);
            }
            catch
            {                
                return new JObject();
            }
        }
    }
}
