using ArenaVirtual.Models;
using ArenaVirtual.Services;

namespace ArenaVirtual.Popups;

public partial class EditarPerfilPopup : ContentPage {
    private Usuario _usuario;
    private IAlertService _alertService;
    private DatabaseService _databaseService;

    public event EventHandler<Usuario> PerfilAtualizado;

    public EditarPerfilPopup(Usuario usuario, IAlertService alertService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;
        _databaseService = App.Current.Handler.MauiContext.Services.GetService<DatabaseService>();

        NomeEntry.Text = _usuario.Nome;
        EmailEntry.Text = _usuario.Email;
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        string novoNome = NomeEntry.Text?.Trim();
        string novoEmail = EmailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(novoNome) || string.IsNullOrWhiteSpace(novoEmail)) {
            await _alertService.DisplayAlert("Erro", "Nome e e-mail são obrigatórios.", "OK");
            return;
        }

        // Verificar se e-mail foi alterado e se já existe
        if (!novoEmail.Equals(_usuario.Email, StringComparison.OrdinalIgnoreCase)) {
            var emailExistente = await _databaseService.ListarUsuariosAsync();
            if (emailExistente.Any(u => u.Email.Equals(novoEmail, StringComparison.OrdinalIgnoreCase))) {
                await _alertService.DisplayAlert("Erro", "Este e-mail já está em uso.", "OK");
                return;
            }
        }

       _usuario.Nome = novoNome;
        _usuario.Email = novoEmail;

        await _databaseService.AtualizarUsuarioAsync(_usuario);

        PerfilAtualizado?.Invoke(this, _usuario);

        await _alertService.DisplayAlert("Sucesso", "Perfil atualizado com sucesso!", "OK");
        await Navigation.PopModalAsync();
    }
}
