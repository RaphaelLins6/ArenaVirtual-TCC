using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArenaVirtualAPI.Models;

public class Campeonato : ISyncable {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(200)]
    public string? Nome { get; set; }

    [MaxLength(200)]
    public string? Local { get; set; }

    [NotMapped]                  // propriedade só de UI (não vai ao banco)
    public bool EhFavorito { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    public int OrganizadorId { get; set; }  // FK para Usuario (se quiser mapear depois)

    public string? LogoUrl { get; set; }
    public string? NomeOrganizador { get; set; }
    public string? EmailOrganizador { get; set; }
    public string? TelefoneOrganizador { get; set; }

    public int NumeroMaximoEquipes { get; set; }
    public decimal ValorTaxaInscricao { get; set; }
    public string? FormatoCampeonato { get; set; }
    public string? LocaisDosJogos { get; set; }
    public bool HaveraPremiacao { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Valor padrão
    public bool IsSynced { get; set; } = false; // Valor padrão

}
