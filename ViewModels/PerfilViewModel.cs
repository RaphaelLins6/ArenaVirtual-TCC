using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

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

        // Se quiser tornar a imagem observável, descomente abaixo:
        // [ObservableProperty]
        // private ImageSource imagemPerfilSource;

        public PerfilViewModel(Usuario usuario, IAlertService alertService) {
            _usuarioLogado = usuario;
            _alertService = alertService;
            CarregarDadosDoUsuario();
        }

        public void CarregarDadosDoUsuario() {
            if (_usuarioLogado != null) {
                if (DateTime.Now.Hour < 12) Saudacao = "Bom dia,";
                else if (DateTime.Now.Hour < 18) Saudacao = "Boa tarde,";
                else Saudacao = "Boa noite,";

                NomeUsuario = _usuarioLogado.Nome;
                EmailUsuario = _usuarioLogado.Email;
                TipoPerfilUsuario = _usuarioLogado.Perfil.ToString();

                // Se quiser atualizar a imagem via ViewModel:
                // ImagemPerfilSource = string.IsNullOrEmpty(_usuarioLogado.ImagemPath)
                //     ? "default_profile.png"
                //     : ImageSource.FromFile(_usuarioLogado.ImagemPath);
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
            var popup = new AlterarSenhaPopup(_usuarioLogado, _alertService);
            await Shell.Current.Navigation.PushModalAsync(popup);
        }
    }
}
