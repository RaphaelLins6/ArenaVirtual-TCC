using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Data; // Assumindo que seu DbContext está em ArenaVirtualAPI.Data
using System.Linq;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    // Serviço de EstatisticaPartida que lida com a lógica de negócio e persistência
    public class EstatisticaPartidaService : IEstatisticaPartidaService {
        private readonly ApiDbContext _context;

        // Injeção de dependência do DbContext
        public EstatisticaPartidaService(ApiDbContext context) {
            _context = context;
        }

        // Obtém todas as estatísticas, incluindo as entidades relacionadas (Partida, Jogador, Time)
        public async Task<IEnumerable<EstatisticaPartida>> GetAllAsync() {
            return await _context.EstatisticasPartidas
                                 .Include(e => e.Usuario)
                                 .Include(e => e.Jogo)
                                 .Include(e => e.Time)
                                 .ToListAsync();
        }

        // Obtém uma estatística pelo ID local
        public async Task<EstatisticaPartida?> GetByIdAsync(int id) {
            return await _context.EstatisticasPartidas
                                 .Include(e => e.Usuario)
                                 .Include(e => e.Jogo)
                                 .Include(e => e.Time)
                                 .FirstOrDefaultAsync(e => e.Id == id);
        }

        // Obtém uma estatística pelo ClientAppId (chave de sincronização)
        public async Task<EstatisticaPartida?> GetByClientAppIdAsync(Guid clientAppId) {
            return await _context.EstatisticasPartidas
                                 .Include(e => e.Usuario)
                                 .Include(e => e.Jogo)
                                 .Include(e => e.Time)
                                 .FirstOrDefaultAsync(e => e.ClientAppId == clientAppId);
        }

        // Adiciona ou Atualiza uma estatística (Upsert baseado no ClientAppId)
        public async Task<EstatisticaPartida> AddOrUpdateAsync(EstatisticaPartida estatistica) {
            var existingEstatistica = await _context.EstatisticasPartidas
                .FirstOrDefaultAsync(e => e.ClientAppId == estatistica.ClientAppId);

            estatistica.UpdatedAt = DateTime.UtcNow;

            if (existingEstatistica == null) {
                // Estatística não existe, adicionar
                estatistica.IsSynced = false;
                await _context.EstatisticasPartidas.AddAsync(estatistica);
            } else {
                // Estatística existe, atualizar as propriedades

                // Atualizar as chaves estrangeiras locais
                existingEstatistica.UsuarioId = estatistica.UsuarioId;
                existingEstatistica.JogoId = estatistica.JogoId;
                existingEstatistica.TimeId = estatistica.TimeId;

                // Atualizar as propriedades das estatísticas
                existingEstatistica.Pontos = estatistica.Pontos;
                existingEstatistica.Rebotes = estatistica.Rebotes;
                existingEstatistica.Assistencias = estatistica.Assistencias;
                existingEstatistica.Roubos = estatistica.Roubos;
                existingEstatistica.Bloqueios = estatistica.Bloqueios;
                existingEstatistica.Faltas = estatistica.Faltas;
                existingEstatistica.Turnovers = estatistica.Turnovers;
                existingEstatistica.Arremessos2PontosConvertidos = estatistica.Arremessos2PontosConvertidos;
                existingEstatistica.Arremessos2PontosTentados = estatistica.Arremessos2PontosTentados;
                existingEstatistica.Arremessos3PontosConvertidos = estatistica.Arremessos3PontosConvertidos;
                existingEstatistica.Arremessos3PontosTentados = estatistica.Arremessos3PontosTentados;
                existingEstatistica.LancesLivresConvertidos = estatistica.LancesLivresConvertidos;
                existingEstatistica.LancesLivresTentados = estatistica.LancesLivresTentados;

                existingEstatistica.IsSynced = false; // Marcar como não sincronizado após alteração
                existingEstatistica.UpdatedAt = estatistica.UpdatedAt;

                _context.EstatisticasPartidas.Update(existingEstatistica);
                estatistica = existingEstatistica; // Retorna a instância rastreada pelo EF
            }

            await _context.SaveChangesAsync();
            return estatistica;
        }

        // Marca uma estatística específica como sincronizada
        public async Task<bool> MarkAsSyncedAsync(Guid clientAppId) {
            var estatistica = await _context.EstatisticasPartidas
                .FirstOrDefaultAsync(e => e.ClientAppId == clientAppId);

            if (estatistica == null) {
                return false;
            }

            estatistica.IsSynced = true;
            estatistica.UpdatedAt = DateTime.UtcNow;
            _context.EstatisticasPartidas.Update(estatistica);
            await _context.SaveChangesAsync();
            return true;
        }

        // Remove uma estatística pelo ClientAppId
        public async Task<bool> DeleteAsync(Guid clientAppId) {
            var estatistica = await _context.EstatisticasPartidas
                .FirstOrDefaultAsync(e => e.ClientAppId == clientAppId);

            if (estatistica == null) {
                return false;
            }

            _context.EstatisticasPartidas.Remove(estatistica);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}