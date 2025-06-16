// Em CADCompanion.Agent/Program.cs
using CADCompanion.Agent.Configuration;
using CADCompanion.Agent.Services;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "CAD Companion Agent";
    })
    .UseSerilog((context, loggerConfig) =>
    {
        loggerConfig.ReadFrom.Configuration(context.Configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<CompanionConfiguration>(hostContext.Configuration.GetSection("CompanionConfiguration"));

        services.AddHttpClient<IApiCommunicationService, ApiCommunicationService>(client =>
        {
            var serverUrl = hostContext.Configuration["ServerBaseUrl"];
            if (string.IsNullOrEmpty(serverUrl))
            {
                throw new InvalidOperationException("A URL do servidor (ServerBaseUrl) não foi definida.");
            }
            client.BaseAddress = new Uri(serverUrl);
        });

        // Registra os serviços como Singleton (uma única instância para toda a aplicação)
        services.AddSingleton<IWorkDrivenMonitoringService, WorkDrivenMonitoringService>();
        services.AddSingleton<DocumentProcessingService>();
        services.AddSingleton<IInventorDocumentEventService, InventorDocumentEventService>();
        services.AddSingleton<WorkSessionService>();
        services.AddSingleton<InventorConnectionService>();
        services.AddSingleton<InventorBOMExtractor>();
        
        // Registra o CompanionWorkerService como o serviço de fundo principal
        services.AddHostedService<CompanionWorkerService>();
    })
    .Build();

await host.RunAsync();