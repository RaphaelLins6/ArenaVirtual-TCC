using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ArenaVirtualAPI.Services;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using System;

namespace ArenaVirtualAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    // Injeta o serviço de agregação
    public class PatrocinioDetalhesController(IPatrocinioDetalheService detalheService) : ControllerBase {
        private readonly IPatrocinioDetalheService _detalheService = detalheService;

        // GET: api/patrociniodetalhes/proposta/{propostaId}
        /// <summary>
        /// Obtém os detalhes agregados de um patrocínio com base no ID local da Proposta.
        /// </summary>
        [HttpGet("proposta/{propostaId:int}")]
        public async Task<ActionResult<PatrocinioDetalheReadDto>> GetDetalheByPropostaId(int propostaId) {
            var detalhe = await _detalheService.GetDetalheByPropostaIdAsync(propostaId);

            if (detalhe == null || detalhe.Proposta == null) {
                // Retorna 404 se a proposta base não for encontrada
                return NotFound();
            }

            // Mapeia o modelo agregado para o DTO de leitura
            return Ok(MapToReadDto(detalhe));
        }

        // ---------------------------------------------------------------------
        // Mapeamento DTO
        // ---------------------------------------------------------------------

        private PatrocinioDetalheReadDto MapToReadDto(PatrocinioDetalhe detalhe) {
            var proposta = detalhe.Proposta;
            var campanha = detalhe.Campanha;

            // Mapeia a Proposta (assumindo que as propriedades de navegação Patrocinador e Campeonato estão carregadas para obter ClientAppId)
            var dto = new PatrocinioDetalheReadDto {
                PropostaId = proposta.Id,
                PropostaClientAppId = proposta.ClientAppId,
                PropostaUpdatedAt = proposta.UpdatedAt,
                ValorMonetario = proposta.ValorMonetario,
                DataInicio = proposta.DataInicio,
                DataFim = proposta.DataFim,
                Mensagem = proposta.Mensagem,
                Aprovada = proposta.Aprovada,

                NomePatrocinador = proposta.NomePatrocinador,
                ImagemPatrocinador = proposta.ImagemPatrocinador,

                // Mapeamento das FKs (ClientAppId das entidades relacionadas)
                PatrocinadorClientAppId = proposta.Patrocinador?.ClientAppId ?? Guid.Empty,
                CampeonatoClientAppId = proposta.Campeonato?.ClientAppId ?? Guid.Empty,
            };

            // Mapeia a Campanha (opcional)
            if (campanha != null) {
                dto.CampanhaClientAppId = campanha.ClientAppId;
                dto.CampanhaNome = campanha.Nome;
                dto.CampanhaValorProposta = campanha.ValorProposta;
                dto.CampanhaInicio = campanha.Inicio;
                dto.CampanhaFim = campanha.Fim;
                dto.CampanhaDescricao = campanha.Descricao;
            }

            return dto;
        }
    }
}