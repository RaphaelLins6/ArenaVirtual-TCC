using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace ArenaVirtual.ViewModels {
    public partial class RegisterViewModel : ObservableObject {
        [ObservableProperty] private string nome = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string senha = string.Empty;
        [ObservableProperty] private string confirmarSenha = string.Empty;
        [ObservableProperty] private TipoPerfil perfilSelecionado;

        public ObservableCollection<TipoPerfil> PerfisDisponiveis { get; }

        public RegisterViewModel() {
            PerfisDisponiveis = new ObservableCollection<TipoPerfil>(Enum.GetValues(typeof(TipoPerfil)).Cast<TipoPerfil>());
            this.perfilSelecionado = TipoPerfil.Atleta;
        }

        [RelayCommand]
        public async Task Registrar() {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha) || string.IsNullOrWhiteSpace(ConfirmarSenha)) {
                await Toast.Make("Por favor, preencha todos os campos.", ToastDuration.Short).Show();
                return;
            }

            if (Senha != ConfirmarSenha) {
                await Toast.Make("A senha e a confirmação de senha não coincidem.", ToastDuration.Short).Show();
                return;
            }

            if (!Email.Contains("@") || !Email.Contains(".")) {
                await Toast.Make("Por favor, insira um e-mail válido.", ToastDuration.Short).Show();
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
                
                
                await Toast.Make("Usuário registrado com sucesso!", ToastDuration.Short).Show();

                // Limpa os campos
                Nome = string.Empty;
                Email = string.Empty;
                Senha = string.Empty;
                ConfirmarSenha = string.Empty;
                PerfilSelecionado = TipoPerfil.Atleta;

                if (Application.Current?.Windows.Count > 0) {
                    Application.Current.MainPage = new AppShell(usuarioCadastrado);
                }
            } else {
                await Toast.Make("Email já cadastrado ou erro ao registrar.", ToastDuration.Short).Show();
            }
        }
    }
}