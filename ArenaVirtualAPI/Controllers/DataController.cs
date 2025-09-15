using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ArenaVirtualAPI.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation($"[Sync] Recebida requisição de sincronização para o tipo: {modelTypeName}");
        _logger.LogDebug($"[Sync] JSON Recebido: {data.GetRawText()}");

        try {
            var idMapping = await _backendSyncService.ProcessAndMapItemsAsync(data, modelTypeName);
            _logger.LogDebug($"[Sync] JSON de Retorno: {JsonSerializer.Serialize(idMapping)}");
            return Ok(idMapping);
        } catch (Exception ex) {
            _logger.LogError($"[Sync] Erro interno na sincronização de upload para {modelTypeName}: {ex.Message}");
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    [HttpGet("updates")]
    public async Task<IActionResult> GetUpdates([FromQuery] DateTime lastSyncTime) {
        _logger.LogInformation($"[Updates] Requisição recebida. Última sincronização: {lastSyncTime}");

        try {
            var updatedItems = await _backendSyncService.GetUpdatesAsync(lastSyncTime);
            _logger.LogDebug($"[Updates] JSON de Resposta: {JsonSerializer.Serialize(updatedItems.UpdatedItems)}");
            return Ok(updatedItems.UpdatedItems);
        } catch (Exception ex) {
            _logger.LogError($"[Updates] Erro ao obter atualizações: {ex.Message}");
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }
}