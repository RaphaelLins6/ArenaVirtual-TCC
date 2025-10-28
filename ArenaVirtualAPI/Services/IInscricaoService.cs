using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Services {
    // Interface para definir as operações de negócio para a entidade Inscricao
    public interface IInscricaoService {
        // Obtém todas as inscrições
        Task<IEnumerable<Inscricao>> GetAllAsync();

        // Obtém uma inscrição pelo seu ID local (chave primária)
        Task<Inscricao?> GetByIdAsync(int id);

        // Obtém uma inscrição pelo seu ID universal (ClientAppId)
        Task<Inscricao?> GetByClientAppIdAsync(Guid clientAppId);

        // Adiciona uma nova inscrição ou atualiza uma existente (usando ClientAppId para upsert)
        Task<Inscricao> AddOrUpdateAsync(Inscricao inscricao);

        // Marca uma inscrição como sincronizada
        Task<bool> MarkAsSyncedAsync(Guid clientAppId);

        // Remove uma inscrição pelo seu ID universal (ClientAppId)
        Task<bool> DeleteAsync(Guid clientAppId);
    }
}
