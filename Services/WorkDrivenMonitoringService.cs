// Em Services/WorkDrivenMonitoringService.cs

using CADCompanion.Agent.Configuration; // <<--- A CORREÇÃO ESTÁ AQUI
using CADCompanion.Agent.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace CADCompanion.Agent.Services;

public class WorkDrivenMonitoringService : IWorkDrivenMonitoringService, IDisposable
{
    private readonly ILogger<WorkDrivenMonitoringService> _logger;
    private readonly DocumentProcessingService _documentProcessingService;
    private readonly CompanionSettings _settings;
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();

    public WorkDrivenMonitoringService(
        ILogger<WorkDrivenMonitoringService> logger,
        IOptions<CompanionConfiguration> configuration,
        DocumentProcessingService documentProcessingService)
    {
        _logger = logger;
        _settings = configuration.Value.Settings;
        _documentProcessingService = documentProcessingService;
        InitializeWatchers();
    }

    public void StartMonitoring()
    {
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = true;
        }
        _logger.LogInformation("Monitoramento de arquivos iniciado.");
    }

    public void StopMonitoring()
    {
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
        }
        _logger.LogInformation("Monitoramento de arquivos parado.");
    }
    
    public void Dispose()
    {
        foreach (var watcher in _watchers.Values)
        {
            watcher.Dispose();
        }
        _watchers.Clear();
        GC.SuppressFinalize(this);
    }

    private void InitializeWatchers()
    {
        if (_settings.MonitoredFolders == null || !_settings.MonitoredFolders.Any())
        {
            _logger.LogWarning("Nenhuma pasta configurada para monitoramento.");
            return;
        }

        foreach (var folder in _settings.MonitoredFolders)
        {
            if (Directory.Exists(folder.Path))
            {
                var watcher = new FileSystemWatcher(folder.Path)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    IncludeSubdirectories = folder.IncludeSubdirectories,
                    EnableRaisingEvents = false
                };
                watcher.Changed += OnFileChanged;
                _watchers.TryAdd(folder.Path, watcher);
            }
            else
            {
                _logger.LogWarning("A pasta configurada para monitoramento não existe: {Path}", folder.Path);
            }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.EndsWith(".iam", StringComparison.OrdinalIgnoreCase))
        {
            _documentProcessingService.EnqueueDocumentEvent(new DocumentEvent
            {
                Type = DocumentEventType.Saved,
                FilePath = e.FullPath,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}