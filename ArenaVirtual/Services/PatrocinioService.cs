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

        public async Task<List<CampanhaPatrocinio>> ObterCampanhasAtivasAsync() {
            var usuarioAtual = _sessaoService.GetUsuarioAtual();
            if (usuarioAtual == null || usuarioAtual.Perfil != TipoPerfil.Patrocinador) {
                return new List<CampanhaPatrocinio>();
            }

            try {
                var todasCampanhas = await _databaseService.ListarCampanhasPatrocinioAsync();

                var campanhasAtivas = todasCampanhas
                    .Where(c => c.PatrocinadorId == usuarioAtual.Id && c.Fim >= DateTime.Now)
                    .ToList();

                return campanhasAtivas;
            } catch (Exception ex) {
                //Debug.WriteLine($"[PatrocinioService] Erro ao obter campanhas ativas: {ex.Message}");
                return new List<CampanhaPatrocinio>();
            }
        }

        public async Task<int> CriarCampanhaAsync(CampanhaPatrocinio campanha) {
            var usuarioAtual = _sessaoService.GetUsuarioAtual();
            if (usuarioAtual == null || usuarioAtual.Id <= 0) {
                //Debug.WriteLine("[PatrocinioService] Patrocinador não logado.");
                return 0;
            }

            campanha.PatrocinadorId = usuarioAtual.Id;
            campanha.ClientAppId = Guid.NewGuid();
            campanha.IsSynced = false;
            campanha.UpdatedAt = DateTime.UtcNow;

            int result = await _databaseService.InserirCampanhaPatrocinioAsync(campanha);

            if (result > 0) {
                //Debug.WriteLine($"[PatrocinioService] Campanha '{campanha.Nome}' salva localmente. Agendando sincronização...");
                _syncService.ScheduleSync();
            }
            return result;
        }

            public async Task<int> CriarPropostaPatrocinioAsync(int campeonatoId, decimal valor, string mensagem) {
            var usuarioAtual = _sessaoService.GetUsuarioAtual();
            if (usuarioAtual == null || usuarioAtual.Id <= 0) {
                //Debug.WriteLine("[PatrocinioService] Patrocinador não logado.");
                return 0;
            }

            var proposta = new PropostaPatrocinio {
                PatrocinadorId = usuarioAtual.Id,
                CampeonatoId = campeonatoId, 
                Mensagem = mensagem,
                ValorMonetario = valor,
                Aprovada = false, 
                ClientAppId = Guid.NewGuid(),
                IsSynced = false,
                UpdatedAt = DateTime.UtcNow
            };

            int result = await _databaseService.InserirPropostaPatrocinioAsync(proposta);

            if (result > 0) {
                //Debug.WriteLine($"[PatrocinioService] Proposta de Patrocínio para o Campeonato {campeonatoId} salva localmente. Agendando sincronização...");
                _syncService.ScheduleSync();
            }
            return result;
            }

        public async Task<List<PropostaPatrocinio>> ObterPropostasDoPatrocinadorAsync() {
            var usuarioAtual = _sessaoService.GetUsuarioAtual();

            if (usuarioAtual == null || usuarioAtual.Perfil != TipoPerfil.Patrocinador) {
                //Debug.WriteLine("[PatrocinioService] Patrocinador não logado ou perfil incorreto para obter propostas.");
                return new List<PropostaPatrocinio>();
            }

            try {
                var todasPropostas = await _databaseService.ListarPropostasPatrocinioAsync();

                var propostasDoUsuario = todasPropostas
                    .Where(p => p.PatrocinadorId == usuarioAtual.Id)
                    .ToList();

                return propostasDoUsuario;

            } catch (Exception ex) {
                //Debug.WriteLine($"[PatrocinioService] Erro ao obter propostas do patrocinador: {ex.Message}");
                return new List<PropostaPatrocinio>();
            }
        }
        public async Task<IEnumerable<PropostaPatrocinio>> ObterPropostasPendentesPorCampeonatoAsync(Guid campeonatoClientAppId) {
            try {
                var propostas = await _databaseService.ListarPropostasPatrocinioPorCampeonatoAsync(campeonatoClientAppId);

                return propostas.Where(p => !p.Aprovada);
            } catch (Exception ex) {
                //Debug.WriteLine($"[PatrocinioService] Erro ao obter propostas pendentes: {ex.Message}");
                return new List<PropostaPatrocinio>();
            }
        }

        public async Task<Usuario> ObterPatrocinadorPorIdAsync(int patrocinadorId) { 
            try {
                return await _databaseService.ObterUsuarioPorIdAsync(patrocinadorId); 
            } catch (Exception ex) {
                //Debug.WriteLine($"[PatrocinioService] Erro ao obter patrocinador/usuario: {ex.Message}");
                return null;
            }
        }

        public async Task AtualizarPropostaAsync(PropostaPatrocinio proposta) {
            try {
                proposta.UpdatedAt = DateTime.UtcNow;
                proposta.IsSynced = false;

                await _databaseService.AtualizarPropostaPatrocinioAsync(proposta);
                _syncService.ScheduleSync();
            } catch (Exception ex) {
                //Debug.WriteLine($"[PatrocinioService] Erro ao atualizar proposta: {ex.Message}");
                throw; 
            }
        }

        public async Task DeletarPropostaAsync(PropostaPatrocinio proposta) {
            try {
                await _databaseService.DeletarPropostaPatrocinioAsync(proposta); //
                _syncService.ScheduleSync();
            } catch (Exception ex) {
                //Debug.WriteLine($"[PatrocinioService] Erro ao deletar proposta: {ex.Message}");
                throw;
            }
        }

        public Task InserirCampanhaAsync(CampanhaPatrocinio campanha) {
            return _databaseService.InserirCampanhaAsync(campanha);
        }

        public async Task<CampanhaPatrocinio?> ObterCampanhaDeDivulgacaoAtivaAsync(Guid campeonatoClientAppId) {
            try {
                var todasCampanhas = await _databaseService.ListarCampanhasPatrocinioPorCampeonatoAsync(campeonatoClientAppId);

                if (todasCampanhas == null) {
                    //System.Diagnostics.Debug.WriteLine($"[PatrocinioService - Rastreio] Lista de Campanhas retornada pelo DB Service é nula para o Campeonato: {campeonatoClientAppId}");
                    return null; 
                }

                //System.Diagnostics.Debug.WriteLine($"[PatrocinioService - Rastreio] Total de Campanhas encontradas para {campeonatoClientAppId}: {todasCampanhas.Count}");

                var campanhaAtiva = todasCampanhas
                    .Where(c => c.Fim.AddDays(1) > DateTime.Now)
                    .OrderByDescending(c => c.Fim)
                    .FirstOrDefault();

                if (campanhaAtiva != null) {
                    //System.Diagnostics.Debug.WriteLine($"[PatrocinioService - Rastreio] Campanha ATIVA encontrada. ID: {campanhaAtiva.Id}, Nome: {campanhaAtiva.Nome}");
                } else {
                    //System.Diagnostics.Debug.WriteLine($"[PatrocinioService - Rastreio] Nenhuma Campanha ATIVA encontrada após filtro de data.");
                }

                return campanhaAtiva;

            } catch (Exception ex) {
                //System.Diagnostics.Debug.WriteLine($"[PatrocinioService - FALHA] Erro CRÍTICO ao obter campanha de divulgação ativa: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ExisteCampanhaAtivaNoCampeonatoAsync(Guid campeonatoClientAppId) {
            try {
                var campanhaAtiva = await ObterCampanhaDeDivulgacaoAtivaAsync(campeonatoClientAppId);

                return campanhaAtiva != null;

            } catch (Exception ex) {
                //Debug.WriteLine($"[PatrocinioService] Erro ao verificar campanha ativa: {ex.Message}");
                return false;
            }
        }
    }
}