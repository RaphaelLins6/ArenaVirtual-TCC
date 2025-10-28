using ArenaVirtualAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Services {
    // Interface para definir as operações de negócio para a entidade CampanhaPatrocinio
    public interface ICampanhaPatrocinioService {
        // Obtém todas as campanhas
        Task<IEnumerable<CampanhaPatrocinio>> GetAllAsync();

        // Obtém uma campanha pelo seu ID local (chave primária)
        Task<CampanhaPatrocinio?> GetByIdAsync(int id);

        // Obtém uma campanha pelo seu ID universal (ClientAppId)
        Task<CampanhaPatrocinio?> GetByClientAppIdAsync(Guid clientAppId);

        // Adiciona uma nova campanha ou atualiza uma existente (usando ClientAppId para upsert)
        Task<CampanhaPatrocinio> AddOrUpdateAsync(CampanhaPatrocinio campanha);

        // Marca uma campanha como sincronizada
        Task<bool> MarkAsSyncedAsync(Guid clientAppId);

        // Remove uma campanha pelo seu ID universal (ClientAppId)
        Task<bool> DeleteAsync(Guid clientAppId);

        // OPCIONAL: Busca campanhas ativas para um campeonato específico
        Task<IEnumerable<CampanhaPatrocinio>> GetActiveByCampeonatoIdAsync(int campeonatoId, DateTime? dataConsulta = null);
    }
}