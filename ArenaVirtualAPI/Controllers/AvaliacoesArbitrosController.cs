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
    public class AvaliacoesArbitrosController(IAvaliacaoArbitroService avaliacaoService) : ControllerBase {
        private readonly IAvaliacaoArbitroService _avaliacaoService = avaliacaoService;

        // GET: api/avaliacoesarbitros
        /// <summary>
        /// Obtém todas as avaliações de árbitros.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AvaliacaoArbitroReadDto>>> GetAvaliacoesArbitros() {
            var avaliacoes = await _avaliacaoService.GetAllAsync();
            return Ok(avaliacoes.Select(MapToReadDto));
        }

        // GET: api/avaliacoesarbitros/{id}
        /// <summary>
        /// Obtém uma avaliação pelo ID local.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AvaliacaoArbitroReadDto>> GetAvaliacaoArbitro(int id) {
            var avaliacao = await _avaliacaoService.GetByIdAsync(id);

            if (avaliacao == null) {
                return NotFound();
            }

            return Ok(MapToReadDto(avaliacao));
        }

        // GET: api/avaliacoesarbitros/media/{arbitroId}
        /// <summary>
        /// Calcula a nota média de um árbitro pelo seu ID local.
        /// </summary>
        [HttpGet("media/{arbitroId:int}")]
        public async Task<ActionResult<double>> GetAverageRatingByArbitroId(int arbitroId) {
            var media = await _avaliacaoService.GetAverageRatingByArbitroIdAsync(arbitroId);

            // Retorna a média. Se não houver avaliações, o serviço retorna 0.0
            return Ok(media);
        }

        // POST: api/avaliacoesarbitros (Cria ou Atualiza via ClientAppId)
        /// <summary>
        /// Cria uma nova avaliação ou atualiza uma existente (upsert) se o ClientAppId for fornecido.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AvaliacaoArbitroReadDto>> PostAvaliacaoArbitro([FromBody] AvaliacaoArbitroCreateUpdateDto dto) {
            var clientAppId = dto.ClientAppId ?? Guid.NewGuid();

            var entity = new AvaliacaoArbitro {
                ClientAppId = clientAppId,
                ArbitroId = dto.ArbitroId,
                JogoId = dto.JogoId,
                Comentarios = dto.Comentarios,
                Nota = dto.Nota
            };

            var resultEntity = await _avaliacaoService.AddOrUpdateAsync(entity);

            if (resultEntity.Id > 0 && resultEntity.ClientAppId == clientAppId) {
                return CreatedAtAction(nameof(GetAvaliacaoArbitro), new { id = resultEntity.Id }, MapToReadDto(resultEntity));
            }

            return Ok(MapToReadDto(resultEntity));
        }

        // PUT: api/avaliacoesarbitros/{id}
        /// <summary>
        /// Atualiza uma avaliação de árbitro pelo ID local.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutAvaliacaoArbitro(int id, [FromBody] AvaliacaoArbitroCreateUpdateDto dto) {
            var entity = await _avaliacaoService.GetByIdAsync(id);

            if (entity == null) {
                return NotFound();
            }

            // Atualiza propriedades
            entity.ArbitroId = dto.ArbitroId;
            entity.JogoId = dto.JogoId;
            entity.Comentarios = dto.Comentarios;
            entity.Nota = dto.Nota;

            // Reutiliza o AddOrUpdateAsync para atualizar a entidade e marcar IsSynced=false
            await _avaliacaoService.AddOrUpdateAsync(entity);

            return NoContent();
        }

        // DELETE: api/avaliacoesarbitros/clientapp/{clientAppId}
        /// <summary>
        /// Remove uma avaliação de árbitro pelo ClientAppId.
        /// </summary>
        [HttpDelete("clientapp/{clientAppId:Guid}")]
        public async Task<IActionResult> DeleteAvaliacaoArbitro(Guid clientAppId) {
            var deleted = await _avaliacaoService.DeleteAsync(clientAppId);

            if (!deleted) {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/avaliacoesarbitros/markassynced/{clientAppId}
        /// <summary>
        /// Marca uma avaliação de árbitro como sincronizada.
        /// </summary>
        [HttpPost("markassynced/{clientAppId:Guid}")]
        public async Task<IActionResult> MarkAsSynced(Guid clientAppId) {
            var success = await _avaliacaoService.MarkAsSyncedAsync(clientAppId);

            if (!success) {
                return NotFound();
            }

            return NoContent();
        }

        // ---------------------------------------------------------------------
        // Mapeamento DTO
        // ---------------------------------------------------------------------

        private static AvaliacaoArbitroReadDto MapToReadDto(AvaliacaoArbitro a) => new() {
            Id = a.Id,
            ClientAppId = a.ClientAppId,
            ArbitroId = a.ArbitroId,
            JogoId = a.JogoId,
            Comentarios = a.Comentarios,
            Nota = a.Nota,
            UpdatedAt = a.UpdatedAt
        };
    }
}