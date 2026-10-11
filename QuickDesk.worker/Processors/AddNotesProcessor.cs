using QuickDesk.worker.Application.Interfaces;
using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Infrastructure.Interfaces;
using QuickDesk.worker.Models;
using QuickDesk.worker.Models.Pipeline;
using QuickDesk.worker.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace QuickDesk.worker.Processors
{
    public sealed class AddNotesProcessor : BaseJobTypeProcessor
    {
        private readonly ILogger<AddNotesProcessor> _logger;
        private ValidateJobStep _validateJobStep;
        private JobRunningStep _jobRunStep;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IQueryService _queryService;
        private readonly AddEmailLogStep _addEmailLogStep;

        
        public AddNotesProcessor(ILogger<AddNotesProcessor> logger, ValidateJobStep validateJobStep, JobRunningStep jobRunStep, ILogger<BaseJobTypeProcessor> baseLogger, IJobValidateService jobValidateService, IEmailSenderService emailSenderService, IQueryService queryService, AddEmailLogStep addEmailLogStep) : base(baseLogger, jobValidateService)
        {
            _logger = logger;
            _validateJobStep = validateJobStep;
            _jobRunStep = jobRunStep;
            _emailSenderService = emailSenderService;
            _queryService = queryService;
            _addEmailLogStep = addEmailLogStep;
        }

        public override JobTypeEnum JobTypeId => JobTypeEnum.Remainder;

        public override async Task<bool> ProcessJobAsync(ProcessingContext jobItem, CancellationToken ct)
        {
            if (jobItem == null)
                return false;

            try
            {
                if(Enum.IsDefined(typeof(JobTypeEnum), (int)jobItem.JobTypeId))
                {
                    jobItem.JobType = (JobTypeEnum)jobItem.JobTypeId;
                }

                var validationResult = await ValidateJobStep(jobItem, ct);
                if (!validationResult.IsSuccess)
                {
                    return false;
                }

                var jobRunResult = await JobRunningStep(jobItem, ct);
                if (!jobRunResult.IsSuccess)
                {
                    return false;
                }

                await AddEmailLogStep(jobItem, ct);

                AddNotes? addNotes = null;

                if(jobItem.Details != null && jobItem.Details.HasValues)
                {
                    addNotes = jobItem.Details.ToObject<AddNotes>();
                }
                else if (!string.IsNullOrEmpty(jobItem.DetailsJson))
                {
                    addNotes = JsonSerializer.Deserialize<AddNotes>(jobItem.DetailsJson);
                }

                var userDetail = await _queryService.GetUserDetail(jobItem.UserId, ct);
                var emailBody = GenerateEmailBody(addNotes, userDetail.UserName);
                await _emailSenderService.SendEmailAsync(userDetail.Email, "QuickDesk - Notes Created Successfully.", emailBody, ct);
                return true;
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in AddNotesProcessor:ProcessJobAsync InputParameters: {jobItem}", jobItem);
                return false;
            }
        }

        private async Task<ProcessingResult> ValidateJobStep(ProcessingContext context, CancellationToken cancellationToken)
        {
            await _validateJobStep.ExecuteAsync(context, cancellationToken);
            if (context.Errors.Any())
            {
                return ProcessingResult.Failure(string.Join("; ", context.Errors));
            }
            return ProcessingResult.Success();
        }

        private async Task<ProcessingResult> JobRunningStep(ProcessingContext context, CancellationToken cancellationToken)
        {
            await _jobRunStep.ExecuteAsync(context, cancellationToken);
            if (context.Errors.Any())
            {
                return ProcessingResult.Failure(string.Join("; ", context.Errors));
            }
            return ProcessingResult.Success();
        }

        private string GenerateEmailBody(AddNotes addNotes, string userName)
        {
           string emailBody = $@"
            <html>
                <head>
                    <style>
                        .body{{
                            font-family: Arial, sans-serif;
                            padding: 20px;
                        }}
                        .email-Container{{
                            max-width: 600px;
                            background-color: #ffffff;
                            padding: 25px;
                            border-radius: 10px;            
                            border: 1px solid #ffffff;
                            box-shadow: rgba(0, 0, 0, 0.35) 0px 5px 15px;
                        }}
                        h1{{
                            color: #00008B;
                        }}
                        p {{
                            color: #333333;                            
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-Container'>
                        <h1>QuickDesk Notes</h1>                        
                        
                        <P>Hi, {userName}</p>

                        <p>Your note has been created successfully!</p>
                        
                        <hr />

                        <h3>Title - <span>{addNotes.Title}</span></h3>
                        
                        <h3>Decription - <span>{addNotes.Description}</span></h3>

                        <hr />

                        <p>Thank you for using QuickDesk.</p>
                </body>
            ";

            return emailBody;
        }

        private async Task<ProcessingResult> AddEmailLogStep(ProcessingContext context, CancellationToken cancellation)
        {
            await _addEmailLogStep.ExecuteAsync(context, cancellation);
            if (context.Errors.Any())
            {
                return ProcessingResult.Failure(string.Join("; ", context.Errors));
            }
            return ProcessingResult.Success();
        }
    }
}
