using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Processors
{
    public sealed class SendEmailProcessor : BaseJobTypeProcessor
    {
        public override JobTypeEnum Handles => JobTypeEnum.SendEmail;

        private readonly ValidateJobStep _validateJobStep;
        private readonly JobRunningStep _jobRunningStep;

        public SendEmailProcessor(ValidateJobStep validateJobStep,  JobRunningStep jobRunningStep)
        {            
            _validateJobStep = validateJobStep;
            _jobRunningStep = jobRunningStep;
        }
    }
}
