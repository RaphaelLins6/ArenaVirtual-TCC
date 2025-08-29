using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ArenaVirtualAPI.Services {
    public class TimeService : IBackendService<Time> {
        private readonly AppDbContext _context;

        public TimeService(AppDbContext context) {
            _context = context;
        }

        public async Task<Time?> GetByIdAsync(int id) {
            return await _context.Times.FindAsync(id);
        }

        public async Task AddAsync(Time time) {
            // No backend, um item recém-adicionado é considerado sincronizado
            time.IsSynced = true;
            time.UpdatedAt = DateTime.UtcNow; // Garante que o timestamp é definido no backend
            _context.Times.Add(time);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Time time) {
            var existingTime = await _context.Times.FindAsync(time.Id);
            if (existingTime != null) {
                // Atualize as propriedades do time existente com os dados recebidos.
                // Adapte esta lista de propriedades conforme o que deve ser atualizado.
                existingTime.Nome = time.Nome;
                existingTime.LogoUrl = time.LogoUrl;
                existingTime.CampeonatoId = time.CampeonatoId;
                existingTime.Descricao = time.Descricao;
                existingTime.DataCriacao = time.DataCriacao;
                existingTime.Regiao = time.Regiao;
                existingTime.PontuacaoTotal = time.PontuacaoTotal;
                existingTime.Vitorias = time.Vitorias;
                existingTime.Derrotas = time.Derrotas;
                existingTime.Empates = time.Empates;
                existingTime.CapitaoId = time.CapitaoId;
                //existingTime.Membros = time.Membros; // Cuidado com coleções em atualizações diretas

                // Propriedades de sincronização gerenciadas pelo BackendSyncService
                existingTime.IsSynced = true;
                existingTime.UpdatedAt = DateTime.UtcNow; // Atualiza o timestamp de modificação no backend

                _context.Times.Update(existingTime);
                await _context.SaveChangesAsync();
            }
        }

        // CORREÇÃO: Altere o tipo de retorno para Task<IEnumerable<ISyncable>>
        public async Task<IEnumerable<ISyncable>> GetUpdatedSinceAsync(DateTime lastSyncTime) {
            // Retorna todos os times que foram atualizados (ou criados) desde a última sincronização
            // O Entity Framework Core irá converter a lista de Times para IEnumerable<ISyncable>
            return await _context.Times
                                 .Where(t => t.UpdatedAt > lastSyncTime)
                                 .ToListAsync();
        }

        public async Task ProcessItemsAsync(IEnumerable<Time> items) {
            foreach (var item in items) {
                var existingItem = await _context.Times.FindAsync(item.Id);
                if (existingItem == null) {
                    await AddAsync(item);
                } else {
                    await UpdateAsync(item);
                }
            }
        }
    }
}
