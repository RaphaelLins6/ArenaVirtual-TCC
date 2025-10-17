using ArenaVirtual.Models;
using System.Diagnostics;
using ArenaVirtual.ViewModels.Patrocinador;
using ArenaVirtual.Views.Patrocinador;

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
                // Assumindo que ListarCampanhasPatrocinioAsync existe no DatabaseService
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
            // Assumindo que InserirCampanhaPatrocinioAsync existe no DatabaseService
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
                CampeonatoId = campeonatoId, // Note: O correto aqui seria o ClientAppId do Campeonato, dependendo da sua arquitetura
                Mensagem = mensagem,
                Aprovada = false, // Sempre começa como não aprovada
                ClientAppId = Guid.NewGuid(),
                IsSynced = false,
                UpdatedAt = DateTime.UtcNow
            };

            // Assumindo que InserirPropostaPatrocinioAsync existe no DatabaseService
            int result = await _databaseService.InserirPropostaPatrocinioAsync(proposta);

            if (result > 0) {
                Debug.WriteLine($"[PatrocinioService] Proposta de Patrocínio para o Campeonato {campeonatoId} salva localmente. Agendando sincronização...");
                _syncService.ScheduleSync();
            }
            return result;
        }

        /// <summary>
        /// Obtém todas as propostas feitas pelo Patrocinador logado.
        /// </summary>
        public async Task<List<PropostaPatrocinio>> ObterPropostasDoPatrocinadorAsync() {
            var usuarioAtual = _sessaoService.GetUsuarioAtual();

            if (usuarioAtual == null || usuarioAtual.Perfil != TipoPerfil.Patrocinador) {
                Debug.WriteLine("[PatrocinioService] Patrocinador não logado ou perfil incorreto para obter propostas.");
                return new List<PropostaPatrocinio>();
            }

            try {
                // Assumindo que ListarPropostasPatrocinioAsync existe no DatabaseService
                var todasPropostas = await _databaseService.ListarPropostasPatrocinioAsync();

                var propostasDoUsuario = todasPropostas
                    .Where(p => p.PatrocinadorId == usuarioAtual.Id)
                    .ToList();

                return propostasDoUsuario;

            } catch (Exception ex) {
                Debug.WriteLine($"[PatrocinioService] Erro ao obter propostas do patrocinador: {ex.Message}");
                return new List<PropostaPatrocinio>();
            }
        }

        // =========================================================
        // MÉTODOS ADICIONADOS PARA O GerenciarSolicitacoesViewModel
        // =========================================================

        public async Task<IEnumerable<PropostaPatrocinio>> ObterPropostasPendentesPorCampeonatoAsync(Guid campeonatoClientAppId) {
            try {
                // Assumindo que você tem um método no DatabaseService que obtém as propostas por Campeonato ClientAppId
                var propostas = await _databaseService.ListarPropostasPatrocinioPorCampeonatoAsync(campeonatoClientAppId);

                // Filtra apenas as propostas que ainda não foram aprovadas
                return propostas.Where(p => !p.Aprovada);
            } catch (Exception ex) {
                Debug.WriteLine($"[PatrocinioService] Erro ao obter propostas pendentes: {ex.Message}");
                return new List<PropostaPatrocinio>();
            }
        }

        public async Task<Usuario> ObterPatrocinadorPorIdAsync(int patrocinadorId) { // << CORREÇÃO: Mudar retorno para Usuario
            try {
                // Chama o método do DatabaseService que busca um Usuario por ID
                // Assumindo que ObterUsuarioPorIdAsync existe e é público no DatabaseService (ou fazemos a chamada direta abaixo)
                return await _databaseService.ObterUsuarioPorIdAsync(patrocinadorId); // << CORREÇÃO: Chamar o método correto
            } catch (Exception ex) {
                Debug.WriteLine($"[PatrocinioService] Erro ao obter patrocinador/usuario: {ex.Message}");
                return null;
            }
        }

        public async Task AtualizarPropostaAsync(PropostaPatrocinio proposta) {
            try {
                proposta.UpdatedAt = DateTime.UtcNow;
                proposta.IsSynced = false;

                // Assumindo que AtualizarPropostaPatrocinioAsync existe no DatabaseService
                await _databaseService.AtualizarPropostaPatrocinioAsync(proposta);
                _syncService.ScheduleSync();
            } catch (Exception ex) {
                Debug.WriteLine($"[PatrocinioService] Erro ao atualizar proposta: {ex.Message}");
                throw; // Re-lança para que o ViewModel possa tratar o erro.
            }
        }

        public async Task DeletarPropostaAsync(PropostaPatrocinio proposta) {
            try {
                // CORREÇÃO: Usar o nome correto do método no DatabaseService
                await _databaseService.DeletarPropostaPatrocinioAsync(proposta); //
                _syncService.ScheduleSync();
            } catch (Exception ex) {
                Debug.WriteLine($"[PatrocinioService] Erro ao deletar proposta: {ex.Message}");
                throw; // Re-lança para que o ViewModel possa tratar o erro.
            }
        }

        public Task InserirCampanhaAsync(CampanhaPatrocinio campanha) {
            // Chama o método que você implementou no DatabaseService
            return _databaseService.InserirCampanhaAsync(campanha);
        }
    }
}