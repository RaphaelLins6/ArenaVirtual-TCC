using ArenaVirtual.Models;
using ArenaVirtual.Services;

namespace ArenaVirtual.Popups;

public partial class EditarPerfilPopup : ContentPage {
    private Usuario _usuario;
    private IAlertService _alertService;
    private DatabaseService _databaseService; // Agora será inicializado para não-nulo

    public event EventHandler<Usuario>? PerfilAtualizado;

    public EditarPerfilPopup(Usuario usuario, IAlertService alertService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;

        var serviceProvider = App.Current?.Handler?.MauiContext?.Services;
        if (serviceProvider != null) {
            _databaseService = serviceProvider.GetRequiredService<DatabaseService>();
        } else {
            throw new InvalidOperationException("DatabaseService not registered or app context is null.");
        }

        NomeEntry.Text = _usuario.Nome;
        EmailEntry.Text = _usuario.Email;
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        string novoNome = NomeEntry.Text?.Trim() ?? string.Empty;
        string novoEmail = EmailEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(novoNome) || string.IsNullOrWhiteSpace(novoEmail)) {
            await _alertService.DisplayAlert("Erro", "Nome e e-mail são obrigatórios.", "OK");
            return;
        }

        if (!novoEmail.Equals(_usuario.Email, StringComparison.OrdinalIgnoreCase)) {
            var emailExistente = await _databaseService.ListarUsuariosAsync();
            if (emailExistente.Any(u => u.Email != null && u.Email.Equals(novoEmail, StringComparison.OrdinalIgnoreCase))) {
                await _alertService.DisplayAlert("Erro", "Este e-mail já está em uso.", "OK");
                return;
            }
        }

        _usuario.Nome = novoNome;
        _usuario.Email = novoEmail;

        await _databaseService.AtualizarUsuarioAsync(_usuario);

        PerfilAtualizado?.Invoke(this, _usuario); // Dispara o evento de forma segura (?.Invoke)

        await _alertService.DisplayAlert("Sucesso", "Perfil atualizado com sucesso!", "OK");
        await Navigation.PopModalAsync();
    }
}