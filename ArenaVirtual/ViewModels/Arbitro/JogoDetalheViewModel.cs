using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArenaVirtual.Models.ViewModels.Shared {
    public partial class JogoDetalheViewModel : ObservableObject {

        public JogoDetalheViewModel() {
            Jogo = new Jogo();
            NomeTimeA = "Time A";
            NomeTimeB = "Time B";
            NomeCampeonato = "Campeonato";
            DataHora = DateTime.Now;
        }

        public Jogo Jogo { get; }

        public string NomeTimeA { get; set; }
        public string NomeTimeB { get; set; }
        public string NomeCampeonato { get; set; }
        public DateTime DataHora { get; set; }
        public bool PodeLancarEstatisticas => Jogo.Status != JogoStatus.Finalizado;
        public JogoDetalheViewModel(Jogo jogo, string nomeA, string nomeB, string campeonato) {
            Jogo = jogo ?? throw new ArgumentNullException(nameof(jogo)); 
            NomeTimeA = nomeA;
            NomeTimeB = nomeB;
            NomeCampeonato = campeonato;
            DataHora = jogo.DataHora;
        }
    }
}