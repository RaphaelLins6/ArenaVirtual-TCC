// Controllers/DataController.cs
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.Services;
using System;
using System.Linq; // Adicionado para os métodos de extensão Sum() e Count()

[ApiController]
[Route("api/[controller]")] // Isso define a rota base como /api/data
public class DataController : ControllerBase {
    private readonly BackendSyncService _backendSyncService;
    private readonly ILogger<DataController> _logger; // Para logging

    // Construtor com Injeção de Dependência
    public DataController(BackendSyncService backendSyncService, ILogger<DataController> logger) {
        _backendSyncService = backendSyncService;
        _logger = logger;
    }

    // Endpoint para sincronização de upload (enviar dados do app para a API)
    // A rota completa será /api/data/sync/{modelTypeName}
    // Recebe uma lista de objetos e o nome do tipo do modelo via URL
    [HttpPost("sync/{modelTypeName}")]
    public async Task<IActionResult> Sync([FromBody] JsonElement data, string modelTypeName) {
        if (data.ValueKind == JsonValueKind.Undefined || string.IsNullOrEmpty(modelTypeName)) {
            _logger.LogWarning("Requisição de sincronização inválida: dados ou nome do tipo ausentes.");
            return BadRequest("Dados ou nome do tipo de modelo ausentes.");
        }

        _logger.LogInformation($"Recebida requisição de sincronização para o tipo: {modelTypeName}");

        try {
            await _backendSyncService.ProcessUploadAsync(data, modelTypeName);
            return Ok($"Sincronização de upload de {modelTypeName} bem-sucedida.");
        } catch (JsonException ex) {
            _logger.LogError($"Erro de desserialização na sincronização de upload para {modelTypeName}: {ex.Message}");
            return BadRequest($"Erro no formato dos dados para {modelTypeName}: {ex.Message}");
        } catch (Exception ex) {
            _logger.LogError($"Erro interno na sincronização de upload para {modelTypeName}: {ex.Message}");
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    // Endpoint para sincronização de download (enviar dados da API para o app)
    // A rota completa será /api/data/updates
    [HttpGet("updates")]
    public async Task<IActionResult> GetUpdates([FromQuery] DateTime lastSyncTime) {
        _logger.LogInformation($"Requisição de atualizações recebida. Última sincronização: {lastSyncTime}");

        try {
            var updatedItems = await _backendSyncService.GetUpdatesAsync(lastSyncTime);
            var allUpdatedItems = new List<object>();

            // Itere sobre o dicionário e adicione todos os itens a uma única lista
            foreach (var keyValuePair in updatedItems.UpdatedItems) {
                allUpdatedItems.AddRange(keyValuePair.Value.Cast<object>());
            }

            // Retorna a lista de itens. Se a lista estiver vazia, a resposta JSON será `[]`.
            return Ok(allUpdatedItems);

        } catch (Exception ex) {
            _logger.LogError($"Erro ao obter atualizações: {ex.Message}");
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }
}
