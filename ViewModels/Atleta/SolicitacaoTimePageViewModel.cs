using ArenaVirtual.Models;
using ArenaVirtual.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels.Atleta {
    public class SolicitacaoTimePageViewModel : BaseViewModel {
        private readonly DatabaseService _databaseService;
        private readonly UsuarioService _usuarioService;
        private readonly TimeService _timeService;

        public ObservableCollection<Usuario> MembrosDoTime { get; set; } = [];
        public ObservableCollection<ConviteViewModel> ConvitesPendentes { get; set; } = [];

        public ICommand AceitarConviteCommand { get; }
        public ICommand RecusarConviteCommand { get; }

        public SolicitacaoTimePageViewModel(DatabaseService databaseService, UsuarioService usuarioService, TimeService timeService) {
            _databaseService = databaseService;
            _usuarioService = usuarioService;
            _timeService = timeService;

            AceitarConviteCommand = new Command<ConviteViewModel>(async (conviteVM) => await AceitarConviteAsync(conviteVM));
            RecusarConviteCommand = new Command<ConviteViewModel>(async (conviteVM) => await RecusarConviteAsync(conviteVM));
        }

        public async Task LoadData() {
            if (IsBusy) return;
            IsBusy = true;
            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual?.TimeId == null) {
                    await Application.Current.MainPage.DisplayAlert("Aviso", "Você não é capitão de um time.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // Limpar coleções
                MembrosDoTime.Clear();
                ConvitesPendentes.Clear();

                // Carregar membros do time
                var membros = await _usuarioService.GetMembrosByTimeIdAsync(usuarioAtual.TimeId.Value);
                foreach (var membro in membros) {
                    MembrosDoTime.Add(membro);
                }

                // Carregar convites pendentes
                var convites = await _databaseService.ListarConvitesPendentesAsync(usuarioAtual.TimeId.Value);
                foreach (var convite in convites) {
                    var solicitante = await _usuarioService.ObterUsuarioPorIdAsync(convite.IdSolicitante);
                    if (solicitante != null) {
                        ConvitesPendentes.Add(new ConviteViewModel(convite, solicitante));
                    }
                }

            } finally {
                IsBusy = false;
            }
        }

        private async Task AceitarConviteAsync(ConviteViewModel conviteVM) {
            IsBusy = true;
            try {
                // Obter o convite original e o usuário solicitante
                var convite = conviteVM.ConviteOriginal;
                var usuarioSolicitante = conviteVM.UsuarioSolicitante;

                // Mude o status do convite para Aceito
                convite.Status = StatusConvite.Aceito;
                await _databaseService.AtualizarConviteAsync(convite);

                // Vincule o usuário ao time
                usuarioSolicitante.TimeId = convite.IdTime;
                await _databaseService.AtualizarUsuarioAsync(usuarioSolicitante);

                // Recarregue os dados para atualizar a UI
                await LoadData();

                await Application.Current.MainPage.DisplayAlert("Sucesso", $"O usuário {usuarioSolicitante.Nome} foi adicionado ao time.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        private async Task RecusarConviteAsync(ConviteViewModel conviteVM) {
            IsBusy = true;
            try {
                // Obter o convite original
                var convite = conviteVM.ConviteOriginal;

                // Mude o status do convite para Recusado
                convite.Status = StatusConvite.Recusado;
                await _databaseService.AtualizarConviteAsync(convite);

                // Recarregue os dados para atualizar a UI
                await LoadData();

                await Application.Current.MainPage.DisplayAlert("Aviso", $"O convite do usuário {conviteVM.UsuarioSolicitante.Nome} foi recusado.", "OK");
            } finally {
                IsBusy = false;
            }
        }
    }
}