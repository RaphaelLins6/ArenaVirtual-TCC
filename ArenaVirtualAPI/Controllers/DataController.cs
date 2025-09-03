using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ArenaVirtualAPI.Services;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase {
    private readonly BackendSyncService _backendSyncService;
    private readonly ILogger<DataController> _logger;

    public DataController(BackendSyncService backendSyncService, ILogger<DataController> logger) {
        _backendSyncService = backendSyncService;
        _logger = logger;
    }

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

    [HttpGet("updates")]
    public async Task<IActionResult> GetUpdates([FromQuery] DateTime lastSyncTime) {
        _logger.LogInformation($"Requisição de atualizações recebida. Última sincronização: {lastSyncTime}");

        try {
            var updatedItems = await _backendSyncService.GetUpdatesAsync(lastSyncTime);
            return Ok(updatedItems.UpdatedItems);
        } catch (Exception ex) {
            _logger.LogError($"Erro ao obter atualizações: {ex.Message}");
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }
}
