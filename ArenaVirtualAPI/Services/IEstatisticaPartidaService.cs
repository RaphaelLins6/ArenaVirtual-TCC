using ArenaVirtualAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    // Interface para definir as operações de negócio para a entidade EstatisticaPartida
    public interface IEstatisticaPartidaService {
        // Obtém todas as estatísticas
        Task<IEnumerable<EstatisticaPartida>> GetAllAsync();

        // Obtém uma estatística pelo seu ID local (chave primária)
        Task<EstatisticaPartida?> GetByIdAsync(int id);

        // Obtém uma estatística pelo seu ID universal (ClientAppId)
        Task<EstatisticaPartida?> GetByClientAppIdAsync(Guid clientAppId);

        // Adiciona uma nova estatística ou atualiza uma existente (usando ClientAppId para upsert)
        Task<EstatisticaPartida> AddOrUpdateAsync(EstatisticaPartida estatistica);

        // Marca uma estatística como sincronizada
        Task<bool> MarkAsSyncedAsync(Guid clientAppId);

        // Remove uma estatística pelo seu ID universal (ClientAppId)
        Task<bool> DeleteAsync(Guid clientAppId);

        // **OPCIONAL:** Métodos de consulta específicos, como buscar por Partida ou Jogador
        // Task<IEnumerable<EstatisticaPartida>> GetByJogoIdAsync(int jogoId);
        // Task<IEnumerable<EstatisticaPartida>> GetByUsuarioIdAsync(int usuarioId);
    }
}