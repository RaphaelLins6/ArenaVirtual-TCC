using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Data; // Assumindo que seu DbContext está em ArenaVirtualAPI.Data
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    // Serviço de Inscrição que lida com a lógica de negócio e persistência
    public class InscricaoService : IInscricaoService {
        private readonly ApiDbContext _context;

        // Injeção de dependência do DbContext
        public InscricaoService(ApiDbContext context) {
            _context = context;
        }

        // Obtém todas as inscrições, incluindo as entidades relacionadas
        public async Task<IEnumerable<Inscricao>> GetAllAsync() {
            // Inclui as propriedades de navegação para um retorno mais completo (se necessário)
            return await _context.Inscricoes
                                 .Include(i => i.Time)
                                 .Include(i => i.Campeonato)
                                 .ToListAsync();
        }

        // Obtém uma inscrição pelo ID local
        public async Task<Inscricao?> GetByIdAsync(int id) {
            return await _context.Inscricoes
                                 .Include(i => i.Time)
                                 .Include(i => i.Campeonato)
                                 .FirstOrDefaultAsync(i => i.Id == id);
        }

        // Obtém uma inscrição pelo ClientAppId (chave de sincronização)
        public async Task<Inscricao?> GetByClientAppIdAsync(Guid clientAppId) {
            return await _context.Inscricoes
                                 .Include(i => i.Time)
                                 .Include(i => i.Campeonato)
                                 .FirstOrDefaultAsync(i => i.ClientAppId == clientAppId);
        }

        // Adiciona ou Atualiza uma inscrição (Upsert baseado no ClientAppId)
        public async Task<Inscricao> AddOrUpdateAsync(Inscricao inscricao) {
            var existingInscricao = await _context.Inscricoes
                .FirstOrDefaultAsync(i => i.ClientAppId == inscricao.ClientAppId);

            inscricao.UpdatedAt = DateTime.UtcNow;

            if (existingInscricao == null) {
                // Inscrição não existe, adicionar
                inscricao.IsSynced = false; // Garante que a nova entrada não está sincronizada inicialmente
                await _context.Inscricoes.AddAsync(inscricao);
            } else {
                // Inscrição existe, atualizar propriedades (excluindo chaves primárias e de sincronização)

                // Mapeamento manual das propriedades que podem ser alteradas
                existingInscricao.TimeClientAppId = inscricao.TimeClientAppId;
                existingInscricao.CampeonatoClientAppId = inscricao.CampeonatoClientAppId;
                existingInscricao.Status = inscricao.Status;

                // Atualiza as chaves estrangeiras locais (TimeId, CampeonatoId) se elas vierem preenchidas
                existingInscricao.TimeId = inscricao.TimeId;
                existingInscricao.CampeonatoId = inscricao.CampeonatoId;

                existingInscricao.IsSynced = false; // Marcar como não sincronizado após alteração
                existingInscricao.UpdatedAt = inscricao.UpdatedAt;

                _context.Inscricoes.Update(existingInscricao);
                inscricao = existingInscricao; // Retorna a instância rastreada pelo EF
            }

            await _context.SaveChangesAsync();
            return inscricao;
        }

        // Marca uma inscrição específica como sincronizada
        public async Task<bool> MarkAsSyncedAsync(Guid clientAppId) {
            var inscricao = await _context.Inscricoes
                .FirstOrDefaultAsync(i => i.ClientAppId == clientAppId);

            if (inscricao == null) {
                return false;
            }

            inscricao.IsSynced = true;
            inscricao.UpdatedAt = DateTime.UtcNow; // Atualiza o timestamp
            _context.Inscricoes.Update(inscricao);
            await _context.SaveChangesAsync();
            return true;
        }

        // Remove uma inscrição pelo ClientAppId
        public async Task<bool> DeleteAsync(Guid clientAppId) {
            var inscricao = await _context.Inscricoes
                .FirstOrDefaultAsync(i => i.ClientAppId == clientAppId);

            if (inscricao == null) {
                return false;
            }

            _context.Inscricoes.Remove(inscricao);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
