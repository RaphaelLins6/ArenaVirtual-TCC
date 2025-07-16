using System;
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArenaVirtual.ViewModels {
    public partial class PerfilViewModel : ObservableObject {
        private readonly IAlertService _alertService;
        private Usuario _usuarioLogado;

        [ObservableProperty]
        private string saudacao;

        [ObservableProperty]
        private string nomeUsuario;

        [ObservableProperty]
        private string emailUsuario;

        [ObservableProperty]
        private string tipoPerfilUsuario;

        [ObservableProperty]
        private string localizacaoUsuario; // Exemplo de propriedade adicional  

        public PerfilViewModel(Usuario usuario, IAlertService alertService) {
            _usuarioLogado = usuario;
            _alertService = alertService;
            CarregarDadosDoUsuario();
        }

        private void CarregarDadosDoUsuario() {
            if (_usuarioLogado != null) {
                if (DateTime.Now.Hour < 12) saudacao = "Bom dia,";
                else if (DateTime.Now.Hour < 18) saudacao = "Boa tarde,";
                else saudacao = "Boa noite,";

                NomeUsuario = _usuarioLogado.Nome;
                EmailUsuario = _usuarioLogado.Email;
                TipoPerfilUsuario = _usuarioLogado.Perfil.ToString();
                // LocalizacaoUsuario = _usuarioLogado.Localizacao; // Descomente se existir  
            }
        }

        [RelayCommand]
        private async Task EditarPerfil() {
            await _alertService.DisplayAlert("Editar Perfil", "Funcionalidade de edição de perfil em desenvolvimento.", "OK");
        }

        [RelayCommand]
        private async Task AlterarSenha() {
            await _alertService.DisplayAlert("Alterar Senha", "Funcionalidade de alteração de senha em desenvolvimento.", "OK");
        }
    }
}
