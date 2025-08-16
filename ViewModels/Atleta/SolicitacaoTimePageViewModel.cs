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

        public ObservableCollection<ConviteViewModel> ConvitesPendentes { get; set; } = new ObservableCollection<ConviteViewModel>();

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

                ConvitesPendentes.Clear();

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
                var convite = conviteVM.ConviteOriginal;
                var usuarioSolicitante = conviteVM.UsuarioSolicitante;

                convite.Status = StatusConvite.Aceito;
                await _databaseService.AtualizarConviteAsync(convite);

                usuarioSolicitante.TimeId = convite.IdTime;
                await _databaseService.AtualizarUsuarioAsync(usuarioSolicitante);

                ConvitesPendentes.Remove(conviteVM);

                await Application.Current.MainPage.DisplayAlert("Sucesso", $"O usuário {usuarioSolicitante.Nome} foi adicionado ao time.", "OK");
            } finally {
                IsBusy = false;
            }
        }

        private async Task RecusarConviteAsync(ConviteViewModel conviteVM) {
            IsBusy = true;
            try {
                var convite = conviteVM.ConviteOriginal;

                convite.Status = StatusConvite.Recusado;
                await _databaseService.AtualizarConviteAsync(convite);

                ConvitesPendentes.Remove(conviteVM);

                await Application.Current.MainPage.DisplayAlert("Aviso", $"O convite do usuário {conviteVM.UsuarioSolicitante.Nome} foi recusado.", "OK");
            } finally {
                IsBusy = false;
            }
        }
    }
}