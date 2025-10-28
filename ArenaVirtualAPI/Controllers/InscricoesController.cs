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
    public class InscricoesController(IInscricaoService inscricaoService) : ControllerBase {
        private readonly IInscricaoService _inscricaoService = inscricaoService;

        // GET: api/inscricoes
        /// <summary>
        /// Obtém todas as inscrições.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InscricaoReadDto>>> GetInscricoes() {
            var inscricoes = await _inscricaoService.GetAllAsync();
            return Ok(inscricoes.Select(MapToReadDto));
        }

        // GET: api/inscricoes/{id}
        /// <summary>
        /// Obtém uma inscrição pelo ID local.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<InscricaoReadDto>> GetInscricao(int id) {
            var inscricao = await _inscricaoService.GetByIdAsync(id);

            if (inscricao == null) {
                return NotFound();
            }

            return Ok(MapToReadDto(inscricao));
        }

        // POST: api/inscricoes (Cria ou Atualiza via ClientAppId)
        /// <summary>
        /// Cria uma nova inscrição ou atualiza uma existente (upsert) se o ClientAppId for fornecido.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<InscricaoReadDto>> PostInscricao([FromBody] InscricaoCreateUpdateDto dto) {
            // Gera um novo ClientAppId se for uma nova criação via API
            var clientAppId = dto.ClientAppId ?? Guid.NewGuid();

            var entity = new Inscricao {
                ClientAppId = clientAppId,
                TimeId = dto.TimeId,
                TimeClientAppId = dto.TimeClientAppId,
                CampeonatoId = dto.CampeonatoId,
                CampeonatoClientAppId = dto.CampeonatoClientAppId,
                Status = dto.Status,
                // O Service cuidará das propriedades IsSynced e UpdatedAt
            };

            // Usa AddOrUpdateAsync para a lógica de upsert (criação ou atualização)
            var resultEntity = await _inscricaoService.AddOrUpdateAsync(entity);

            // Se foi uma operação de criação (novo ID gerado), retorna 201 Created
            if (resultEntity.Id > 0 && resultEntity.ClientAppId == clientAppId) {
                return CreatedAtAction(nameof(GetInscricao), new { id = resultEntity.Id }, MapToReadDto(resultEntity));
            }

            // Se foi uma atualização de um item existente (upsert), retorna 200 OK
            return Ok(MapToReadDto(resultEntity));
        }

        // PUT: api/inscricoes/{id}
        /// <summary>
        /// Atualiza uma inscrição pelo ID local.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutInscricao(int id, [FromBody] InscricaoCreateUpdateDto dto) {
            var entity = await _inscricaoService.GetByIdAsync(id);

            if (entity == null) {
                return NotFound();
            }

            // Atualiza propriedades (mantendo o ClientAppId existente)
            entity.TimeId = dto.TimeId;
            entity.TimeClientAppId = dto.TimeClientAppId;
            entity.CampeonatoId = dto.CampeonatoId;
            entity.CampeonatoClientAppId = dto.CampeonatoClientAppId;
            entity.Status = dto.Status;

            // Reutiliza o AddOrUpdateAsync para atualizar a entidade e marcar IsSynced=false
            await _inscricaoService.AddOrUpdateAsync(entity);

            return NoContent();
        }

        // DELETE: api/inscricoes/clientapp/{clientAppId}
        /// <summary>
        /// Remove uma inscrição pelo ClientAppId.
        /// </summary>
        [HttpDelete("clientapp/{clientAppId:Guid}")]
        public async Task<IActionResult> DeleteInscricao(Guid clientAppId) {
            var deleted = await _inscricaoService.DeleteAsync(clientAppId);

            if (!deleted) {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/inscricoes/markassynced/{clientAppId}
        /// <summary>
        /// Marca uma inscrição como sincronizada.
        /// </summary>
        [HttpPost("markassynced/{clientAppId:Guid}")]
        public async Task<IActionResult> MarkAsSynced(Guid clientAppId) {
            var success = await _inscricaoService.MarkAsSyncedAsync(clientAppId);

            if (!success) {
                return NotFound();
            }

            return NoContent();
        }

        // ---------------------------------------------------------------------
        // Mapeamento DTO
        // ---------------------------------------------------------------------

        private static InscricaoReadDto MapToReadDto(Inscricao i) => new() {
            Id = i.Id,
            ClientAppId = i.ClientAppId,
            TimeId = i.TimeId,
            TimeClientAppId = i.TimeClientAppId,
            CampeonatoId = i.CampeonatoId,
            CampeonatoClientAppId = i.CampeonatoClientAppId,
            Status = i.Status,
            UpdatedAt = i.UpdatedAt
        };
    }
}