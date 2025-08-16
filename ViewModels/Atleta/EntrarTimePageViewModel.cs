using ArenaVirtual.Models;
using ArenaVirtual.Services;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels.Atleta {
    public class EntrarTimePageViewModel : BaseViewModel {

        private readonly TimeService _timeService;
        private readonly DatabaseService _databaseService;
        private readonly UsuarioService _usuarioService;

        private ObservableCollection<Time> _timesDisponiveis = [];
        public ObservableCollection<Time> TimesDisponiveis {
            get => _timesDisponiveis;
            set => SetProperty(ref _timesDisponiveis, value);
        }

        public ICommand PesquisarCommand { get; }
        public ICommand SolicitarEntradaCommand { get; }

        public EntrarTimePageViewModel(TimeService timeService, DatabaseService databaseService, UsuarioService usuarioService) {
            _timeService = timeService;
            _databaseService = databaseService;
            _usuarioService = usuarioService;

            PesquisarCommand = new Command<string>(async (query) => await PesquisarTimesAsync(query));
            SolicitarEntradaCommand = new Command<Time>(async (time) => await SolicitarEntradaAsync(time));

            Task.Run(async () => await PesquisarTimesAsync(string.Empty));
        }

        private async Task PesquisarTimesAsync(string query) {
            IsBusy = true;
            try {
                // Obter todos os times
                var todosTimes = await _timeService.ObterTodosAsync();

                // Filtrar os times com base na query
                var timesFiltrados = string.IsNullOrWhiteSpace(query)
                    ? todosTimes
                    : todosTimes.Where(t => t.Nome.ToLower().Contains(query.ToLower())).ToList();

                // Atualizar a ObservableCollection
                TimesDisponiveis.Clear();
                foreach (var time in timesFiltrados) {
                    // Aqui você pode adicionar uma lógica para não mostrar o próprio time, etc.
                    TimesDisponiveis.Add(time);
                }
            } finally {
                IsBusy = false;
            }
        }

        private async Task SolicitarEntradaAsync(Time time) {
            if (time == null) return;
            IsBusy = true;
            try {
                var usuarioAtual = SessaoService.Instancia.GetUsuarioAtual();
                if (usuarioAtual == null) {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Usuário não logado.", "OK");
                    return;
                }

                // Verifique se o usuário já tem um convite pendente para este time
                var conviteExistente = await _databaseService.ObterConvitePorUsuarioETimeAsync(usuarioAtual.Id, time.Id);
                if (conviteExistente != null) {
                    await Application.Current.MainPage.DisplayAlert("Aviso", "Você já solicitou a entrada neste time. Aguarde a resposta do capitão.", "OK");
                    return;
                }

                // Crie e salve o novo convite no banco de dados
                var novoConvite = new Convite {
                    IdSolicitante = usuarioAtual.Id,
                    IdTime = time.Id,
                    DataEnvio = DateTime.UtcNow,
                    Status = StatusConvite.Pendente
                };

                var resultado = await _databaseService.InserirConviteAsync(novoConvite);

                if (resultado > 0) {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", $"Sua solicitação para entrar em '{time.Nome}' foi enviada com sucesso!", "OK");
                } else {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível enviar a solicitação. Tente novamente.", "OK");
                }
            } finally {
                IsBusy = false;
            }
        }
    }
}