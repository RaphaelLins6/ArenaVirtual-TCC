// Exemplo de como a classe Inscricao deve ser (no arquivo Models/Inscricao.cs)
using SQLite;
using System;

namespace ArenaVirtual.Models {
    public class Inscricao : ISyncable {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Guid ClientAppId { get; set; } = Guid.NewGuid();
        public Guid TimeClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }
        public string? Status { get; set; } // "Pendente", "Aceita", "Recusada"
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Ignore]
        public Time? Time { get; set; } // Propriedade de navegação para carregar dados do time
    }
}