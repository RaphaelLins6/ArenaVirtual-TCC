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
    // Injeta o serviço de sincronização
    public class PropostasPatrocinioController(
        IBackendService<PropostaPatrocinio, PropostaPatrocinioSyncDto> propostaService) : ControllerBase {
        private readonly IBackendService<PropostaPatrocinio, PropostaPatrocinioSyncDto> _propostaService = propostaService;

        // GET: api/propostaspatrocinio/{id}
        /// <summary>
        /// Obtém uma Proposta de Patrocínio pelo ID local.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PropostaPatrocinioReadDto>> GetPropostaPatrocinio(int id) {
            var proposta = await _propostaService.GetByIdAsync(id);

            if (proposta == null) {
                return NotFound();
            }

            return Ok(MapToReadDto(proposta));
        }

        // POST: api/propostaspatrocinio
        /// <summary>
        /// Cria uma nova Proposta de Patrocínio diretamente pela API.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PropostaPatrocinioReadDto>> PostPropostaPatrocinio([FromBody] PropostaPatrocinioCreateUpdateDto dto) {
            var entity = new PropostaPatrocinio {
                PatrocinadorId = dto.PatrocinadorId,
                CampeonatoId = dto.CampeonatoId,
                NomePatrocinador = dto.NomePatrocinador,
                ImagemPatrocinador = dto.ImagemPatrocinador,
                LinkPatrocinador = dto.LinkPatrocinador,
                ValorMonetario = dto.ValorMonetario,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                Mensagem = dto.Mensagem,
                Aprovada = dto.Aprovada,
                // O ClientAppId é gerado aqui, pois a API é a fonte.
                ClientAppId = Guid.NewGuid(),
            };

            await _propostaService.AddAsync(entity);

            return CreatedAtAction(nameof(GetPropostaPatrocinio), new { id = entity.Id }, MapToReadDto(entity));
        }

        // PUT: api/propostaspatrocinio/{id}
        /// <summary>
        /// Atualiza uma Proposta de Patrocínio pelo ID local.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutPropostaPatrocinio(int id, [FromBody] PropostaPatrocinioCreateUpdateDto dto) {
            var entity = await _propostaService.GetByIdAsync(id);

            if (entity == null) {
                return NotFound();
            }

            // Atualiza campos
            entity.PatrocinadorId = dto.PatrocinadorId;
            entity.CampeonatoId = dto.CampeonatoId;
            entity.NomePatrocinador = dto.NomePatrocinador;
            entity.ImagemPatrocinador = dto.ImagemPatrocinador;
            entity.LinkPatrocinador = dto.LinkPatrocinador;
            entity.ValorMonetario = dto.ValorMonetario;
            entity.DataInicio = dto.DataInicio;
            entity.DataFim = dto.DataFim;
            entity.Mensagem = dto.Mensagem;
            entity.Aprovada = dto.Aprovada;

            // O service cuidará de marcar como IsSynced=true e atualizar UpdatedAt
            await _propostaService.UpdateAsync(entity);
            return NoContent();
        }

        // ---------------------------------------------------------------------
        // Endpoint de Sincronização (Upload do Cliente)
        // ---------------------------------------------------------------------

        // POST: api/propostaspatrocinio/sync-upload
        /// <summary>
        /// Recebe e processa uma lista de DTOs de Propostas de Patrocínio para sincronização (upsert do cliente).
        /// </summary>
        [HttpPost("sync-upload")]
        public async Task<ActionResult<Dictionary<Guid, int>>> SyncUpload([FromBody] IEnumerable<PropostaPatrocinioSyncDto> dtos) {
            if (dtos == null || !dtos.Any()) {
                return BadRequest("Nenhum item enviado para sincronização.");
            }

            // O ProcessAndMapItemsAsync cuida do upsert e retorna o mapa ClientAppId -> Id
            var idMapping = await _propostaService.ProcessAndMapItemsAsync(dtos);

            // A Fase 2 (UpdateForeignKeysAsync) deve ser chamada APÓS O UPLOAD de todas as entidades relacionadas
            // (Usuario e Campeonato) ter sido concluído, no serviço de sincronização mestre.
            // Aqui, o controller apenas retorna o mapeamento de IDs locais.

            return Ok(idMapping);
        }

        // GET: api/propostaspatrocinio/sync-download?lastSyncTime=2025-01-01T00:00:00Z
        /// <summary>
        /// Retorna todas as Propostas de Patrocínio atualizadas desde o último horário de sincronização.
        /// </summary>
        [HttpGet("sync-download")]
        public async Task<ActionResult<IEnumerable<PropostaPatrocinioSyncDto>>> SyncDownload([FromQuery] DateTime lastSyncTime) {
            var updatedItems = await _propostaService.GetUpdatedSinceAsync(lastSyncTime);
            return Ok(updatedItems);
        }

        // ---------------------------------------------------------------------
        // Mapeamento DTO
        // ---------------------------------------------------------------------

        private static PropostaPatrocinioReadDto MapToReadDto(PropostaPatrocinio p) => new() {
            Id = p.Id,
            ClientAppId = p.ClientAppId,
            PatrocinadorId = p.PatrocinadorId,
            CampeonatoId = p.CampeonatoId,
            NomePatrocinador = p.NomePatrocinador,
            ImagemPatrocinador = p.ImagemPatrocinador,
            LinkPatrocinador = p.LinkPatrocinador,
            ValorMonetario = p.ValorMonetario,
            DataInicio = p.DataInicio,
            DataFim = p.DataFim,
            Mensagem = p.Mensagem,
            Aprovada = p.Aprovada,
            UpdatedAt = p.UpdatedAt
        };
    }
}