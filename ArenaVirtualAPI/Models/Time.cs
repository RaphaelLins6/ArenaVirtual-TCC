using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ArenaVirtualAPI.Models;

public class Time : ISyncable {

    [Key]
    public int Id { get; set; }

    [Required]
    public Guid ClientAppId { get; set; }

    public int CampeonatoId { get; set; }

    [Required, MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? LogoUrl { get; set; }

    [MaxLength(500)]
    public string Descricao { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string Regiao { get; set; }

    public int Vitorias { get; set; } = 0;
    public int Derrotas { get; set; } = 0;
    public int Empates { get; set; } = 0;

    public int CapitaoId { get; set; }

    public Guid CapitaoClientAppId { get; set; }

    [ForeignKey("CapitaoId")]
    public Usuario? Capitao { get; set; }

    [ForeignKey("CampeonatoId")]
    public Campeonato? Campeonato { get; set; }

    [JsonIgnore]
    public ICollection<Usuario> Membros { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;
}