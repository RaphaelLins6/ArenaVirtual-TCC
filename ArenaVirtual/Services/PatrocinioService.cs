using ArenaVirtual.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaVirtual.Services {
    public class PatrocinioService {
        private readonly DatabaseService _databaseService;
        private readonly SessaoService _sessaoService;
        private readonly SyncService _syncService;

        public PatrocinioService(DatabaseService databaseService, SessaoService sessaoService, SyncService syncService) {
            _databaseService = databaseService;
            _sessaoService = sessaoService;
            _syncService = syncService;
        }

        // =========================================================
        // MÉTODOS DE CAMPANHA (Patrocinador cria uma Campanha)
        // =========================================================

        /// <summary>
        /// Obtém todas as Campanhas de Patrocínio ativas do Patrocinador logado.
        /// </summary>
        public async Task<List<CampanhaPatrocinio>> ObterCampanhasAtivasAsync() {
            var usuarioAtual = _sessaoService.GetUsuarioAtual();
            if (usuarioAtual == null || usuarioAtual.Perfil != TipoPerfil.Patrocinador) {
                return new List<CampanhaPatrocinio>();
            }

            try {
                var todasCampanhas = await _databaseService.ListarCampanhasPatrocinioAsync();

                // Filtra pelo ID do Patrocinador logado e por campanhas ativas (data Fim ainda não passou)
                var campanhasAtivas = todasCampanhas
                    .Where(c => c.PatrocinadorId == usuarioAtual.Id && c.Fim >= DateTime.Now)
                    .ToList();

                return campanhasAtivas;
            } catch (Exception ex) {
                Debug.WriteLine($"[PatrocinioService] Erro ao obter campanhas ativas: {ex.Message}");
                return new List<CampanhaPatrocinio>();
            }
        }

        /// <summary>
        /// Cria e salva uma nova Campanha de Patrocínio.
        /// </summary>
        public async Task<int> CriarCampanhaAsync(CampanhaPatrocinio campanha) {
            var usuarioAtual = _sessaoService.GetUsuarioAtual();
            if (usuarioAtual == null || usuarioAtual.Id <= 0) {
                Debug.WriteLine("[PatrocinioService] Patrocinador não logado.");
                return 0;
            }

            // 1. Preencher metadados da Campanha
            campanha.PatrocinadorId = usuarioAtual.Id;
            campanha.ClientAppId = Guid.NewGuid();
            campanha.IsSynced = false;
            campanha.UpdatedAt = DateTime.UtcNow;

            // 2. Inserir no banco de dados
            int result = await _databaseService.InserirCampanhaPatrocinioAsync(campanha);

            if (result > 0) {
                Debug.WriteLine($"[PatrocinioService] Campanha '{campanha.Nome}' salva localmente. Agendando sincronização...");
                _syncService.ScheduleSync();
            }
            return result;
        }


        // =========================================================
        // MÉTODOS DE PROPOSTA (Patrocinador propõe um Patrocínio a um Campeonato)
        // =========================================================

        /// <summary>
        /// Cria e salva uma nova Proposta de Patrocínio para um Campeonato.
        /// </summary>
        public async Task<int> CriarPropostaPatrocinioAsync(int campeonatoId, string mensagem) {
            var usuarioAtual = _sessaoService.GetUsuarioAtual();
            if (usuarioAtual == null || usuarioAtual.Id <= 0) {
                Debug.WriteLine("[PatrocinioService] Patrocinador não logado.");
                return 0;
            }

            var proposta = new PropostaPatrocinio {
                PatrocinadorId = usuarioAtual.Id,
                CampeonatoId = campeonatoId,
                Mensagem = mensagem,
                Aprovada = false, // Sempre começa como não aprovada
                ClientAppId = Guid.NewGuid(),
                IsSynced = false,
                UpdatedAt = DateTime.UtcNow
            };

            int result = await _databaseService.InserirPropostaPatrocinioAsync(proposta);

            if (result > 0) {
                Debug.WriteLine($"[PatrocinioService] Proposta de Patrocínio para o Campeonato {campeonatoId} salva localmente. Agendando sincronização...");
                _syncService.ScheduleSync();
            }
            return result;
        }

        public async Task<List<PropostaPatrocinio>> ObterPropostasDoPatrocinadorAsync() {
            var usuarioAtual = _sessaoService.GetUsuarioAtual();

            // 1. Verifica se o usuário está logado e se é um Patrocinador
            if (usuarioAtual == null || usuarioAtual.Perfil != TipoPerfil.Patrocinador) {
                Debug.WriteLine("[PatrocinioService] Patrocinador não logado ou perfil incorreto para obter propostas.");
                return new List<PropostaPatrocinio>();
            }

            try {
                // 2. Busca todas as propostas no banco de dados
                var todasPropostas = await _databaseService.ListarPropostasPatrocinioAsync();
                // ^ Assumindo que você tem este método no seu DatabaseService

                // 3. Filtra apenas as propostas do usuário logado
                var propostasDoUsuario = todasPropostas
                    .Where(p => p.PatrocinadorId == usuarioAtual.Id)
                    .ToList();

                return propostasDoUsuario;

            } catch (Exception ex) {
                Debug.WriteLine($"[PatrocinioService] Erro ao obter propostas do patrocinador: {ex.Message}");
                return new List<PropostaPatrocinio>();
            }
        }
        // TODO: Implementar ObterPropostasPorPatrocinadorAsync
        // TODO: Implementar ObterPropostasPorCampeonatoAsync
    }
}