using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    // Serviço de CampanhaPatrocinio que lida com a lógica de negócio e persistência
    public class CampanhaPatrocinioService : ICampanhaPatrocinioService {
        private readonly ApiDbContext _context;

        // Injeção de dependência do DbContext
        public CampanhaPatrocinioService(ApiDbContext context) {
            _context = context;
        }

        // Obtém todas as campanhas, incluindo as entidades relacionadas
        public async Task<IEnumerable<CampanhaPatrocinio>> GetAllAsync() {
            return await _context.CampanhasPatrocinios
                                 .Include(c => c.Patrocinador) // Inclui o Usuário Patrocinador
                                 .Include(c => c.Campeonato) // Inclui o Campeonato
                                 .ToListAsync();
        }

        // Obtém uma campanha pelo ID local
        public async Task<CampanhaPatrocinio?> GetByIdAsync(int id) {
            return await _context.CampanhasPatrocinios
                                 .Include(c => c.Patrocinador)
                                 .Include(c => c.Campeonato)
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Obtém uma campanha pelo ClientAppId (chave de sincronização)
        public async Task<CampanhaPatrocinio?> GetByClientAppIdAsync(Guid clientAppId) {
            return await _context.CampanhasPatrocinios
                                 .Include(c => c.Patrocinador)
                                 .Include(c => c.Campeonato)
                                 .FirstOrDefaultAsync(c => c.ClientAppId == clientAppId);
        }

        // Adiciona ou Atualiza uma campanha (Upsert baseado no ClientAppId)
        public async Task<CampanhaPatrocinio> AddOrUpdateAsync(CampanhaPatrocinio campanha) {
            var existingCampanha = await _context.CampanhasPatrocinios
                .FirstOrDefaultAsync(c => c.ClientAppId == campanha.ClientAppId);

            campanha.UpdatedAt = DateTime.UtcNow;

            if (existingCampanha == null) {
                // Campanha não existe, adicionar
                campanha.IsSynced = false;
                await _context.CampanhasPatrocinios.AddAsync(campanha);
            } else {
                // Campanha existe, atualizar as propriedades

                // Atualizar as chaves estrangeiras locais
                existingCampanha.PatrocinadorId = campanha.PatrocinadorId;
                existingCampanha.CampeonatoId = campanha.CampeonatoId;

                // Atualizar as demais propriedades
                existingCampanha.Nome = campanha.Nome;
                existingCampanha.ImagemPatrocinador = campanha.ImagemPatrocinador;
                existingCampanha.ValorProposta = campanha.ValorProposta;
                existingCampanha.Inicio = campanha.Inicio;
                existingCampanha.Fim = campanha.Fim;
                existingCampanha.Descricao = campanha.Descricao;

                existingCampanha.IsSynced = false; // Marcar como não sincronizado após alteração
                existingCampanha.UpdatedAt = campanha.UpdatedAt;

                _context.CampanhasPatrocinios.Update(existingCampanha);
                campanha = existingCampanha; // Retorna a instância rastreada pelo EF
            }

            await _context.SaveChangesAsync();
            return campanha;
        }

        // Marca uma campanha específica como sincronizada
        public async Task<bool> MarkAsSyncedAsync(Guid clientAppId) {
            var campanha = await _context.CampanhasPatrocinios
                .FirstOrDefaultAsync(c => c.ClientAppId == clientAppId);

            if (campanha == null) {
                return false;
            }

            campanha.IsSynced = true;
            campanha.UpdatedAt = DateTime.UtcNow;
            _context.CampanhasPatrocinios.Update(campanha);
            await _context.SaveChangesAsync();
            return true;
        }

        // Remove uma campanha pelo ClientAppId
        public async Task<bool> DeleteAsync(Guid clientAppId) {
            var campanha = await _context.CampanhasPatrocinios
                .FirstOrDefaultAsync(c => c.ClientAppId == clientAppId);

            if (campanha == null) {
                return false;
            }

            _context.CampanhasPatrocinios.Remove(campanha);
            await _context.SaveChangesAsync();
            return true;
        }

        // Implementação do método opcional de consulta
        public async Task<IEnumerable<CampanhaPatrocinio>> GetActiveByCampeonatoIdAsync(int campeonatoId, DateTime? dataConsulta = null) {
            var dataAtual = dataConsulta ?? DateTime.UtcNow;

            return await _context.CampanhasPatrocinios
                .Where(c => c.CampeonatoId == campeonatoId && c.Inicio <= dataAtual && c.Fim >= dataAtual)
                .Include(c => c.Patrocinador)
                .ToListAsync();
        }
    }
}