using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ArenaVirtual.Models 
{
    public class RodadaDeJogos : ObservableCollection<Jogo>, ISyncable {
        public string NomeRodada { get; set; } 

        public RodadaDeJogos(string nomeRodada, IEnumerable<Jogo> jogos) : base(jogos) {
            NomeRodada = nomeRodada;
        }
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public RodadaDeJogos() { }
    }
}