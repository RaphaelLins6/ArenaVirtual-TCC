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
    public class EstatisticasPartidasController(IEstatisticaPartidaService estatisticaService) : ControllerBase {
        private readonly IEstatisticaPartidaService _estatisticaService = estatisticaService;

        // GET: api/estatisticaspartidas
        /// <summary>
        /// Obtém todas as estatísticas de partida.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstatisticaPartidaReadDto>>> GetEstatisticasPartidas() {
            var estatisticas = await _estatisticaService.GetAllAsync();
            return Ok(estatisticas.Select(MapToReadDto));
        }

        // GET: api/estatisticaspartidas/{id}
        /// <summary>
        /// Obtém uma estatística de partida pelo ID local.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EstatisticaPartidaReadDto>> GetEstatisticaPartida(int id) {
            var estatistica = await _estatisticaService.GetByIdAsync(id);

            if (estatistica == null) {
                return NotFound();
            }

            return Ok(MapToReadDto(estatistica));
        }

        // POST: api/estatisticaspartidas (Cria ou Atualiza via ClientAppId)
        /// <summary>
        /// Cria uma nova estatística ou atualiza uma existente (upsert) se o ClientAppId for fornecido.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EstatisticaPartidaReadDto>> PostEstatisticaPartida([FromBody] EstatisticaPartidaCreateUpdateDto dto) {
            // Gera um novo ClientAppId se for uma nova criação via API
            var clientAppId = dto.ClientAppId ?? Guid.NewGuid();

            var entity = new EstatisticaPartida {
                ClientAppId = clientAppId,
                UsuarioId = dto.UsuarioId,
                JogoId = dto.JogoId,
                TimeId = dto.TimeId,
                Pontos = dto.Pontos,
                Rebotes = dto.Rebotes,
                Assistencias = dto.Assistencias,
                Roubos = dto.Roubos,
                Bloqueios = dto.Bloqueios,
                Faltas = dto.Faltas,
                Turnovers = dto.Turnovers,
                Arremessos2PontosConvertidos = dto.Arremessos2PontosConvertidos,
                Arremessos2PontosTentados = dto.Arremessos2PontosTentados,
                Arremessos3PontosConvertidos = dto.Arremessos3PontosConvertidos,
                Arremessos3PontosTentados = dto.Arremessos3PontosTentados,
                LancesLivresConvertidos = dto.LancesLivresConvertidos,
                LancesLivresTentados = dto.LancesLivresTentados,
            };

            // Usa AddOrUpdateAsync para a lógica de upsert (criação ou atualização)
            var resultEntity = await _estatisticaService.AddOrUpdateAsync(entity);

            // Se foi uma operação de criação (novo ID gerado), retorna 201 Created
            if (resultEntity.Id > 0 && resultEntity.ClientAppId == clientAppId) {
                return CreatedAtAction(nameof(GetEstatisticaPartida), new { id = resultEntity.Id }, MapToReadDto(resultEntity));
            }

            // Se foi uma atualização de um item existente (upsert), retorna 200 OK
            return Ok(MapToReadDto(resultEntity));
        }

        // PUT: api/estatisticaspartidas/{id}
        /// <summary>
        /// Atualiza uma estatística de partida pelo ID local.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutEstatisticaPartida(int id, [FromBody] EstatisticaPartidaCreateUpdateDto dto) {
            var entity = await _estatisticaService.GetByIdAsync(id);

            if (entity == null) {
                return NotFound();
            }

            // Atualiza chaves estrangeiras (Ids Locais)
            entity.UsuarioId = dto.UsuarioId;
            entity.JogoId = dto.JogoId;
            entity.TimeId = dto.TimeId;

            // Atualiza estatísticas
            entity.Pontos = dto.Pontos;
            entity.Rebotes = dto.Rebotes;
            entity.Assistencias = dto.Assistencias;
            entity.Roubos = dto.Roubos;
            entity.Bloqueios = dto.Bloqueios;
            entity.Faltas = dto.Faltas;
            entity.Turnovers = dto.Turnovers;
            entity.Arremessos2PontosConvertidos = dto.Arremessos2PontosConvertidos;
            entity.Arremessos2PontosTentados = dto.Arremessos2PontosTentados;
            entity.Arremessos3PontosConvertidos = dto.Arremessos3PontosConvertidos;
            entity.Arremessos3PontosTentados = dto.Arremessos3PontosTentados;
            entity.LancesLivresConvertidos = dto.LancesLivresConvertidos;
            entity.LancesLivresTentados = dto.LancesLivresTentados;

            // Reutiliza o AddOrUpdateAsync para atualizar a entidade e marcar IsSynced=false
            await _estatisticaService.AddOrUpdateAsync(entity);

            return NoContent();
        }

        // DELETE: api/estatisticaspartidas/clientapp/{clientAppId}
        /// <summary>
        /// Remove uma estatística de partida pelo ClientAppId.
        /// </summary>
        [HttpDelete("clientapp/{clientAppId:Guid}")]
        public async Task<IActionResult> DeleteEstatisticaPartida(Guid clientAppId) {
            var deleted = await _estatisticaService.DeleteAsync(clientAppId);

            if (!deleted) {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/estatisticaspartidas/markassynced/{clientAppId}
        /// <summary>
        /// Marca uma estatística de partida como sincronizada.
        /// </summary>
        [HttpPost("markassynced/{clientAppId:Guid}")]
        public async Task<IActionResult> MarkAsSynced(Guid clientAppId) {
            var success = await _estatisticaService.MarkAsSyncedAsync(clientAppId);

            if (!success) {
                return NotFound();
            }

            return NoContent();
        }

        // ---------------------------------------------------------------------
        // Mapeamento DTO
        // ---------------------------------------------------------------------

        private static EstatisticaPartidaReadDto MapToReadDto(EstatisticaPartida e) => new() {
            Id = e.Id,
            ClientAppId = e.ClientAppId,
            UsuarioId = e.UsuarioId,
            JogoId = e.JogoId,
            TimeId = e.TimeId,
            Pontos = e.Pontos,
            Rebotes = e.Rebotes,
            Assistencias = e.Assistencias,
            Roubos = e.Roubos,
            Bloqueios = e.Bloqueios,
            Faltas = e.Faltas,
            Turnovers = e.Turnovers,
            Arremessos2PontosConvertidos = e.Arremessos2PontosConvertidos,
            Arremessos2PontosTentados = e.Arremessos2PontosTentados,
            Arremessos3PontosConvertidos = e.Arremessos3PontosConvertidos,
            Arremessos3PontosTentados = e.Arremessos3PontosTentados,
            LancesLivresConvertidos = e.LancesLivresConvertidos,
            LancesLivresTentados = e.LancesLivresTentados,
            UpdatedAt = e.UpdatedAt
        };
    }
}