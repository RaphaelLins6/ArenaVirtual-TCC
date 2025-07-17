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
                    imagemPerfil.Source = novaImagemPath; // x:Name="ImagemPerfil" na sua Image
                }
            };
            await Navigation.PushModalAsync(popup);
        }

        private async void EditarPerfil_Clicked(object sender, EventArgs e) {
            var popup = new EditarPerfilPopup(_usuario, _alertService);
            popup.PerfilAtualizado += (s, usuarioAtualizado) => {
                _usuario = usuarioAtualizado;
                // Atualize os labels conforme necessário, por exemplo:
                // NomeLabel.Text = _usuario.Nome;
                // EmailLabel.Text = _usuario.Email;
            };
            await Navigation.PushModalAsync(popup);
        }

        private async void TrocarSenha_Clicked(object sender, EventArgs e) {
            var popup = new TrocarSenhaPopup(_usuario, _alertService);
            await Navigation.PushModalAsync(popup);
        }
    }

    public partial class TrocarSenhaPopup : ContentPage {
        private readonly Usuario _usuario;
        private readonly IAlertService _alertService;

        public TrocarSenhaPopup(Usuario usuario, IAlertService alertService) {
            _usuario = usuario;
            _alertService = alertService;

            Title = "Trocar Senha";

            var senhaAtualEntry = new Entry { Placeholder = "Senha atual", IsPassword = true };
            var novaSenhaEntry = new Entry { Placeholder = "Nova senha", IsPassword = true };
            var confirmarSenhaEntry = new Entry { Placeholder = "Confirmar nova senha", IsPassword = true };

            var trocarButton = new Button { Text = "Trocar Senha" };
            trocarButton.Clicked += async (s, e) => {
                if (string.IsNullOrWhiteSpace(senhaAtualEntry.Text) ||
                    string.IsNullOrWhiteSpace(novaSenhaEntry.Text) ||
                    string.IsNullOrWhiteSpace(confirmarSenhaEntry.Text)) {
                    await _alertService.DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                    return;
                }
                if (novaSenhaEntry.Text != confirmarSenhaEntry.Text) {
                    await _alertService.DisplayAlert("Erro", "As senhas não coincidem.", "OK");
                    return;
                }
                // Aqui você pode adicionar a lógica para validar a senha atual e atualizar a senha do usuário
                await _alertService.DisplayAlert("Sucesso", "Senha alterada com sucesso!", "OK");
                await Navigation.PopModalAsync();
            };

            Content = new VerticalStackLayout {
                Padding = 20,
                Spacing = 15,
                Children = {
                    senhaAtualEntry,
                    novaSenhaEntry,
                    confirmarSenhaEntry,
                    trocarButton
                }
            };
        }
    }
}