using ArenaVirtual.Models;
using ArenaVirtual.Services;
using MvvmHelpers;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls; 

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
                if (usuarioAtual?.ClientAppId == Guid.Empty || usuarioAtual?.TimeClientAppId == null) {
                    await Application.Current.MainPage.DisplayAlert("Aviso", "Você não é o capitão de um time ou não pertence a um time.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                var timeDoUsuario = await _timeService.ObterPorClientAppIdAsync(usuarioAtual.TimeClientAppId.Value);
                if (timeDoUsuario == null || timeDoUsuario.CapitaoClientAppId != usuarioAtual.ClientAppId) {
                    await Application.Current.MainPage.DisplayAlert("Aviso", "Você não é o capitão do seu time para gerenciar solicitações.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                ConvitesPendentes.Clear();

                var convites = await _databaseService.ListarConvitesPendentesAsync(timeDoUsuario.ClientAppId);

                var tarefas = convites.Select(convite => _databaseService.ObterUsuarioPorClientAppIdAsync(convite.SolicitanteClientAppId)).ToList();
                var usuarios = await Task.WhenAll(tarefas);

                for (int i = 0; i < convites.Count; i++) {
                    var solicitante = usuarios[i];
                    if (solicitante != null) {
                        ConvitesPendentes.Add(new ConviteViewModel(convites[i], solicitante));
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

                if (usuarioSolicitante != null) {
                    usuarioSolicitante.TimeClientAppId = convite.TimeClientAppId;
                    await _databaseService.AtualizarUsuarioAsync(usuarioSolicitante);
                }

                ConvitesPendentes.Remove(conviteVM);

                await Application.Current.MainPage.DisplayAlert("Sucesso", $"O usuário {usuarioSolicitante?.Nome ?? "desconhecido"} foi adicionado ao time.", "OK");
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

                await Application.Current.MainPage.DisplayAlert("Aviso", $"O convite do usuário {conviteVM.UsuarioSolicitante?.Nome ?? "desconhecido"} foi recusado.", "OK");
            } finally {
                IsBusy = false;
            }
        }
    }
}