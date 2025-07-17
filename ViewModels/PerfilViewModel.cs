using System;
using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArenaVirtual.ViewModels {
    public partial class PerfilViewModel : ObservableObject {
        private readonly IAlertService _alertService;
        private Usuario _usuarioLogado;

        [ObservableProperty]
        private string saudacao = string.Empty;

        [ObservableProperty]
        private string nomeUsuario = string.Empty;

        [ObservableProperty]
        private string emailUsuario = string.Empty;

        [ObservableProperty]
        private string tipoPerfilUsuario = string.Empty;

        [ObservableProperty]
        private string localizacaoUsuario = string.Empty;

        public PerfilViewModel(Usuario usuario, IAlertService alertService) {
            _usuarioLogado = usuario;
            _alertService = alertService;
            CarregarDadosDoUsuario();
        }

        private void CarregarDadosDoUsuario() {
            if (_usuarioLogado != null) {
                if (DateTime.Now.Hour < 12) Saudacao = "Bom dia,";
                else if (DateTime.Now.Hour < 18) Saudacao = "Boa tarde,";
                else Saudacao = "Boa noite,";

                NomeUsuario = _usuarioLogado.Nome;
                EmailUsuario = _usuarioLogado.Email;
                TipoPerfilUsuario = _usuarioLogado.Perfil.ToString();
            }
        }

        [RelayCommand]
        private async Task EditarPerfil() {
            var popup = new EditarPerfilPopup(_usuarioLogado, _alertService);
            popup.PerfilAtualizado += (s, usuarioAtualizado) => {
                _usuarioLogado = usuarioAtualizado;
                CarregarDadosDoUsuario();
            };

            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        [RelayCommand]
        private async Task AlterarSenha() {
            await _alertService.DisplayAlert("Alterar Senha", "Funcionalidade de alteração de senha em desenvolvimento.", "OK");
        }
    }
}
