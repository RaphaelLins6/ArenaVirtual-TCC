using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ArenaVirtual.Views;
using System.Diagnostics;
using System;
using System.Threading.Tasks;

namespace ArenaVirtual.ViewModels {
    // Certifique-se de que a classe seja 'partial' para que o código gerado funcione
    public partial class RegisterViewModel(IAlertService alertService, UsuarioService usuarioService, SyncService syncService) : ObservableObject {

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private string nome = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        [ObservableProperty]
        private string confirmarSenha = string.Empty;

        // Use [NotifyPropertyChangedFor] para atualizar a propriedade IsPatrocinador quando o perfil muda
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPatrocinador))]
        private TipoPerfil perfilSelecionado = TipoPerfil.Atleta;

        // Novos campos
        [ObservableProperty]
        private string telefone = string.Empty;

        [ObservableProperty]
        private string localizacao = string.Empty;

        [ObservableProperty]
        private string linkRedeSocial = string.Empty;

        [ObservableProperty]
        private string nomeEmpresa = string.Empty;

        [ObservableProperty]
        private string cnpj = string.Empty;

        [ObservableProperty]
        private string modalidades = string.Empty;

        [ObservableProperty]
        private string areasInteressePatrocinio = string.Empty;

        [ObservableProperty]
        private string faixaOrcamentoPatrocinio = string.Empty;

        [ObservableProperty]
        private double? peso = null;

        [ObservableProperty]
        private double? altura = null;

        [ObservableProperty]
        private DateTime? dataNascimento = null;

        [ObservableProperty]
        private GeneroEnum? generoSelecionado = null;

        public ObservableCollection<TipoPerfil> PerfisDisponiveis { get; } = new ObservableCollection<TipoPerfil>(Enum.GetValues<TipoPerfil>());
        public ObservableCollection<GeneroEnum> GenerosDisponiveis { get; } = new ObservableCollection<GeneroEnum>(Enum.GetValues<GeneroEnum>());

        private readonly IAlertService _alertService = alertService;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly SyncService _syncService = syncService;

        // Propriedade de conveniência que agora é observável graças ao NotifyPropertyChangedFor
        public bool IsPatrocinador => PerfilSelecionado == TipoPerfil.Patrocinador;

        [RelayCommand]
        public async Task Registrar() {
            try {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha) || string.IsNullOrWhiteSpace(ConfirmarSenha)) {
                    await _alertService.DisplayAlert("Campos Vazios", "Por favor, preencha todos os campos obrigatórios.", "OK");
                    return;
                }

                if (Senha != ConfirmarSenha) {
                    await _alertService.DisplayAlert("Senhas Diferentes", "As senhas não coincidem.", "OK");
                    return;
                }

                if (!Email.Contains('@') || !Email.Contains('.')) {
                    await _alertService.DisplayAlert("E-mail Inválido", "Por favor, insira um e-mail válido.", "OK");
                    return;
                }

                var usuario = new Usuario {
                    Nome = this.Nome,
                    Email = this.Email,
                    SenhaHash = UsuarioService.GerarHash(this.Senha),
                    Perfil = this.PerfilSelecionado,
                    Telefone = this.Telefone,
                    Localizacao = this.Localizacao,
                    LinkRedeSocial = this.LinkRedeSocial,
                    NomeEmpresa = this.NomeEmpresa,
                    CNPJ = this.Cnpj,
                    FaixaOrcamentoPatrocinio = this.FaixaOrcamentoPatrocinio,
                    Peso = this.Peso,
                    Altura = this.Altura,
                    DataNascimento = this.DataNascimento,
                    Genero = this.GeneroSelecionado
                };

                Usuario? usuarioCadastrado = await _usuarioService.Cadastrar(usuario);

                if (usuarioCadastrado != null) {
                    await _alertService.DisplayAlert("Sucesso", "Usuário registrado com sucesso!", "OK");

                    // Limpa os campos após o registro
                    Nome = string.Empty;
                    Email = string.Empty;
                    Senha = string.Empty;
                    ConfirmarSenha = string.Empty;
                    PerfilSelecionado = TipoPerfil.Atleta;
                    Telefone = string.Empty;
                    Localizacao = string.Empty;
                    LinkRedeSocial = string.Empty;
                    NomeEmpresa = string.Empty;
                    Cnpj = string.Empty;
                    Modalidades = string.Empty;
                    AreasInteressePatrocinio = string.Empty;
                    FaixaOrcamentoPatrocinio = string.Empty;
                    Peso = null;
                    Altura = null;
                    DataNascimento = null;
                    GeneroSelecionado = null;

                    SessaoService.Instancia.Login(usuarioCadastrado);
                    System.Diagnostics.Debug.WriteLine($"[RegisterViewModel] SessaoService.Instancia.Login() chamado para ID: {usuarioCadastrado.Id}, Email: {usuarioCadastrado.Email}");

                    Debug.WriteLine("[RegisterViewModel] Usuário registrado e logado. Disparando sincronização inicial completa.");
                    try {
                        await _syncService.SyncAsync(new Progress<string>());
                    } catch (Exception ex) {
                        Debug.WriteLine($"[RegisterViewModel] Erro na sincronização pós-registro: {ex.Message}");
                    }

                    MainThread.BeginInvokeOnMainThread(() => {
                        if (Application.Current?.Windows.Count > 0) {
                            Application.Current.Windows[0].Page = new AppShell(usuarioCadastrado);
                        }
                    });
                } else {
                    await _alertService.DisplayAlert("Erro", "Email já cadastrado ou falha ao registrar.", "OK");
                }
            } finally {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public static async Task VoltarParaLogin() {
            var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;
            if (serviceProvider != null) {
                var loginPage = serviceProvider.GetService<LoginPage>();
                if (loginPage != null && Application.Current?.Windows.Count > 0) {
                    Application.Current.Windows[0].Page = loginPage;
                }
            }
            await Task.CompletedTask;
        }
    }
}
