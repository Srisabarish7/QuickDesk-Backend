using QuickDesk.worker.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Application.Interfaces
{
    public interface IJobItemValidator
    {
        ValidationResult Validate(QuickDeskWorkerItem jobItem);
    }

    public sealed class  ValidationResult
    {
        private ValidationResult() { }

        public bool IsValid { get; private init; }

        public IReadOnlyList<string> Errors { get; private init; } = [];
        public IReadOnlyList<string> Warnings { get; private init; } = [];

        public static ValidationResult Success(IEnumerable<string>? warnings = null) =>
            new() { IsValid = true, Warnings = (warnings ?? []).ToList().AsReadOnly() };

        public static ValidationResult Failure(IEnumerable<string> errors, IEnumerable<string>? warnings = null) => new() 
                  { 
                    IsValid = false, 
                    Errors = errors.ToList().AsReadOnly(), 
                    Warnings = (warnings ?? []).ToList().AsReadOnly() 
                  };
    }

    public interface IProcessJobItemUseCase
    {
        Task<ProcessingResult> ExecuteAsync(QuickDeskWorkerItem jobItem, CancellationToken ct);
    }

    public sealed class ProcessingResult
    {
        private ProcessingResult() { }
        public bool IsSuccess { get; private init; }
        public string? ErrorMessage { get; private init; }
        public static ProcessingResult Success() => new() { IsSuccess = true };
        public static ProcessingResult Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
