using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace ArenaVirtual.ViewModels {
    public partial class RegisterViewModel : ObservableObject {
        [ObservableProperty] private string nome = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string senha = string.Empty;
        [ObservableProperty] private string confirmarSenha = string.Empty;
        [ObservableProperty] private TipoPerfil perfilSelecionado;

        public ObservableCollection<TipoPerfil> PerfisDisponiveis { get; }

        private readonly IAlertService _alertService;

        public RegisterViewModel(IAlertService alertService) {
            _alertService = alertService;
            PerfisDisponiveis = new ObservableCollection<TipoPerfil>(Enum.GetValues(typeof(TipoPerfil)).Cast<TipoPerfil>());
            this.perfilSelecionado = TipoPerfil.Atleta;
        }

        [RelayCommand]
        public async Task Registrar() {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha) || string.IsNullOrWhiteSpace(ConfirmarSenha)) {
                await _alertService.DisplayAlert("Campos Vazios", "Por favor, preencha todos os campos.", "OK");
                return;
            }

            if (Senha != ConfirmarSenha) {
                await _alertService.DisplayAlert("Senhas Diferentes", "As senhas não coincidem.", "OK");
                return;
            }

            if (!Email.Contains("@") || !Email.Contains(".")) {
                await _alertService.DisplayAlert("E-mail Inválido", "Por favor, insira um e-mail válido.", "OK");
                return;
            }

            var usuario = new Usuario {
                Nome = this.Nome,
                Email = this.Email,
                Senha = this.Senha,
                Perfil = this.PerfilSelecionado
            };

            Usuario? usuarioCadastrado = await UsuarioService.Cadastrar(usuario);

            if (usuarioCadastrado != null) {
                await _alertService.DisplayAlert("Sucesso", "Usuário registrado com sucesso!", "OK");

                Nome = string.Empty;
                Email = string.Empty;
                Senha = string.Empty;
                ConfirmarSenha = string.Empty;
                PerfilSelecionado = TipoPerfil.Atleta;

                MainThread.BeginInvokeOnMainThread(() => {
                    if (Application.Current?.Windows.Count > 0) {
                        Application.Current.MainPage = new AppShell(usuarioCadastrado);
                    }
                });
            } else {
                await _alertService.DisplayAlert("Erro", "Email já cadastrado ou falha ao registrar.", "OK");
            }
        }
    }
}