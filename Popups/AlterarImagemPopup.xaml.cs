using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System.Diagnostics;

namespace ArenaVirtual.Popups;

public partial class AlterarImagemPopup : ContentPage 
{
    private readonly Usuario _usuario;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private byte[] _imagemBytes = [];

    public event EventHandler<string>? ImagemAtualizada;

    public AlterarImagemPopup(Usuario usuario, IAlertService alertService)
    {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;
        _databaseService = App.Current?.Handler?.MauiContext?.Services?.GetRequiredService<DatabaseService>()
                             ?? throw new InvalidOperationException("DatabaseService not registered or app context is null.");

        if (!string.IsNullOrEmpty(_usuario.ImagemPath))
            ImagemPerfil.Source = _usuario.ImagemPath;
    }

    private async void EscolherImagem_Clicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Escolha uma imagem"
            });

            if (photo != null)
            {
                using var stream = await photo.OpenReadAsync();
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                _imagemBytes = memoryStream.ToArray();

                ImagemPerfil.Source = ImageSource.FromStream(() => new MemoryStream(_imagemBytes));
            }
        }
        catch (Exception ex)
        {
            await _alertService.DisplayAlert("Erro", "Erro ao escolher imagem: " + ex.Message, "OK");
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Corrigido para ContentPage
    }

    private async void Salvar_Clicked(object sender, EventArgs e)
    {
        string? novoCaminho = null;
        if (_imagemBytes.Length > 0)
        {
            var fileName = $"perfil_{_usuario.Id}_{DateTime.Now.Ticks}.jpg";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            File.WriteAllBytes(filePath, _imagemBytes);
            novoCaminho = filePath;
            _usuario.ImagemPath = novoCaminho;
        }

        // Recarregue o usuário do banco para garantir rastreamento correto
        var usuarioDb = (await _databaseService.ListarUsuariosAsync())
            .FirstOrDefault(u => u.Id == _usuario.Id);

        if (usuarioDb != null)
        {
            usuarioDb.ImagemPath = _usuario.ImagemPath;

            Debug.WriteLine($"[AlterarImagemPopup] ID do usuário no banco: {usuarioDb.Id}");
            Debug.WriteLine($"[AlterarImagemPopup] Novo caminho da foto de perfil: {usuarioDb.ImagemPath}");

            int rowsAffected = await _databaseService.AtualizarUsuarioAsync(usuarioDb);

            Debug.WriteLine($"[AlterarImagemPopup] Resultado de AtualizarUsuarioAsync: {rowsAffected} linhas afetadas.");

            if (rowsAffected > 0)
            {
                if (App.CurrentUser != null && App.CurrentUser.Id == usuarioDb.Id)
                {
                    App.CurrentUser.ImagemPath = usuarioDb.ImagemPath;
                    Debug.WriteLine("[AlterarImagemPopup] App.CurrentUser.ImagemPath atualizado na sessão.");
                }

                await _alertService.DisplayAlert("Sucesso", "Imagem atualizada com sucesso!", "OK");
                ImagemAtualizada?.Invoke(this, usuarioDb.ImagemPath);
                await Navigation.PopModalAsync();
            }
            else
            {
                await _alertService.DisplayAlert("Erro", "Falha ao atualizar a imagem no banco de dados.", "OK");
                Debug.WriteLine("[AlterarImagemPopup] Erro: Nenhuma linha afetada por AtualizarUsuarioAsync.");
            }
        }
        else
        {
            await _alertService.DisplayAlert("Erro", "Usuário não encontrado no banco de dados.", "OK");
            Debug.WriteLine("[AlterarImagemPopup] Erro: Usuário não encontrado no banco.");
        }
    }
}