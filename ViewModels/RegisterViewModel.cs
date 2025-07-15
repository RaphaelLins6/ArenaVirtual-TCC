using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArenaVirtual.ViewModels {
    public partial class RegisterViewModel : ObservableObject {
        private string nome = string.Empty;
        public string Nome {
            get => nome;
            set => SetProperty(ref nome, value);
        }

        private string email = string.Empty;
        public string Email {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string senha = string.Empty;
        public string Senha {
            get => senha;
            set => SetProperty(ref senha, value);
        }

        private TipoPerfil perfilSelecionado;
        public TipoPerfil PerfilSelecionado {
            get => perfilSelecionado;
            set => SetProperty(ref perfilSelecionado, value);
        }

        public ObservableCollection<TipoPerfil> PerfisDisponiveis { get; }

        public RegisterViewModel() {
            PerfisDisponiveis = new ObservableCollection<TipoPerfil>(Enum.GetValues(typeof(TipoPerfil)).Cast<TipoPerfil>());
            this.PerfilSelecionado = TipoPerfil.Atleta;
        }

        [RelayCommand]
        public async Task Registrar() {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha)) {
                await Shell.Current.DisplayAlert("Erro de Registro", "Por favor, preencha todos os campos e selecione um tipo de perfil.", "OK");
                return;
            }

            var usuario = new Usuario {
                Nome = this.nome,
                Email = this.email,
                Senha = this.senha,
                Perfil = this.perfilSelecionado
            };

            Usuario? usuarioCadastrado = await UsuarioService.Cadastrar(usuario);

            if (usuarioCadastrado != null) {
                await Shell.Current.DisplayAlert("Sucesso", "Usuário registrado com sucesso!", "OK");
                if (Application.Current?.Windows.Count > 0) {
                    Application.Current.Windows[0].Page = new AppShell(usuarioCadastrado);
                }
            } else {
                await Shell.Current.DisplayAlert("Erro", "Email já cadastrado ou erro ao registrar.", "OK");
            }
        }
    }
}