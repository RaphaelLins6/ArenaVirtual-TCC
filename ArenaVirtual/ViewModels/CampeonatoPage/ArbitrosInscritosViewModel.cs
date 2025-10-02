using ArenaVirtual.Models;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels.CampeonatoPage {

    public class ArbitrosInscritosViewModel : ObservableObject {

        private readonly DatabaseService _databaseService;
        private readonly UsuarioService _usuarioService;

        private Guid _campeonatoId;
        public Guid CampeonatoId {
            get => _campeonatoId;
            set => SetProperty(ref _campeonatoId, value);
        }

        private ObservableCollection<Usuario> _arbitrosInscritos = new();
        public ObservableCollection<Usuario> ArbitrosInscritos {
            get => _arbitrosInscritos;
            set => SetProperty(ref _arbitrosInscritos, value);
        }

        private bool _isBusy;
        public bool IsBusy {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand LoadArbitrosCommand { get; }

        public ArbitrosInscritosViewModel(DatabaseService databaseService, UsuarioService usuarioService) {
            _databaseService = databaseService;
            _usuarioService = usuarioService;

            LoadArbitrosCommand = new Command(async () => await LoadArbitrosAsync());

            MessagingCenter.Subscribe<GerenciarSolicitacoesViewModel, Usuario>(
                this, "ArbitroAceito", (sender, arbitro) => {
                    MainThread.BeginInvokeOnMainThread(() => {
                        if (!ArbitrosInscritos.Any(a => a.ClientAppId == arbitro.ClientAppId)) {
                            ArbitrosInscritos.Add(arbitro);
                        }
                    });
                });
        }

        public async Task LoadArbitrosAsync() {
            if (CampeonatoId == Guid.Empty || IsBusy) return;

            IsBusy = true;
            try {
                var convitesAceitos = await _databaseService
                    .ObterConvitesAceitosPorCampeonatoAsync(CampeonatoId);

                var tarefas = convitesAceitos
                    .Select(c => _usuarioService.ObterUsuarioPorClientAppIdAsync(c.UsuarioClientAppId))
                    .ToList();

                var arbitros = await Task.WhenAll(tarefas);

                MainThread.BeginInvokeOnMainThread(() => {
                    ArbitrosInscritos.Clear();
                    foreach (var a in arbitros.Where(x => x != null)) {
                        ArbitrosInscritos.Add(a);
                    }
                });

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar árbitros: {ex.Message}");
            } finally {
                IsBusy = false;
            }
        }

        ~ArbitrosInscritosViewModel() {
            MessagingCenter.Unsubscribe<GerenciarSolicitacoesViewModel, Usuario>(this, "ArbitroAceito");
        }
    }
}
