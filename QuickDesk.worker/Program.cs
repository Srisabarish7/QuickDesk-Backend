using QuickDesk.worker;
using QuickDesk.worker.Configuration;
using Serilog;

public class Program
{
    public static async Task Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        try
        {
            var builder = CreateHostBuilder(args).Build();
            Log.Information("Starting QuickDesk-worker service...");
            Log.Information("QuickDesk-worker v{Version} starting on {Machine}", typeof(Program).Assembly.GetName().Version?.ToString(), Environment.MachineName);

            ValidateConfiguration(builder.Services);


            await builder.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while starting the QuickDesk-worker service.");
            await Log.CloseAndFlushAsync();
            Environment.Exit(1);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseWindowsService()
        .UseSerilog((context, services, configuration) => 
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("ApplicationName", "QuickDesk-worker")
                .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version?.ToString() ?? "Unknown");
        })
        .ConfigureServices((hostContext, services) =>
        {
            // Configuration
            services.Configure<QuickDeskOptions>
            (hostContext.Configuration.GetSection(QuickDeskOptions.SectionName));

            services.AddHostedService<QuickDeskWorker>();
        });

    private static void ValidateConfiguration(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.GetConnectionString("QuickDesk")))
        {
            errors.Add("Connection string 'QuickDesk' is missing or empty.");
        }

        var opts = config.GetSection(QuickDeskOptions.SectionName).Get<QuickDeskOptions>();
        if (opts is not null) 
        {
            if(opts.BatchSize is < 1 or > 100) 
            {
                errors.Add($"BatchSize must be between 1 and 100. Current value: {opts.BatchSize}");
            }
            if(opts.MaxDegreeOfParallelism is < 1 or > 50)
            {
                errors.Add($"MaxDegreeOfParallelism must be between 1 and 10. Current value: {opts.MaxDegreeOfParallelism}");
            }
            if(opts.PollingIntervalMs < 500)
            {
                errors.Add($"PollingIntervalMs must be at least 500 ms. Current value: {opts.PollingIntervalMs}");
            }
            if(opts.StuckItemTimeoutMinutes < 1)
            {
                errors.Add($"StuckItemTimeoutMinutes must be at least 1 minute. Current value: {opts.StuckItemTimeoutMinutes}");
            }
        }

        if(errors.Any())
        {
            var errorMessage = "Configuration validation failed:\n" + string.Join("\n", errors);
            Log.Error(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        Log.Information("Configuration validation passed successfully.");
    }
}
