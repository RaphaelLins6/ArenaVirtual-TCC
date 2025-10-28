using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    // Interface para definir as operações de negócio para a entidade AvaliacaoArbitro
    public interface IAvaliacaoArbitroService {
        // Obtém todas as avaliações
        Task<IEnumerable<AvaliacaoArbitro>> GetAllAsync();

        // Obtém uma avaliação pelo seu ID local (chave primária)
        Task<AvaliacaoArbitro?> GetByIdAsync(int id);

        // Obtém uma avaliação pelo seu ID universal (ClientAppId)
        Task<AvaliacaoArbitro?> GetByClientAppIdAsync(Guid clientAppId);

        // Adiciona uma nova avaliação ou atualiza uma existente (usando ClientAppId para upsert)
        Task<AvaliacaoArbitro> AddOrUpdateAsync(AvaliacaoArbitro avaliacao);

        // Marca uma avaliação como sincronizada
        Task<bool> MarkAsSyncedAsync(Guid clientAppId);

        // Remove uma avaliação pelo seu ID universal (ClientAppId)
        Task<bool> DeleteAsync(Guid clientAppId);

        // OPCIONAL: Calcula a nota média de um árbitro
        Task<double> GetAverageRatingByArbitroIdAsync(int arbitroId);
    }
}