using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Domain.Enum
{
    public enum JobStatusEnum
    {
        Scheduled = 1,
        Queued = 2,
        Running = 3,
        Completed = 4,
        Failed = 5
    }

    public enum JobTypeEnum
    {
        Unknown = 0,
        SendEmail = 1,
        Remainder = 2,
        ProcessingImage = 3,
        ExportEmail = 4
    }
}
