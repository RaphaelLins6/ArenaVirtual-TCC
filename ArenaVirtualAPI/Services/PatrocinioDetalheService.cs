using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    public class PatrocinioDetalheService : IPatrocinioDetalheService {
        private readonly ApiDbContext _context;

        public PatrocinioDetalheService(ApiDbContext context) {
            _context = context;
        }

        public async Task<PatrocinioDetalhe?> GetDetalheByPropostaIdAsync(int propostaId) {
            // 1. Busca a Proposta de Patrocínio
            var proposta = await _context.PropostasPatrocinio
                .Include(p => p.Campeonato) // Inclui o Campeonato para acesso a dados, se necessário
                .Include(p => p.Patrocinador) // Inclui o Patrocinador (Usuario)
                .FirstOrDefaultAsync(p => p.Id == propostaId);

            if (proposta == null) {
                return null;
            }

            // 2. Busca a Campanha de Patrocínio relacionada, assumindo que:
            //    - É a campanha feita pelo Patrocinador para o mesmo Campeonato
            //    - OU, em um cenário de negócio mais simples, pode ser que a proposta se refira diretamente
            //      à campanha (Se houvesse um CampanhaId na Proposta).
            //    Como essa FK não está visível no seu modelo, vamos buscar a campanha com as mesmas FKs:
            var campanha = await _context.CampanhasPatrocinios
                .FirstOrDefaultAsync(c =>
                    c.CampeonatoId == proposta.CampeonatoId &&
                    c.PatrocinadorId == proposta.PatrocinadorId);

            // Se for essencial que haja uma campanha relacionada, você pode retornar null aqui também.
            // Para o modelo de detalhe, vamos permitir que a Campanha seja null se não for encontrada.

            // 3. Monta e retorna o objeto de agregação (Detalhe)
            return new PatrocinioDetalhe {
                Proposta = proposta,
                Campanha = campanha! // O compilador C# moderno pode reclamar de 'campanha', use '!' se for garantido ou '?' se for aceito null
            };
        }
    }
}