using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace ArenaVirtual.ViewModels {
    public partial class PerfilViewModel : ObservableObject {

        private readonly IAlertService _alertService;
        private readonly DatabaseService _databaseService;
        private readonly SyncService _syncService;

        private bool _isUpdating = false;

        [ObservableProperty] private string saudacao = string.Empty;
        [ObservableProperty] private string nomeUsuario = string.Empty;
        [ObservableProperty] private string emailUsuario = string.Empty;
        [ObservableProperty] private string tipoPerfilUsuario = string.Empty;
        [ObservableProperty] private string localizacaoUsuario = string.Empty;
        [ObservableProperty] private string telefoneUsuario = string.Empty;
        [ObservableProperty] private string linkRedeSocialUsuario = string.Empty;

        [ObservableProperty] private DateTime? dataNascimentoUsuario;
        [ObservableProperty] private GeneroEnum? generoUsuario;
        [ObservableProperty] private string nomeEmpresaUsuario = string.Empty;
        [ObservableProperty] private string cnpjUsuario = string.Empty;
        [ObservableProperty] private double? pesoUsuario;
        [ObservableProperty] private double? alturaUsuario = 0;
        [ObservableProperty] private string faixaOrcamentoPatrocinioUsuario = string.Empty;

        [ObservableProperty] private bool isAtleta;
        [ObservableProperty] private bool isOrganizador;
        [ObservableProperty] private bool isArbitro;
        [ObservableProperty] private bool isPatrocinador;

        [ObservableProperty]
        private Usuario usuarioLogado;

        [ObservableProperty]
        private ImageSource imagemPerfilSource;

        [ObservableProperty]
        private string? caminhoNovaImagemSelecionada;

        [ObservableProperty]
        private bool isBusy;

        public PerfilViewModel(IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
            _alertService = alertService;
            _databaseService = databaseService;
            _syncService = syncService;

            UsuarioLogado = SessaoService.Instancia.GetUsuarioAtual();
            CarregarDadosDoUsuario();

            MessagingCenter.Subscribe<object, Usuario>(this, "PerfilAtualizado", (sender, usuarioAtualizado) => {
                if (_isUpdating) return;

                MainThread.BeginInvokeOnMainThread(() => {
                    _isUpdating = true;
                    UsuarioLogado = usuarioAtualizado;
                    CarregarDadosDoUsuario();
                    _isUpdating = false;
                });
            });
        }

        [RelayCommand]
        private async Task AlterarImagem() {
            if (UsuarioLogado == null) {
                await _alertService.DisplayAlert("Erro", "Nenhum usuário logado para alterar a imagem.", "OK");
                return;
            }

            var services = App.Current?.Handler?.MauiContext?.Services;
            if (services == null) {
                await _alertService.DisplayAlert("Erro", "Serviços do aplicativo não estão disponíveis.", "OK");
                return;
            }

            var popup = services.GetRequiredService<AlterarImagemPopup>();
            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        public void CarregarDadosDoUsuario() {
            if (UsuarioLogado != null) {
                if (DateTime.Now.Hour < 12) Saudacao = "Bom dia,";
                else if (DateTime.Now.Hour < 18) Saudacao = "Boa tarde,";
                else Saudacao = "Boa noite,";

                NomeUsuario = UsuarioLogado.Nome;
                EmailUsuario = UsuarioLogado.Email;
                TipoPerfilUsuario = UsuarioLogado.Perfil.ToString();
                LocalizacaoUsuario = UsuarioLogado.Localizacao;
                TelefoneUsuario = UsuarioLogado.Telefone;
                LinkRedeSocialUsuario = UsuarioLogado.LinkRedeSocial;

                DataNascimentoUsuario = null;
                GeneroUsuario = null;
                NomeEmpresaUsuario = string.Empty;
                CnpjUsuario = string.Empty;
                PesoUsuario = null;
                AlturaUsuario = null;
                FaixaOrcamentoPatrocinioUsuario = string.Empty;

                switch (UsuarioLogado.Perfil) {
                    case TipoPerfil.Atleta:
                        DataNascimentoUsuario = UsuarioLogado.DataNascimento;
                        GeneroUsuario = UsuarioLogado.Genero;
                        PesoUsuario = UsuarioLogado.Peso;
                        AlturaUsuario = UsuarioLogado.Altura;
                        break;
                    case TipoPerfil.Organizador:
                        NomeEmpresaUsuario = UsuarioLogado.NomeEmpresa;
                        CnpjUsuario = UsuarioLogado.CNPJ;
                        break;
                    case TipoPerfil.Arbitro:
                        DataNascimentoUsuario = UsuarioLogado.DataNascimento;
                        GeneroUsuario = UsuarioLogado.Genero;
                        break;
                    case TipoPerfil.Patrocinador:
                        NomeEmpresaUsuario = UsuarioLogado.NomeEmpresa;
                        CnpjUsuario = UsuarioLogado.CNPJ;
                        FaixaOrcamentoPatrocinioUsuario = UsuarioLogado.FaixaOrcamentoPatrocinio;
                        break;
                }

                IsAtleta = UsuarioLogado.Perfil == TipoPerfil.Atleta;
                IsOrganizador = UsuarioLogado.Perfil == TipoPerfil.Organizador;
                IsArbitro = UsuarioLogado.Perfil == TipoPerfil.Arbitro;
                IsPatrocinador = UsuarioLogado.Perfil == TipoPerfil.Patrocinador;

                AtualizarImagemSource();
            }
        }

        private void AtualizarImagemSource() {
            var caminhoDaImagem = UsuarioLogado.ImagemPath;
            if (!string.IsNullOrEmpty(caminhoDaImagem) && File.Exists(caminhoDaImagem)) {
                try {
                    byte[] imageBytes = File.ReadAllBytes(caminhoDaImagem);
                    ImagemPerfilSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    System.Diagnostics.Debug.WriteLine($"[PerfilViewModel] Imagem carregada via FromStream de: {caminhoDaImagem}");
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"[PerfilViewModel] ERRO ao carregar imagem: {ex.Message}");
                    ImagemPerfilSource = "placeholder.png";
                }
            } else {
                ImagemPerfilSource = "placeholder.png";
            }
        }

        [RelayCommand]
        private async Task EditarPerfil() {
            var popup = new EditarPerfilPopup(UsuarioLogado, _alertService, _databaseService, _syncService);
            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        [RelayCommand]
        private async Task AlterarSenha() {
            var popup = new AlterarSenhaPopup(UsuarioLogado, _alertService, _databaseService, _syncService);
            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        [RelayCommand]
        public async Task SalvarImagem() {
            if (string.IsNullOrEmpty(CaminhoNovaImagemSelecionada)) {
                await _alertService.DisplayAlert("Aviso", "Por favor, escolha uma imagem primeiro.", "OK");
                return;
            }

            IsBusy = true;

            try {
                string diretorioImagens = FileSystem.AppDataDirectory;
                string nomeArquivo = Path.GetFileName(CaminhoNovaImagemSelecionada);
                string caminhoFinalImagem = Path.Combine(diretorioImagens, nomeArquivo);

                if (!File.Exists(caminhoFinalImagem)) {
                    File.Copy(CaminhoNovaImagemSelecionada, caminhoFinalImagem, true);
                }

                UsuarioLogado.ImagemPath = caminhoFinalImagem;
                UsuarioLogado.IsSynced = false;
                UsuarioLogado.UpdatedAt = DateTime.UtcNow;

                await _databaseService.AtualizarUsuarioAsync(UsuarioLogado);
                SessaoService.Instancia.SetUsuarioAtual(UsuarioLogado);

                await _syncService.SyncAsync(new Progress<string>());

                // AQUI ESTÁ A CORREÇÃO: Chamar o método para atualizar a imagem na UI
                // Esta chamada garante que o PerfilViewModel, que está ligado à tela
                // principal, vai atualizar a propriedade ImagemPerfilSource.
                AtualizarImagemSource();

                await _alertService.DisplayAlert("Sucesso", "Imagem de perfil atualizada!", "OK");

                await Shell.Current.Navigation.PopModalAsync();
            } catch (Exception ex) {
                await _alertService.DisplayAlert("Erro", $"Erro ao salvar imagem: {ex.Message}", "OK");
            } finally {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Cancelar() {
            // Este comando agora está na ViewModel para maior coesão
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
