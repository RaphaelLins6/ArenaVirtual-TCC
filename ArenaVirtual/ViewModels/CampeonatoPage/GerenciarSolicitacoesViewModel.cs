using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ArenaVirtual.ViewModels.CampeonatoPage {
    public partial class GerenciarSolicitacoesViewModel : ObservableObject {

        private readonly DatabaseService _databaseService;
        private readonly CampeonatoService _campeonatoService;
        private readonly UsuarioService _usuarioService;

        [ObservableProperty]
        private ObservableCollection<Inscricao> _solicitacoesPendentes;

        public GerenciarSolicitacoesViewModel(DatabaseService databaseService, CampeonatoService campeonatoService, UsuarioService usuarioService) {
            _databaseService = databaseService;
            _campeonatoService = campeonatoService;
            _usuarioService = usuarioService;
            SolicitacoesPendentes = new ObservableCollection<Inscricao>();
        }

        [RelayCommand]
        public async Task LoadSolicitacoesAsync() {
            try {
                // Acesso à instância do serviço SessaoService através da propriedade estática.
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual == null) {
                    Debug.WriteLine("Usuário não logado. Não é possível carregar solicitações.");
                    return;
                }

                // 🚨 Pendente: Este método precisa ser implementado em CampeonatoService
                //var meuCampeonato = await _campeonatoService.ObterPorCapitaoClientAppIdAsync(usuarioAtual.ClientAppId);
                //if (meuCampeonato == null) {
                //    Debug.WriteLine("Usuário não é capitão de nenhum campeonato.");
                //    return;
                //}

                // 🚨 Pendente: Este método precisa ser implementado em DatabaseService
                //var solicitacoes = await _databaseService.ObterSolicitacoesPendentesPorCampeonatoAsync(meuCampeonato.ClientAppId);

                //MainThread.BeginInvokeOnMainThread(() => {
                //    SolicitacoesPendentes.Clear();
                //    foreach (var solicitacao in solicitacoes) {
                //        SolicitacoesPendentes.Add(solicitacao);
                //    }
                //});

            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao carregar solicitações: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task AceitarSolicitacaoAsync(Inscricao solicitacao) {
            try {
                // Altera o status da inscrição para "Aceita" e marca para sincronização
                solicitacao.Status = "Aceita";
                solicitacao.IsSynced = false;
                solicitacao.UpdatedAt = DateTime.UtcNow;

                // Atualiza a inscrição no banco de dados local
                //await _databaseService.AtualizarInscricaoAsync(solicitacao);

                // Remove a solicitação da lista exibida na interface
                SolicitacoesPendentes.Remove(solicitacao);

                Debug.WriteLine($"Solicitação do time {solicitacao.Time?.Nome} aceita com sucesso.");
                // Opcional: Acione a sincronização com o servidor
                // await _syncService.SyncAsync(new Progress<string>());
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao aceitar solicitação: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task RecusarSolicitacaoAsync(Inscricao solicitacao) {
            try {
                // Altera o status da inscrição para "Recusada" e marca para sincronização
                solicitacao.Status = "Recusada";
                solicitacao.IsSynced = false;
                solicitacao.UpdatedAt = DateTime.UtcNow;

                // Atualiza a inscrição no banco de dados local
                //await _databaseService.AtualizarInscricaoAsync(solicitacao);

                // Remove a solicitação da lista exibida na interface
                SolicitacoesPendentes.Remove(solicitacao);

                Debug.WriteLine($"Solicitação do time {solicitacao.Time?.Nome} recusada com sucesso.");
                // Opcional: Acione a sincronização com o servidor
                // await _syncService.SyncAsync(new Progress<string>());
            } catch (Exception ex) {
                Debug.WriteLine($"Erro ao recusar solicitação: {ex.Message}");
            }
        }
    }
}
