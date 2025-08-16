using ArenaVirtual.Services;
using ArenaVirtual.Views.Atleta;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ArenaVirtual.Models;

namespace ArenaVirtual.ViewModels.Atleta;

public partial class MeuTimePageViewModel : INotifyPropertyChanged {
    public class MembroModel {
        public string Nome { get; set; }
        public ImageSource Foto { get; set; }
    }

    private Time _time = new Time();
    public Time Time {
        get => _time;
        set {
            if (_time != value) {
                _time = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VinculadoATime));
                OnPropertyChanged(nameof(NaoVinculadoATime));
                OnPropertyChanged(nameof(LogoImageSource));
                OnPropertyChanged(nameof(UsuarioEhCapitao));
            }
        }
    }

    public ImageSource LogoImageSource => GetImageSourceFromFile(Time?.LogoUrl);

    private ObservableCollection<MembroModel> _membrosDoTime = new ObservableCollection<MembroModel>();
    public ObservableCollection<MembroModel> MembrosDoTime {
        get => _membrosDoTime;
        set {
            if (_membrosDoTime != value) {
                _membrosDoTime = value;
                OnPropertyChanged();
            }
        }
    }

    public bool VinculadoATime => SessaoService.Instancia.GetUsuarioAtual()?.TimeId != null;
    public bool NaoVinculadoATime => !VinculadoATime;

    public bool UsuarioEhCapitao =>
        SessaoService.Instancia.GetUsuarioAtual()?.Id == Time?.CapitaoId;

    public ICommand CriarMeuTimeCommand { get; }
    public ICommand EntrarTimeCommand { get; }
    public ICommand GerenciarTimeCommand { get; }
    public ICommand VerSolicitacoesCommand { get; }

    private readonly TimeService _timeService;
    private readonly UsuarioService _usuarioService;
    private readonly DatabaseService _databaseService;

    public MeuTimePageViewModel(TimeService timeService, UsuarioService usuarioService, DatabaseService databaseService) {
        _timeService = timeService;
        _usuarioService = usuarioService;
        _databaseService = databaseService;

        GerenciarTimeCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(EditarTimePage)));
        CriarMeuTimeCommand = new Command(async () => await Shell.Current.Navigation.PushAsync(new CriarTimePage(_timeService)));
        EntrarTimeCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(EntrarTimePage)));
        VerSolicitacoesCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(SolicitacaoTimePage)));

        Time = new Time();
        _ = LoadData();
    }

    private string _statusMessage = "Você ainda não está em um time!";
    public string StatusMessage {
        get => _statusMessage;
        set {
            if (_statusMessage != value) {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _showButtons = true;
    public bool ShowButtons {
        get => _showButtons;
        set {
            if (_showButtons != value) {
                _showButtons = value;
                OnPropertyChanged();
            }
        }
    }

    public async Task LoadData() {
        try {
            var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();

            if (usuarioAtual?.TimeId == null) {
                Time = new Time();
                MembrosDoTime.Clear();

                var convitePendente = await _databaseService.ObterConvitePendenteDoUsuarioAsync(usuarioAtual.Id);
                if (convitePendente != null) {
                    var timeConvidado = await _timeService.ObterPorIdAsync(convitePendente.IdTime);
                    if (timeConvidado != null) {
                        StatusMessage = $"Solicitação enviada para o time: {timeConvidado.Nome}. Aguarde a resposta do capitão.";
                    } else {
                        StatusMessage = "Solicitação enviada para um time. Aguarde a resposta do capitão.";
                    }

                    ShowButtons = false;
                } else {
                    StatusMessage = "Você ainda não está em um time!";
                    ShowButtons = true;
                }

                return;
            }

            var timeDoUsuario = await _timeService.ObterPorIdAsync(usuarioAtual.TimeId.Value);

            if (timeDoUsuario == null) {
                Time = new Time();
                MembrosDoTime.Clear();
                StatusMessage = "Você ainda não está em um time!";
                ShowButtons = true;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Caminho da Logo do Time lido do DB: '{timeDoUsuario.LogoUrl}'");

            Time = timeDoUsuario;

            var membrosCarregados = new ObservableCollection<MembroModel>();
            var usuariosDoTime = await _usuarioService.ListarMembrosDoTimeAsync(timeDoUsuario.Id);

            if (usuariosDoTime != null) {
                foreach (var usuario in usuariosDoTime) {
                    membrosCarregados.Add(new MembroModel {
                        Nome = usuario.Nome,
                        Foto = GetImageSourceFromFile(usuario.ImagemPath)
                    });
                }
            }
            MembrosDoTime = membrosCarregados;
            StatusMessage = ""; 
            ShowButtons = false;
        } catch (Exception ex) {
            Time = new Time();
            MembrosDoTime.Clear();
            await Shell.Current.DisplayAlert("Erro", "Não foi possível carregar os dados do time.", "OK");
            System.Diagnostics.Debug.WriteLine($"[ERRO GERAL] Falha ao carregar dados do time: {ex.Message}");
        }
    }

    private ImageSource GetImageSourceFromFile(string filePath) {
        if (string.IsNullOrEmpty(filePath)) {
            System.Diagnostics.Debug.WriteLine("[DEBUG] filePath para ImageSource está vazio ou nulo. Usando placeholder.");
            return ImageSource.FromFile("placeholder.png");
        }

        try {
            if (File.Exists(filePath)) {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Tentando carregar imagem do caminho: '{filePath}'");
                return ImageSource.FromStream(() => File.OpenRead(filePath));
            } else {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Arquivo não encontrado no caminho: '{filePath}'. Usando placeholder.");
                return ImageSource.FromFile("placeholder.png");
            }
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"[ERRO DE CARREGAMENTO] Falha ao carregar imagem do caminho: '{filePath}'. Erro: {ex.Message}");
            return ImageSource.FromFile("placeholder.png");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

}
