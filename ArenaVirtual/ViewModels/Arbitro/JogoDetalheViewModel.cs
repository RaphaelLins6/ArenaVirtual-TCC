using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArenaVirtual.Models.ViewModels.Shared {
    public partial class JogoDetalheViewModel : ObservableObject {

        // ***** NOVO CONSTRUTOR PADRÃO (Sem argumentos) *****
        // Resolva o erro de compilação quando o XAML ou o designer tenta instanciar a classe.
        public JogoDetalheViewModel() {
            // Inicialize com valores seguros para evitar NullReferenceException no designer/XAML
            Jogo = new Jogo();
            NomeTimeA = "Time A";
            NomeTimeB = "Time B";
            NomeCampeonato = "Campeonato";
            DataHora = DateTime.Now;
        }
        // ***************************************************

        // Objeto Jogo original
        public Jogo Jogo { get; }

        // Propriedades para exibição na UI
        public string NomeTimeA { get; set; }
        public string NomeTimeB { get; set; }
        public string NomeCampeonato { get; set; }
        public DateTime DataHora { get; set; }
        public bool PodeLancarEstatisticas => Jogo.Status != JogoStatus.Finalizado;
        // Construtor usado pela DashboardArbitroViewModel
        public JogoDetalheViewModel(Jogo jogo, string nomeA, string nomeB, string campeonato) {
            Jogo = jogo ?? throw new ArgumentNullException(nameof(jogo)); // Adicionei validação
            NomeTimeA = nomeA;
            NomeTimeB = nomeB;
            NomeCampeonato = campeonato;
            DataHora = jogo.DataHora;
        }
    }
}