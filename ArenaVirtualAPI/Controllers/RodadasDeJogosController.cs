using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArenaVirtualAPI.Services;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using System.Linq;
using System;

namespace ArenaVirtualAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    // Injeta o serviço especializado no construtor
    public class RodadasDeJogosController(
        IBackendService<RodadaDeJogos, RodadaDeJogosSyncDto> rodadaService) : ControllerBase {
        private readonly IBackendService<RodadaDeJogos, RodadaDeJogosSyncDto> _rodadaService = rodadaService;

        // GET: api/rodadasdejogos/{id}
        /// <summary>
        /// Obtém uma Rodada de Jogos pelo ID local.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RodadaDeJogosReadDto>> GetRodadaDeJogos(int id) {
            var rodada = await _rodadaService.GetByIdAsync(id);

            if (rodada == null) {
                return NotFound();
            }

            return Ok(MapToReadDto(rodada));
        }

        // POST: api/rodadasdejogos
        /// <summary>
        /// Cria uma nova Rodada de Jogos diretamente pela API.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RodadaDeJogosReadDto>> PostRodadaDeJogos([FromBody] RodadaDeJogosCreateUpdateDto dto) {
            var entity = new RodadaDeJogos {
                NomeRodada = dto.NomeRodada,
                // O ClientAppId é gerado aqui, pois a API é a fonte.
                ClientAppId = Guid.NewGuid(),
                // As propriedades IsSynced e UpdatedAt são tratadas pelo Service
            };

            await _rodadaService.AddAsync(entity);

            // O ID local (entity.Id) será populado após o SaveChanges no AddAsync
            return CreatedAtAction(nameof(GetRodadaDeJogos), new { id = entity.Id }, MapToReadDto(entity));
        }

        // PUT: api/rodadasdejogos/{id}
        /// <summary>
        /// Atualiza uma Rodada de Jogos pelo ID local.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutRodadaDeJogos(int id, [FromBody] RodadaDeJogosCreateUpdateDto dto) {
            var entity = await _rodadaService.GetByIdAsync(id);

            if (entity == null) {
                return NotFound();
            }

            // Atualiza campos
            entity.NomeRodada = dto.NomeRodada;

            // O service cuidará de marcar como IsSynced=true e atualizar UpdatedAt
            await _rodadaService.UpdateAsync(entity);
            return NoContent();
        }

        // ---------------------------------------------------------------------
        // Endpoint de Sincronização (Upload do Cliente)
        // ---------------------------------------------------------------------

        // POST: api/rodadasdejogos/sync-upload
        /// <summary>
        /// Recebe e processa uma lista de DTOs de Rodadas de Jogos para sincronização (upsert do cliente).
        /// </summary>
        [HttpPost("sync-upload")]
        public async Task<ActionResult<Dictionary<Guid, int>>> SyncUpload([FromBody] IEnumerable<RodadaDeJogosSyncDto> dtos) {
            if (dtos == null || !dtos.Any()) {
                return BadRequest("Nenhum item enviado para sincronização.");
            }

            // O ProcessAndMapItemsAsync cuida do upsert e retorna o mapa ClientAppId -> Id
            var idMapping = await _rodadaService.ProcessAndMapItemsAsync(dtos);

            // RodadaDeJogos não possui FKs a serem resolvidas

            return Ok(idMapping);
        }

        // GET: api/rodadasdejogos/sync-download?lastSyncTime=2025-01-01T00:00:00Z
        /// <summary>
        /// Retorna todas as Rodadas de Jogos atualizadas desde o último horário de sincronização.
        /// </summary>
        [HttpGet("sync-download")]
        public async Task<ActionResult<IEnumerable<RodadaDeJogosSyncDto>>> SyncDownload([FromQuery] DateTime lastSyncTime) {
            var updatedItems = await _rodadaService.GetUpdatedSinceAsync(lastSyncTime);
            return Ok(updatedItems);
        }

        // ---------------------------------------------------------------------
        // Mapeamento DTO
        // ---------------------------------------------------------------------

        private static RodadaDeJogosReadDto MapToReadDto(RodadaDeJogos r) => new() {
            Id = r.Id,
            ClientAppId = r.ClientAppId,
            NomeRodada = r.NomeRodada,
            UpdatedAt = r.UpdatedAt
        };
    }
}