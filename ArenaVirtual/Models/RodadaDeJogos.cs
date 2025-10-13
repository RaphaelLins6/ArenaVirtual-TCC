using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ArenaVirtual.Models 
{
    public class RodadaDeJogos : ObservableCollection<Jogo> {
        public string NomeRodada { get; set; } 

        public RodadaDeJogos(string nomeRodada, IEnumerable<Jogo> jogos) : base(jogos) {
            NomeRodada = nomeRodada;
        }

        public RodadaDeJogos() { }
    }
}