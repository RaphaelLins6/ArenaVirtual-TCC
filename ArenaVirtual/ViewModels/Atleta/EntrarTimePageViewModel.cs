using ArenaVirtual.Models;
using ArenaVirtual.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels.Atleta {

    public class TimeItemViewModel : BaseViewModel {
        public Time Time { get; set; }
        public string Nome => Time?.Nome;
        public string Descricao => Time?.Descricao;
        public string LogoUrl => Time?.LogoUrl;

        private string _buttonText;
        public string ButtonText {
            get => _buttonText;
            set => SetProperty(ref _buttonText, value);
        }

        private bool _isButtonEnabled;
        public bool IsButtonEnabled {
            get => _isButtonEnabled;
            set => SetProperty(ref _isButtonEnabled, value);
        }

        private Color _buttonColor;
        public Color ButtonColor {
            get => _buttonColor;
            set => SetProperty(ref _buttonColor, value);
        }

        public TimeItemViewModel(Time time, string buttonText, bool isEnabled, Color buttonColor) {
            Time = time;
            ButtonText = buttonText;
            IsButtonEnabled = isEnabled;
            ButtonColor = buttonColor;
        }
    }

    public class EntrarTimePageViewModel : BaseViewModel {

        private readonly TimeService _timeService;
        private readonly DatabaseService _databaseService;
        private readonly UsuarioService _usuarioService;

        public ObservableCollection<TimeItemViewModel> TimesDisponiveis { get; set; } = new ObservableCollection<TimeItemViewModel>();

        public ICommand PesquisarCommand { get; }
        public ICommand SolicitarEntradaCommand { get; }

        public EntrarTimePageViewModel(TimeService timeService, DatabaseService databaseService, UsuarioService usuarioService) {
            _timeService = timeService;
            _databaseService = databaseService;
            _usuarioService = usuarioService;

            PesquisarCommand = new Command<string>(async (query) => await PesquisarTimesAsync(query));

            SolicitarEntradaCommand = new Command<TimeItemViewModel>(async (timeItemVM) => await SolicitarEntradaAsync(timeItemVM));

            Task.Run(async () => await PesquisarTimesAsync(string.Empty));
        }

        private async Task PesquisarTimesAsync(string query) {
            IsBusy = true;
            try {
                var todosTimes = await _timeService.ObterTodosAsync();
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                var timesFiltrados = string.IsNullOrWhiteSpace(query)
                  ? todosTimes
                  : todosTimes.Where(t => t.Nome.ToLower().Contains(query.ToLower())).ToList();

                TimesDisponiveis.Clear();
                foreach (var time in timesFiltrados) {
                    var conviteExistente = await _databaseService.ObterConvitePorUsuarioETimeAsync(usuarioAtual.Id, time.Id);

                    string buttonText = "Solicitar Entrada";
                    bool isEnabled = true;
                    Color buttonColor = Color.FromArgb("#FF9800"); // Cor padrão

                    if (conviteExistente != null) {
                        if (conviteExistente.Status == StatusConvite.Pendente) {
                            buttonText = "Pendente";
                            isEnabled = false;
                            buttonColor = Color.FromArgb("#9E9E9E"); // Cor cinza para pendente
                        }
                    }

                    TimesDisponiveis.Add(new TimeItemViewModel(time, buttonText, isEnabled, buttonColor));
                }
            } finally {
                IsBusy = false;
            }
        }

        private async Task SolicitarEntradaAsync(TimeItemViewModel timeItemVM) {
            if (timeItemVM == null) return;
            IsBusy = true;
            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual == null) {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Usuário não logado.", "OK");
                    return;
                }

                var conviteExistente = await _databaseService.ObterConvitePorUsuarioETimeAsync(usuarioAtual.Id, timeItemVM.Time.Id);

                if (conviteExistente != null) {
                    if (conviteExistente.Status == StatusConvite.Pendente) {
                        await Application.Current.MainPage.DisplayAlert("Aviso", "Você já solicitou a entrada neste time. Aguarde a resposta do capitão.", "OK");
                        return;
                    }

                    if (conviteExistente.Status == StatusConvite.Recusado) {
                        conviteExistente.Status = StatusConvite.Pendente;
                        var resultadoAtualizacao = await _databaseService.AtualizarConviteAsync(conviteExistente);

                        if (resultadoAtualizacao > 0) {
                            timeItemVM.ButtonText = "Pendente";
                            timeItemVM.IsButtonEnabled = false;
                            timeItemVM.ButtonColor = Color.FromArgb("#9E9E9E");
                            await Application.Current.MainPage.DisplayAlert("Sucesso", $"Sua nova solicitação para entrar em '{timeItemVM.Time.Nome}' foi enviada com sucesso!", "OK");
                        } else {
                            await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível enviar a nova solicitação. Tente novamente.", "OK");
                        }
                        return;
                    }
                }

                var novoConvite = new Convite {
                    IdSolicitante = usuarioAtual.Id,
                    TimeId = timeItemVM.Time.Id,
                    DataEnvio = DateTime.UtcNow,
                    Status = StatusConvite.Pendente
                };

                var resultadoInsercao = await _databaseService.InserirConviteAsync(novoConvite);

                if (resultadoInsercao > 0) {
                    timeItemVM.ButtonText = "Pendente";
                    timeItemVM.IsButtonEnabled = false;
                    timeItemVM.ButtonColor = Color.FromArgb("#9E9E9E");
                    await Application.Current.MainPage.DisplayAlert("Sucesso", $"Sua solicitação para entrar em '{timeItemVM.Time.Nome}' foi enviada com sucesso!", "OK");
                } else {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível enviar a solicitação. Tente novamente.", "OK");
                }
            } finally {
                IsBusy = false;
            }
        }
    }
}