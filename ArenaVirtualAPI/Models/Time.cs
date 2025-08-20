using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models;

public class Time : ISyncable {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? LogoUrl { get; set; }   // guarde aqui uma URL acessível (ex.: Azure Blob)

    public int CampeonatoId { get; set; }

    [MaxLength(500)]
    public string? Descricao { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? Regiao { get; set; }

    public int PontuacaoTotal { get; set; } = 0;
    public int Vitorias { get; set; } = 0;
    public int Derrotas { get; set; } = 0;
    public int Empates { get; set; } = 0;

    public int? CapitaoId { get; set; }

    public ICollection<Usuario>? Membros { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Valor padrão
    public bool IsSynced { get; set; } = false; // Valor padrão

}
