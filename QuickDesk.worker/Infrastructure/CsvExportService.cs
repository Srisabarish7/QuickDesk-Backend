using QuickDesk.worker.Infrastructure.Interfaces;
using QuickDesk.worker.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure
{
    public sealed class CsvExportService : ICsvExportService
    {
        private readonly ILogger<CsvExportService> _logger;
        private readonly IConfiguration _configuration;

        public CsvExportService(ILogger<CsvExportService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> ExportEmailLogsAsync(IEnumerable<EmailLogs> emailLogs,string userName, long jobId, CancellationToken ct)
        {
            try
            {
                var path = _configuration.GetSection("ExportSettings");
                var directory = path["OutputDirectory"];

                if (string.IsNullOrWhiteSpace(directory))
                {
                    throw new InvalidOperationException("Export directory is not Configured");

                }

                var outputDirectory = Path.GetFullPath(directory);

                Directory.CreateDirectory(outputDirectory);
                var filePath = Path.Combine(outputDirectory, $"EmailReport_{userName}_{jobId}.csv");

                await using var stream = new FileStream(
                        filePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 4096,
                        useAsync: true
                    );

                await using var streamWriter = new StreamWriter(
                        stream,
                        new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
                    );

                await streamWriter.WriteLineAsync(
                    "EmailLogId,UserId,JobId,JobName,Recipient,Subject,Title,Description,Status,SentAt,ErrorMessage,CreatedAt"
                    .AsMemory(), cancellationToken: ct
                    );

                foreach (var log in emailLogs)
                {
                    ct.ThrowIfCancellationRequested();

                    var row = string.Join(",",
                        EscapeCSV(log.EmailLogId.ToString()),
                        EscapeCSV(log.UserId.ToString()),
                        EscapeCSV(log.JobId.ToString()),
                        EscapeCSV(log.JobName),
                        EscapeCSV(log.Recipient),
                        EscapeCSV(log.Subject),
                        EscapeCSV(log.Title),
                        EscapeCSV(log.Description),
                        EscapeCSV(log.Status),
                        EscapeCSV(log.SentAt?.ToString("O")),
                        EscapeCSV(log.ErrorMessage),
                        EscapeCSV(log.CreatedAt?.ToString("O")));

                    await streamWriter.WriteLineAsync(
                        row.AsMemory(), cancellationToken: ct);
                }

                await streamWriter.FlushAsync(ct);

                _logger.LogInformation("Email report generated for JobId {JobId} at {FilePath}", jobId, filePath);

                return filePath;
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in CsvExportService:ExportEmailLogsAsync JobId:{jobId}", jobId);
                throw;
            }           
        }

        private static string EscapeCSV(string? value)
        {
            value ??= string.Empty;

            if (value.Length > 0 &&
                (value[0] == '=' ||
                value[0] == '+' ||
                value[0] == '-' ||
                value[0] == '@' ||
                value[0] == '\t' ||
                value[0] == '\r'))
            {
                value = "'" + value;
            }

            value = value.Replace("\"", "\"\"");

            if(value.Contains(',') || value.Contains('\n') || value.Contains('\r') || value.Contains('"'))
            {
                value = $"\"{value}\"";
            }

            return value;
        }
    }
}
