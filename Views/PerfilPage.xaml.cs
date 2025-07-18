using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace ArenaVirtual.Views {
    public partial class PerfilPage : ContentPage {
        private Usuario _usuario;
        private readonly IAlertService _alertService;
        private readonly PerfilViewModel _viewModel;

        public PerfilPage(Usuario usuarioLogado, IAlertService alertService, IServiceProvider serviceProvider) {
            InitializeComponent();
            _usuario = usuarioLogado;
            _alertService = alertService;
            _viewModel = ActivatorUtilities.CreateInstance<PerfilViewModel>(serviceProvider, usuarioLogado, alertService);
            BindingContext = _viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            _viewModel.CarregarDadosDoUsuario();
            AtualizarImagemPerfilUI();   
        }

        private void AtualizarImagemPerfilUI() {
            var imagemPerfil = this.FindByName<Image>("ImagemPerfil");
            if (imagemPerfil != null && _usuario != null) {
                var caminhoDaImagem = _usuario.ImagemPath;
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
            if (_usuario == null) {
                await _alertService.DisplayAlert("Erro", "Nenhum usuário logado para alterar a imagem.", "OK");
                return;
            }
            var popup = new AlterarImagemPopup(_usuario, _alertService);
            popup.ImagemAtualizada += AlterarImagemPopup_ImagemAtualizada;
            await Navigation.PushModalAsync(popup);
        }

        private void AlterarImagemPopup_ImagemAtualizada(object? sender, string novoCaminhoImagem) {
            MainThread.BeginInvokeOnMainThread(() => {
                _usuario.ImagemPath = novoCaminhoImagem;
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

        private async void EditarPerfil_Clicked(object sender, EventArgs e) {
            if (_usuario == null) {
                await _alertService.DisplayAlert("Erro", "Nenhum usuário logado para editar o perfil.", "OK");
                return;
            }
            var popup = new EditarPerfilPopup(_usuario, _alertService);
            popup.PerfilAtualizado += (s, usuarioAtualizado) => {
                _usuario = usuarioAtualizado;
                _viewModel.CarregarDadosDoUsuario();
            };
            await Navigation.PushModalAsync(popup);
        }

        private async void TrocarSenha_Clicked(object sender, EventArgs e) {
            if (_usuario == null) {
                await _alertService.DisplayAlert("Erro", "Nenhum usuário logado para trocar a senha.", "OK");
                return;
            }
            var popup = new AlterarSenhaPopup(_usuario, _alertService);
            await Navigation.PushModalAsync(popup);
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
        }
    }
}