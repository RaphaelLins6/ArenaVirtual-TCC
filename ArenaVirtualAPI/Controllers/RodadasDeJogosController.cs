using Microsoft.AspNetCore.Mvc;
using ArenaVirtualAPI.Services;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;

namespace ArenaVirtualAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class RodadasDeJogosController(
        IRodadaDeJogosService rodadaService) : ControllerBase {

        private readonly IRodadaDeJogosService _rodadaService = rodadaService;

        // GET: api/rodadasdejogos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RodadaDeJogosReadDto>>> GetAllRodadasDeJogos() {
            var rodadas = await _rodadaService.GetAllAsync();

            if (rodadas == null || !rodadas.Any()) {
                return Ok(Enumerable.Empty<RodadaDeJogosReadDto>());
            }

            var rodadasDto = rodadas.Select(r => MapToReadDto(r));

            return Ok(rodadasDto);
        }

        // GET: api/rodadasdejogos/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RodadaDeJogosReadDto>> GetRodadaDeJogos(int id) {
            var rodada = await _rodadaService.GetByIdAsync(id);

            if (rodada == null) {
                return NotFound();
            }

            return Ok(MapToReadDto(rodada));
        }

        // POST: api/rodadasdejogos
        [HttpPost]
        public async Task<ActionResult<RodadaDeJogosReadDto>> PostRodadaDeJogos([FromBody] RodadaDeJogosCreateUpdateDto dto) {
            var entity = new RodadaDeJogos {
                NomeRodada = dto.NomeRodada,
                ClientAppId = Guid.NewGuid(),
            };

            await _rodadaService.AddAsync(entity);

            return CreatedAtAction(nameof(GetRodadaDeJogos), new { id = entity.Id }, MapToReadDto(entity));
        }

        // PUT: api/rodadasdejogos/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutRodadaDeJogos(int id, [FromBody] RodadaDeJogosCreateUpdateDto dto) {
            var entity = await _rodadaService.GetByIdAsync(id);

            if (entity == null) {
                return NotFound();
            }

            entity.NomeRodada = dto.NomeRodada;

            await _rodadaService.UpdateAsync(entity);
            return NoContent();
        }

        // POST: api/rodadasdejogos/sync-upload
        [HttpPost("sync-upload")]
        public async Task<ActionResult<Dictionary<Guid, int>>> SyncUpload([FromBody] IEnumerable<RodadaDeJogosSyncDto> dtos) {
            if (dtos == null || !dtos.Any()) {
                return BadRequest("Nenhum item enviado para sincronização.");
            }

            var idMapping = await _rodadaService.ProcessAndMapItemsAsync(dtos);

            return Ok(idMapping);
        }

        // GET: api/rodadasdejogos/sync-download?lastSyncTime=2025-01-01T00:00:00Z
        [HttpGet("sync-download")]
        public async Task<ActionResult<IEnumerable<RodadaDeJogosSyncDto>>> SyncDownload([FromQuery] DateTime lastSyncTime) {
            var updatedItems = await _rodadaService.GetUpdatedSinceAsync(lastSyncTime);
            return Ok(updatedItems);
        }

        private static RodadaDeJogosReadDto MapToReadDto(RodadaDeJogos r) => new() {
            Id = r.Id,
            ClientAppId = r.ClientAppId,
            NomeRodada = r.NomeRodada,
            UpdatedAt = r.UpdatedAt
        };
    }
}