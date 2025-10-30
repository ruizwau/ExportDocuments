
using Amazon;
using Amazon.S3;
using DataExport.Core;
using DataExport.Core.Entities;
using DataExport.Core.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(AppContext.BaseDirectory);
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
        config.AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // Bind ExportOptions from config
        services.Configure<ExportOptions>(config.GetSection("Export"));

        // AWS S3 client
        var region = RegionEndpoint.GetBySystemName(config["Export:Region"] ?? "us-east-1");
        services.AddSingleton<IAmazonS3>(new AmazonS3Client(region));

        // SqlProvider that reads from SQL files (relative path configured)
        services.AddSingleton<ISqlProvider, FileSqlProvider>();

        // ExportManifestLogger with SqlProvider dependency
        services.AddSingleton<IExportManifestLogger>(provider => 
            new ExportManifestLogger(
                config["Export:ConnectionStringLog"] ?? throw new InvalidOperationException("ConnectionStringLog is required"), 
                provider.GetRequiredService<ISqlProvider>()
            ));

        // Main export service
        services.AddTransient<ExportService>();
    })
    .Build();

var service = host.Services.GetRequiredService<ExportService>();

// Optional: parse batch number from CLI or skip for all batches
int? batch = null;
if (args.Length > 0 && int.TryParse(args[0], out var parsed))
    batch = parsed;

await service.RunExportAsync(batch);