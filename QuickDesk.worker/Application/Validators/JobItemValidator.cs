using QuickDesk.worker.Application.Interfaces;
using QuickDesk.worker.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Application.Validators
{
    public sealed class JobItemValidator : IJobItemValidator
    {
        public ValidationResult Validate(QuickDeskWorkerItem jobItem)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (jobItem.JobId <= 0)
                errors.Add("JobId must be greater than Zero.");

            if (string.IsNullOrWhiteSpace(jobItem.JobName))
                errors.Add("JobName is required.");

            if (jobItem.JobTypeId <= 0)
                errors.Add("JobTypeId must be greater than zero.");

            if (jobItem.JobStatusId <= 0)
                errors.Add("JobStatus Id must be greater than zero.");

            if (string.IsNullOrEmpty(jobItem.DetailsJson))
                errors.Add("job Details should not be empty.");

            return errors.Count == 0 ? ValidationResult.Success(warnings) : ValidationResult.Failure(errors);
        }
    }
}
