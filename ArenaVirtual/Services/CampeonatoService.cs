using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics; // Adicionado para Debug.WriteLine

namespace ArenaVirtual.Services {
    public class CampeonatoService(DatabaseService databaseService, SyncService syncService) {
        private readonly DatabaseService _databaseService = databaseService;
        private readonly SyncService _syncService = syncService;

        public async Task<List<Campeonato>> ObterTodosAsync() =>
            await _databaseService.ListarCampeonatosAsync();

        public async Task<Campeonato?> ObterPorIdAsync(int id) =>
            (await _databaseService.ListarCampeonatosAsync()).FirstOrDefault(c => c.Id == id);

        public async Task<int> AdicionarAsync(Campeonato campeonato) {
            campeonato.IsSynced = false;
            campeonato.UpdatedAt = DateTime.UtcNow;

            int result = await _databaseService.InserirCampeonatoAsync(campeonato);

            if (result > 0) {
                Debug.WriteLine("[CampeonatoService] Campeonato adicionado localmente. Agendando sincronização...");
                _syncService.ScheduleSync();
            }
            return result;
        }

        public async Task<int> AtualizarAsync(Campeonato campeonato) {
            campeonato.IsSynced = false;
            campeonato.UpdatedAt = DateTime.UtcNow;

            int result = await _databaseService.AtualizarCampeonatoAsync(campeonato);

            if (result > 0) {
                Debug.WriteLine("[CampeonatoService] Campeonato atualizado localmente. Agendando sincronização...");
                _syncService.ScheduleSync();
            }
            return result;
        }

        public async Task<int> RemoverAsync(Campeonato campeonato) {
            int result = await _databaseService.DeletarCampeonatoAsync(campeonato);
            // Se você também quiser sincronizar a remoção, a API precisaria de um endpoint para isso
            // e o Campeonato teria que ser marcado como 'Deletado' em vez de removido diretamente.
            // Por enquanto, vamos manter a sincronização apenas para Adicionar/Atualizar.
            return result;
        }
    }
}