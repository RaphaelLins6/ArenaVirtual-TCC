using ArenaVirtual.Models;
using ArenaVirtual.Services;

namespace ArenaVirtual.Popups;

public partial class EditarPerfilPopup : ContentPage {
    private readonly Usuario _usuario; // Esta instância é a mesma do ViewModel

    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;

    public EditarPerfilPopup(Usuario usuario, IAlertService alertService) {
        InitializeComponent();
        _usuario = usuario; // Recebe a referência do ViewModel.UsuarioLogado
        _alertService = alertService;

        var serviceProvider = App.Current?.Handler?.MauiContext?.Services;
        if (serviceProvider != null) {
            _databaseService = serviceProvider.GetRequiredService<DatabaseService>();
        } else {
            throw new InvalidOperationException("DatabaseService not registered or app context is null.");
        }

        // Preenche campos comuns
        NomeEntry.Text = _usuario.Nome;
        EmailEntry.Text = _usuario.Email;
        TelefoneEntry.Text = _usuario.Telefone;
        LocalizacaoEntry.Text = _usuario.Localizacao;
        LinkRedeSocialEntry.Text = _usuario.LinkRedeSocial;

        // Visibilidade por perfil
        AtletaSection.IsVisible = _usuario.Perfil == TipoPerfil.Atleta;
        OrganizadorSection.IsVisible = _usuario.Perfil == TipoPerfil.Organizador;
        PatrocinadorSection.IsVisible = _usuario.Perfil == TipoPerfil.Patrocinador;

        // Campos específicos
        if (_usuario.Perfil == TipoPerfil.Atleta) {
            DataNascimentoPicker.Date = _usuario.DataNascimento ?? DateTime.Now;
            GeneroPicker.SelectedItem = _usuario.Genero?.ToString();
            PesoEntry.Text = _usuario.Peso?.ToString();
            AlturaEntry.Text = _usuario.Altura?.ToString();
        }
        if (_usuario.Perfil == TipoPerfil.Organizador) {
            NomeEmpresaEntry.Text = _usuario.NomeEmpresa;
            CnpjEntry.Text = _usuario.CNPJ;
        }
        if (_usuario.Perfil == TipoPerfil.Patrocinador) {
            FaixaOrcamentoPatrocinioEntry.Text = _usuario.FaixaOrcamentoPatrocinio;
        }

        if (GeneroPicker != null) {
            GeneroPicker.ItemsSource = Enum.GetNames<GeneroEnum>().ToList();
            if (_usuario.Genero != null)
                GeneroPicker.SelectedItem = _usuario.Genero.ToString();
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        _usuario.Nome = NomeEntry.Text?.Trim() ?? string.Empty;
        _usuario.Email = EmailEntry.Text?.Trim() ?? string.Empty;
        _usuario.Telefone = TelefoneEntry.Text?.Trim() ?? string.Empty;
        _usuario.Localizacao = LocalizacaoEntry.Text?.Trim() ?? string.Empty;
        _usuario.LinkRedeSocial = LinkRedeSocialEntry.Text?.Trim() ?? string.Empty;

        if (_usuario.Perfil == TipoPerfil.Atleta) {
            _usuario.DataNascimento = DataNascimentoPicker.Date;
            _usuario.Genero = Enum.TryParse<GeneroEnum>(GeneroPicker.SelectedItem?.ToString(), out var genero) ? genero : null;
            _usuario.Peso = double.TryParse(PesoEntry.Text, out var peso) ? peso : null;
            _usuario.Altura = double.TryParse(AlturaEntry.Text, out var altura) ? altura : null;
        }
        if (_usuario.Perfil == TipoPerfil.Organizador) {
            _usuario.NomeEmpresa = NomeEmpresaEntry.Text?.Trim() ?? string.Empty;
            _usuario.CNPJ = CnpjEntry.Text?.Trim() ?? string.Empty;
        }
        if (_usuario.Perfil == TipoPerfil.Patrocinador) {
            _usuario.FaixaOrcamentoPatrocinio = FaixaOrcamentoPatrocinioEntry.Text?.Trim() ?? string.Empty;
        }

        await _databaseService.AtualizarUsuarioAsync(_usuario);

        // Envia a mensagem com a instância atualizada do usuário
        Microsoft.Maui.Controls.MessagingCenter.Send(this, "PerfilAtualizado", _usuario);

        await _alertService.DisplayAlert("Sucesso", "Perfil atualizado com sucesso!", "OK");
        await Navigation.PopModalAsync();
    }
}