using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase {
    private readonly BackendSyncService _backendSyncService;
    private readonly ILogger<DataController> _logger;

    public DataController(BackendSyncService backendSyncService, ILogger<DataController> logger) {
        _backendSyncService = backendSyncService;
        _logger = logger;
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncAll([FromBody] AllUploadsDto data, [FromQuery] DateTime lastSyncTime) {
        _logger.LogInformation($"[Sync] Recebida requisição de sincronização completa.");

        try {
            var allUpdates = await _backendSyncService.SyncDataAsync(data, lastSyncTime);
            return Ok(allUpdates);
        } catch (Exception ex) {
            _logger.LogError($"[Sync] Erro interno na sincronização: {ex.Message}");
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    // Seu DataController.cs corrigido
    [HttpGet("updates")]
    public async Task<IActionResult> GetUpdates([FromQuery] DateTime lastSyncTime) {
        _logger.LogInformation($"[Updates] Requisição recebida. Última sincronização: {lastSyncTime}");

        try {
            // A chamada foi corrigida para passar apenas a data
            var updatedItems = await _backendSyncService.GetUpdatesAsync(lastSyncTime);
            _logger.LogDebug($"[Updates] JSON de Resposta: {JsonSerializer.Serialize(updatedItems)}");
            return Ok(updatedItems);
        } catch (Exception ex) {
            _logger.LogError($"[Updates] Erro ao obter atualizações: {ex.Message}");
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }
}