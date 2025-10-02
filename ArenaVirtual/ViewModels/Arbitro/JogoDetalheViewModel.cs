using CommunityToolkit.Mvvm.ComponentModel;

namespace ArenaVirtual.Models.ViewModels.Shared { 
    public partial class JogoDetalheViewModel : ObservableObject {

        // Objeto Jogo original
        public Jogo Jogo { get; }

        // Propriedades para exibição na UI
        public string NomeTimeA { get; set; }
        public string NomeTimeB { get; set; }
        public string NomeCampeonato { get; set; }
        public bool PodeLancarEstatisticas => Jogo.Status != JogoStatus.Finalizado; 
        public JogoDetalheViewModel(Jogo jogo, string nomeA, string nomeB, string campeonato) {
            Jogo = jogo;
            NomeTimeA = nomeA;
            NomeTimeB = nomeB;
            NomeCampeonato = campeonato;
        }
    }
}