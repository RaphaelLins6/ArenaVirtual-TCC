using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Data; // Assumindo que seu DbContext está em ArenaVirtualAPI.Data
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    // Serviço de AvaliacaoArbitro que lida com a lógica de negócio e persistência
    public class AvaliacaoArbitroService : IAvaliacaoArbitroService {
        private readonly ApiDbContext _context;

        // Injeção de dependência do DbContext
        public AvaliacaoArbitroService(ApiDbContext context) {
            _context = context;
        }

        // Obtém todas as avaliações, incluindo as entidades relacionadas
        public async Task<IEnumerable<AvaliacaoArbitro>> GetAllAsync() {
            return await _context.AvaliacoesArbitros
                                 .Include(a => a.Arbitro) // Inclui o Usuário Árbitro
                                 .Include(a => a.Jogo) // Inclui o Jogo
                                 .ToListAsync();
        }

        // Obtém uma avaliação pelo ID local
        public async Task<AvaliacaoArbitro?> GetByIdAsync(int id) {
            return await _context.AvaliacoesArbitros
                                 .Include(a => a.Arbitro)
                                 .Include(a => a.Jogo)
                                 .FirstOrDefaultAsync(a => a.Id == id);
        }

        // Obtém uma avaliação pelo ClientAppId (chave de sincronização)
        public async Task<AvaliacaoArbitro?> GetByClientAppIdAsync(Guid clientAppId) {
            return await _context.AvaliacoesArbitros
                                 .Include(a => a.Arbitro)
                                 .Include(a => a.Jogo)
                                 .FirstOrDefaultAsync(a => a.ClientAppId == clientAppId);
        }

        // Adiciona ou Atualiza uma avaliação (Upsert baseado no ClientAppId)
        public async Task<AvaliacaoArbitro> AddOrUpdateAsync(AvaliacaoArbitro avaliacao) {
            var existingAvaliacao = await _context.AvaliacoesArbitros
                .FirstOrDefaultAsync(a => a.ClientAppId == avaliacao.ClientAppId);

            avaliacao.UpdatedAt = DateTime.UtcNow;

            if (existingAvaliacao == null) {
                // Avaliação não existe, adicionar
                avaliacao.IsSynced = false;
                await _context.AvaliacoesArbitros.AddAsync(avaliacao);
            } else {
                // Avaliação existe, atualizar as propriedades

                // Atualizar as chaves estrangeiras locais
                existingAvaliacao.ArbitroId = avaliacao.ArbitroId;
                existingAvaliacao.JogoId = avaliacao.JogoId;

                // Atualizar as demais propriedades
                existingAvaliacao.Comentarios = avaliacao.Comentarios;
                existingAvaliacao.Nota = avaliacao.Nota;

                existingAvaliacao.IsSynced = false; // Marcar como não sincronizado após alteração
                existingAvaliacao.UpdatedAt = avaliacao.UpdatedAt;

                _context.AvaliacoesArbitros.Update(existingAvaliacao);
                avaliacao = existingAvaliacao; // Retorna a instância rastreada pelo EF
            }

            await _context.SaveChangesAsync();
            return avaliacao;
        }

        // Marca uma avaliação específica como sincronizada
        public async Task<bool> MarkAsSyncedAsync(Guid clientAppId) {
            var avaliacao = await _context.AvaliacoesArbitros
                .FirstOrDefaultAsync(a => a.ClientAppId == clientAppId);

            if (avaliacao == null) {
                return false;
            }

            avaliacao.IsSynced = true;
            avaliacao.UpdatedAt = DateTime.UtcNow;
            _context.AvaliacoesArbitros.Update(avaliacao);
            await _context.SaveChangesAsync();
            return true;
        }

        // Remove uma avaliação pelo ClientAppId
        public async Task<bool> DeleteAsync(Guid clientAppId) {
            var avaliacao = await _context.AvaliacoesArbitros
                .FirstOrDefaultAsync(a => a.ClientAppId == clientAppId);

            if (avaliacao == null) {
                return false;
            }

            _context.AvaliacoesArbitros.Remove(avaliacao);
            await _context.SaveChangesAsync();
            return true;
        }

        // Implementação do método opcional: Calcula a nota média de um árbitro
        public async Task<double> GetAverageRatingByArbitroIdAsync(int arbitroId) {
            // O AsNoTracking é usado para otimizar, já que é uma operação de leitura
            var media = await _context.AvaliacoesArbitros
                .AsNoTracking()
                .Where(a => a.ArbitroId == arbitroId)
                .AverageAsync(a => (double?)a.Nota); // Cast para double? para lidar com o caso de não haver avaliações

            return media ?? 0.0; // Retorna 0.0 se não houver avaliações
        }
    }
}