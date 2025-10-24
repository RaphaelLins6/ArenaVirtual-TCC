using ArenaVirtual.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ArenaVirtual.Services {
    public class CampeonatoService {
        private readonly DatabaseService _databaseService;
        private readonly SyncService _syncService;
        private readonly JogoService _jogoService;

        public CampeonatoService(DatabaseService databaseService, SyncService syncService, JogoService jogoService) {
            _databaseService = databaseService;
            _syncService = syncService;
            _jogoService = jogoService;
        }

        public async Task<int> AtualizarConviteAsync(Convite convite) {
            convite.IsSynced = false;
            int result = await _databaseService.AtualizarConviteAsync(convite);
            if (result > 0) {
                _syncService.ScheduleSync();
            }
            return result;
        }

        public async Task<List<Convite>> ObterSolicitacoesPendentesCampeonatoAsync(Guid campeonatoClientAppId) {
            return await _databaseService.ObterConvitesPendentesAsync(campeonatoClientAppId);
        }

        public async Task<List<Campeonato>> ObterTodosAsync() =>
            await _databaseService.ListarCampeonatosAsync();

        public async Task<Campeonato?> ObterPorIdAsync(int id) =>
            (await _databaseService.ListarCampeonatosAsync())
                .FirstOrDefault(c => c.Id == id);

        public async Task<Campeonato?> ObterPorCapitaoClientAppIdAsync(Guid capitaoClientAppId) {
            return await _databaseService.ObterCampeonatoPorCapitaoClientAppIdAsync(capitaoClientAppId);
        }

        public async Task<Campeonato?> ObterPorOrganizadorClientAppIdAsync(Guid organizadorClientAppId) {
            var campeonatos = await _databaseService.ListarCampeonatosAsync();
            return campeonatos.FirstOrDefault(c => c.OrganizadorClientAppId == organizadorClientAppId);
        }

        public async Task<List<Time>> GetTimesAceitos(int campeonatoId) {
            try {
                var times = await _databaseService.ObterTimesAceitosAsync(campeonatoId);

                //Debug.WriteLine($"[CampeonatoService] Encontrados {times.Count} times aceitos para o campeonato ID {campeonatoId}.");

                return times;
            } catch (Exception ex) {
                //Debug.WriteLine($"[CampeonatoService] ERRO ao obter times aceitos: {ex.Message}");
                return new List<Time>();
            }
        }

        public async Task<int> AdicionarAsync(Campeonato campeonato) {
            campeonato.IsSynced = false;

            int result = await _databaseService.InserirCampeonatoAsync(campeonato);

            if (result > 0) {
                //Debug.WriteLine("[CampeonatoService] Campeonato adicionado localmente. Agendando sincronização...");
                _syncService.ScheduleSync();
            }
            return result;
        }

        public async Task<int> AtualizarAsync(Campeonato campeonato) {
            campeonato.IsSynced = false;

            int result = await _databaseService.AtualizarCampeonatoAsync(campeonato);

            if (result > 0) {
                //Debug.WriteLine("[CampeonatoService] Campeonato atualizado localmente. Agendando sincronização...");
                _syncService.ScheduleSync();
            }
            return result;
        }

        public async Task<int> RemoverAsync(Campeonato campeonato) {
            return await _databaseService.DeletarCampeonatoAsync(campeonato);
        }

        public async Task<bool> RemoverArbitroDoCampeonatoAsync(Guid campeonatoClientAppId, Guid arbitroClientAppId) {
            try {
                //Debug.WriteLine($"[CampeonatoService] Tentando remover árbitro {arbitroClientAppId} do campeonato {campeonatoClientAppId}...");
                int jogosAtualizados = await _databaseService.DesvincularArbitroDosJogosAsync(campeonatoClientAppId, arbitroClientAppId);
                //Debug.WriteLine($"[CampeonatoService] Árbitro desvinculado de {jogosAtualizados} jogo(s).");
                int result = await _databaseService.DeletarConviteArbitroAceitoAsync(campeonatoClientAppId, arbitroClientAppId);
                if (result > 0) {
                    //Debug.WriteLine($"[CampeonatoService] Convite de árbitro deletado com sucesso. {result} registro(s) afetado(s).");
                    _syncService.ScheduleSync();
                    return true;
                }
                //Debug.WriteLine($"[CampeonatoService] Nenhum registro de convite de árbitro encontrado/deletado. Resultado: {result}");
                return false;
            } catch (Exception ex) {
                //Debug.WriteLine($"[CampeonatoService] ERRO ao remover árbitro: {ex.Message}");
                return false;
            }
        }

        public async Task RemoverTimeDoCampeonato(
            int campeonatoId,
            int timeId,
            Guid timeClientAppId, 
            Guid campeonatoClientAppId) 
        {
            await _databaseService.RemoverTimeDoCampeonatoAsync(
                campeonatoId,
                timeId,
                timeClientAppId,
                campeonatoClientAppId
            );
        }

        public async Task RecalcularEGerarJogosAsync(Guid campeonatoClientAppId) {
            //Debug.WriteLine($"[CampeonatoService] Iniciando recalculo para o campeonato: {campeonatoClientAppId}");

            var campeonato = await _databaseService.ObterCampeonatoPorClientAppIdAsync(campeonatoClientAppId);

            if (campeonato == null) return;

            await _databaseService.DeletarJogosECascataPorCampeonatoAsync(campeonatoClientAppId);

            var times = await GetTimesAceitos(campeonato.Id);

            if (times.Count >= 2) {
                await _jogoService.GerarTabelaJogosAsync(campeonato, times);
                //Debug.WriteLine($"[CampeonatoService] {times.Count} times encontrados. Tabela de jogos regenerada.");
            } else {
                //Debug.WriteLine($"[CampeonatoService] Não há times suficientes ({times.Count}) para gerar jogos.");
            }

            _syncService.ScheduleSync();
        }

        public async Task<Campeonato?> ObterPorClientAppIdAsync(Guid clientAppId) {
            return await _databaseService.ObterCampeonatoPorClientAppIdAsync(clientAppId);
        }
    }
}