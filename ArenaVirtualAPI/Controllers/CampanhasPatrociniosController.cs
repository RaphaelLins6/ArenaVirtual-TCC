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
    public class CampanhasPatrociniosController(ICampanhaPatrocinioService campanhaService) : ControllerBase {
        private readonly ICampanhaPatrocinioService _campanhaService = campanhaService;

        // GET: api/campanhaspatrocinios
        /// <summary>
        /// Obtém todas as campanhas de patrocínio.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CampanhaPatrocinioReadDto>>> GetCampanhasPatrocinios() {
            var campanhas = await _campanhaService.GetAllAsync();
            return Ok(campanhas.Select(MapToReadDto));
        }

        // GET: api/campanhaspatrocinios/{id}
        /// <summary>
        /// Obtém uma campanha pelo ID local.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CampanhaPatrocinioReadDto>> GetCampanhaPatrocinio(int id) {
            var campanha = await _campanhaService.GetByIdAsync(id);

            if (campanha == null) {
                return NotFound();
            }

            return Ok(MapToReadDto(campanha));
        }

        // GET: api/campanhaspatrocinios/ativas/{campeonatoId}
        /// <summary>
        /// Obtém todas as campanhas ativas para um campeonato específico.
        /// </summary>
        [HttpGet("ativas/{campeonatoId:int}")]
        public async Task<ActionResult<IEnumerable<CampanhaPatrocinioReadDto>>> GetCampanhasAtivasByCampeonatoId(int campeonatoId) {
            var campanhas = await _campanhaService.GetActiveByCampeonatoIdAsync(campeonatoId);
            return Ok(campanhas.Select(MapToReadDto));
        }

        // POST: api/campanhaspatrocinios (Cria ou Atualiza via ClientAppId)
        /// <summary>
        /// Cria uma nova campanha ou atualiza uma existente (upsert) se o ClientAppId for fornecido.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CampanhaPatrocinioReadDto>> PostCampanhaPatrocinio([FromBody] CampanhaPatrocinioCreateUpdateDto dto) {
            var clientAppId = dto.ClientAppId ?? Guid.NewGuid();

            var entity = new CampanhaPatrocinio {
                ClientAppId = clientAppId,
                PatrocinadorId = dto.PatrocinadorId,
                CampeonatoId = dto.CampeonatoId,
                Nome = dto.Nome,
                ImagemPatrocinador = dto.ImagemPatrocinador,
                ValorProposta = dto.ValorProposta,
                Inicio = dto.Inicio,
                Fim = dto.Fim,
                Descricao = dto.Descricao,
            };

            var resultEntity = await _campanhaService.AddOrUpdateAsync(entity);

            if (resultEntity.Id > 0 && resultEntity.ClientAppId == clientAppId) {
                return CreatedAtAction(nameof(GetCampanhaPatrocinio), new { id = resultEntity.Id }, MapToReadDto(resultEntity));
            }

            return Ok(MapToReadDto(resultEntity));
        }

        // PUT: api/campanhaspatrocinios/{id}
        /// <summary>
        /// Atualiza uma campanha de patrocínio pelo ID local.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCampanhaPatrocinio(int id, [FromBody] CampanhaPatrocinioCreateUpdateDto dto) {
            var entity = await _campanhaService.GetByIdAsync(id);

            if (entity == null) {
                return NotFound();
            }

            // Atualiza propriedades
            entity.PatrocinadorId = dto.PatrocinadorId;
            entity.CampeonatoId = dto.CampeonatoId;
            entity.Nome = dto.Nome;
            entity.ImagemPatrocinador = dto.ImagemPatrocinador;
            entity.ValorProposta = dto.ValorProposta;
            entity.Inicio = dto.Inicio;
            entity.Fim = dto.Fim;
            entity.Descricao = dto.Descricao;

            // Reutiliza o AddOrUpdateAsync para atualizar a entidade e marcar IsSynced=false
            await _campanhaService.AddOrUpdateAsync(entity);

            return NoContent();
        }

        // DELETE: api/campanhaspatrocinios/clientapp/{clientAppId}
        /// <summary>
        /// Remove uma campanha de patrocínio pelo ClientAppId.
        /// </summary>
        [HttpDelete("clientapp/{clientAppId:Guid}")]
        public async Task<IActionResult> DeleteCampanhaPatrocinio(Guid clientAppId) {
            var deleted = await _campanhaService.DeleteAsync(clientAppId);

            if (!deleted) {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/campanhaspatrocinios/markassynced/{clientAppId}
        /// <summary>
        /// Marca uma campanha de patrocínio como sincronizada.
        /// </summary>
        [HttpPost("markassynced/{clientAppId:Guid}")]
        public async Task<IActionResult> MarkAsSynced(Guid clientAppId) {
            var success = await _campanhaService.MarkAsSyncedAsync(clientAppId);

            if (!success) {
                return NotFound();
            }

            return NoContent();
        }

        // ---------------------------------------------------------------------
        // Mapeamento DTO
        // ---------------------------------------------------------------------

        private static CampanhaPatrocinioReadDto MapToReadDto(CampanhaPatrocinio c) => new() {
            Id = c.Id,
            ClientAppId = c.ClientAppId,
            Nome = c.Nome,
            ImagemPatrocinador = c.ImagemPatrocinador,
            PatrocinadorId = c.PatrocinadorId,
            CampeonatoId = c.CampeonatoId,
            ValorProposta = c.ValorProposta,
            Inicio = c.Inicio,
            Fim = c.Fim,
            Descricao = c.Descricao,
            UpdatedAt = c.UpdatedAt
        };
    }
}