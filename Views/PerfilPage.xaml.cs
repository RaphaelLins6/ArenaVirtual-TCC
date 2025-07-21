using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels;

namespace ArenaVirtual.Views {
    public partial class PerfilPage : ContentPage {
        private readonly IAlertService _alertService;
        private readonly PerfilViewModel _viewModel;

        public PerfilPage(Usuario usuarioLogado, IAlertService alertService, IServiceProvider serviceProvider) {
            InitializeComponent();
            _alertService = alertService;
            _viewModel = ActivatorUtilities.CreateInstance<PerfilViewModel>(serviceProvider, usuarioLogado, alertService);
            BindingContext = _viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            _viewModel.CarregarDadosDoUsuario();
            AtualizarImagemPerfilUI();
            this.Focus();
        }

        private void AtualizarImagemPerfilUI() {
            var usuario = _viewModel.UsuarioLogado;
            var imagemPerfil = this.FindByName<Image>("ImagemPerfil");
            if (imagemPerfil != null && usuario != null) {
                var caminhoDaImagem = usuario.ImagemPath;
                if (!string.IsNullOrEmpty(caminhoDaImagem) && File.Exists(caminhoDaImagem)) {
                    try {
                        byte[] imageBytes = File.ReadAllBytes(caminhoDaImagem);
                        imagemPerfil.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                        System.Diagnostics.Debug.WriteLine($"[PerfilPage] Imagem carregada via FromStream de: {caminhoDaImagem}");
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"[PerfilPage] ERRO ao carregar imagem via FromStream: {ex.Message}");
                        imagemPerfil.Source = "default_profile.png";
                    }
                } else {
                    imagemPerfil.Source = "default_profile.png";
                }
            }
        }

        private async void AlterarImagem_Clicked(object sender, EventArgs e) {
            var usuario = _viewModel.UsuarioLogado;
            if (usuario == null) {
                await _alertService.DisplayAlert("Erro", "Nenhum usuário logado para alterar a imagem.", "OK");
                return;
            }
            var popup = new AlterarImagemPopup(usuario, _alertService);
            popup.ImagemAtualizada += AlterarImagemPopup_ImagemAtualizada;
            await Navigation.PushModalAsync(popup);
        }

        private void AlterarImagemPopup_ImagemAtualizada(object? sender, string novoCaminhoImagem) {
            var usuario = _viewModel.UsuarioLogado;
            MainThread.BeginInvokeOnMainThread(() => {
                usuario.ImagemPath = novoCaminhoImagem;
                var imagemPerfil = this.FindByName<Image>("ImagemPerfil");
                if (imagemPerfil != null && !string.IsNullOrEmpty(novoCaminhoImagem) && File.Exists(novoCaminhoImagem)) {
                    try {
                        byte[] imageBytes = File.ReadAllBytes(novoCaminhoImagem);
                        imagemPerfil.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                        System.Diagnostics.Debug.WriteLine($"[PerfilPage] Imagem carregada via FromStream de: {novoCaminhoImagem}");
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"[PerfilPage] ERRO ao carregar imagem via FromStream: {ex.Message}");
                        imagemPerfil.Source = "default_profile.png";
                    }
                } else {
                    imagemPerfil.Source = "default_profile.png";
                }
            });
            if (sender is AlterarImagemPopup popup) {
                popup.ImagemAtualizada -= AlterarImagemPopup_ImagemAtualizada;
            }
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
        }
    }
}