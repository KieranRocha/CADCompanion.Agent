// Em CADCompanion.Agent/Services/ApiCommunicationService.cs

using CADCompanion.Agent.Models;
using CADCompanion.Shared.Contracts;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CADCompanion.Agent.Services;

public class ApiCommunicationService : IApiCommunicationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiCommunicationService> _logger;

    public ApiCommunicationService(HttpClient httpClient, ILogger<ApiCommunicationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // Método principal que envia a BOM para o servidor
    public async Task<bool> SubmitBomAsync(BomSubmissionDto bomData)
    {
        try
        {
            _logger.LogInformation("Enviando BOM para o servidor: {FilePath}", bomData.AssemblyFilePath);
            // Este é o endpoint correto que criamos no servidor
            var response = await _httpClient.PostAsJsonAsync("api/boms/submit", bomData);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("BOM enviada com sucesso para {FilePath}.", bomData.AssemblyFilePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha crítica ao enviar BOM para o servidor.");
            return false;
        }
    }

    // --- Implementações de placeholder para os outros métodos da interface ---
    // Substitua os TODOs pela lógica real de chamada à API

    public async Task SendBOMDataAsync(BOMDataWithContext bomData)
    {
        _logger.LogWarning("Tentativa de chamada ao método obsoleto SendBOMDataAsync. A lógica deve usar SubmitBomAsync.");
        // TODO: Mapear BOMDataWithContext para BomSubmissionDto e chamar SubmitBomAsync
        await Task.CompletedTask;
    }

    public async Task SendDocumentActivityAsync(DocumentEvent documentEvent)
    {
        _logger.LogInformation("Enviando atividade de documento para {FilePath}", documentEvent.FilePath);
        // TODO: Implementar POST para o endpoint /api/activity/log
        await _httpClient.PostAsJsonAsync("api/activity/log", documentEvent);
    }

    public async Task SendHeartbeatAsync()
    {
        _logger.LogInformation("Enviando Heartbeat.");
        // TODO: Implementar POST para o endpoint /api/session/heartbeat
        await _httpClient.PostAsJsonAsync("api/session/heartbeat", new {});
    }

    public async Task SendPartDataAsync(object partData)
    {
        _logger.LogInformation("Enviando dados de peça.");
        // TODO: Implementar POST para o endpoint /api/parts/submit
        await _httpClient.PostAsJsonAsync("api/parts/submit", partData);
    }

    public async Task SendWorkSessionEndedAsync(WorkSession session)
    {
        _logger.LogInformation("Enviando fim da sessão de trabalho.");
        // TODO: Implementar POST para o endpoint /api/session/end
        await _httpClient.PostAsJsonAsync("api/session/end", session);
    }

    public async Task SendWorkSessionUpdatedAsync(WorkSession session)
    {
        _logger.LogInformation("Enviando atualização da sessão de trabalho.");
        // TODO: Implementar POST para o endpoint /api/session/update
        await _httpClient.PostAsJsonAsync("api/session/update", session);
    }
}