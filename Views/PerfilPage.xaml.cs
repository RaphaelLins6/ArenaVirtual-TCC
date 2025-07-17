using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using ArenaVirtual.ViewModels;

namespace ArenaVirtual.Views {
    public partial class PerfilPage : ContentPage {
        private Usuario _usuario;
        private readonly IAlertService _alertService;

        public PerfilPage(Usuario usuarioLogado, IAlertService alertService, IServiceProvider serviceProvider) {
            InitializeComponent();
            _usuario = usuarioLogado;
            _alertService = alertService;
            BindingContext = ActivatorUtilities.CreateInstance<PerfilViewModel>(serviceProvider, usuarioLogado, alertService);
        }

        private async void AlterarImagem_Clicked(object sender, EventArgs e) {
            var popup = new AlterarImagemPopup(_usuario, _alertService);
            popup.ImagemAtualizada += (s, novaImagemPath) => {
                var imagemPerfil = this.FindByName<Image>("ImagemPerfil");
                if (imagemPerfil != null) {
                    imagemPerfil.Source = novaImagemPath; // 
                }
            };
            await Navigation.PushModalAsync(popup);
        }

        private async void EditarPerfil_Clicked(object sender, EventArgs e) {
            var popup = new EditarPerfilPopup(_usuario, _alertService);
            popup.PerfilAtualizado += (s, usuarioAtualizado) => {
                _usuario = usuarioAtualizado;
            };
            await Navigation.PushModalAsync(popup);
        }

        private async void TrocarSenha_Clicked(object sender, EventArgs e) {
            var popup = new AlterarSenhaPopup(_usuario, _alertService);
            await Navigation.PushModalAsync(popup);
        }
    }
}