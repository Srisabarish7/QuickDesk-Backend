using QuickDesk.worker.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Dispatching
{
    public interface IProcessorRegistry
    {      
        IJobTypeProcessor? GetProcessor(JobTypeEnum jobType);
    }
    public class ProcessorRegistry : IProcessorRegistry
    {
        private readonly Dictionary<JobTypeEnum, IJobTypeProcessor> _processors;

        public ProcessorRegistry(IEnumerable<IJobTypeProcessor> processors)
        {
            _processors = processors.ToDictionary(processor => processor.JobTypeId);
        }

        public IJobTypeProcessor? GetProcessor(JobTypeEnum jobType)
            => _processors.TryGetValue(jobType, out var processor) ? processor : null;
    }
}