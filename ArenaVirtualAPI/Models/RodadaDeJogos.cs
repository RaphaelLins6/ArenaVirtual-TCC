using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models // Corrigido namespace para API
{
    // Implementa ISyncable. A propriedade ClientAppId agora é Guid?.
    public class RodadaDeJogos : ISyncable {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Propriedade de sincronização (de ISyncable)
        public Guid ClientAppId { get; set; }

        public string NomeRodada { get; set; } = string.Empty;

        // Relação de 1 para N (RodadaDeJogos possui múltiplos Jogos).
        public virtual ICollection<Jogo> Jogos { get; set; } = new List<Jogo>();

        // Propriedades de sincronização (de ISyncable)
        public bool IsSynced { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Construtores
        public RodadaDeJogos(string nomeRodada) {
            NomeRodada = nomeRodada;
        }

        public RodadaDeJogos() { }
    }
}
