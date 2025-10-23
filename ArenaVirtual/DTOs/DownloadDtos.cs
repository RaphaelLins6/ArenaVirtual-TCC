using ArenaVirtual.Models;
using System.ComponentModel.DataAnnotations;

namespace ArenaVirtual.DTOs {

    public class JogoDownloadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DataHora { get; set; }
        public string Local { get; set; }
        public int PlacarA { get; set; }
        public int PlacarB { get; set; }
        public int? TimeAId { get; set; }
        public int? TimeBId { get; set; }
        public int? CampeonatoId { get; set; }
        public int? ArbitroId { get; set; }
        public int Rodada { get; set; }
        public JogoStatus Status { get; set; }
    }
    public class CampeonatoDownloadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string? Nome { get; set; }
        public string? Local { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int OrganizadorId { get; set; } 
        public string? LogoUrl { get; set; }
        public string? NomeOrganizador { get; set; }
        public string? EmailOrganizador { get; set; }
        public string? TelefoneOrganizador { get; set; }
        public int NumeroMaximoEquipes { get; set; }
        public decimal ValorTaxaInscricao { get; set; }
        public string? FormatoCampeonato { get; set; }
        public string? LocaisDosJogos { get; set; }
        public bool HaveraPremiacao { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Descricao { get; set; }
        public string? Modalidade { get; set; }
        public string? Regras { get; set; }
        public DateTime? DataTermino { get; set; }
        public int? NumeroEquipes { get; set; }
        public bool IsSynced { get; set; }
    }

    public class UsuarioDownloadDto {
        public int? Id { get; set; }
        public Guid? ClientAppId { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public TipoPerfil? Perfil { get; set; }
        public string? ImagemPath { get; set; }
        public string? Localizacao { get; set; }
        public string? Telefone { get; set; }
        public string? LinkRedeSocial { get; set; }
        public DateTime? DataNascimento { get; set; }
        public GeneroEnum? Genero { get; set; }
        public string? NomeEmpresa { get; set; }
        public string? CNPJ { get; set; }
        public double? Peso { get; set; }
        public double? Altura { get; set; }
        public string? FaixaOrcamentoPatrocinio { get; set; }
        public int? TimeId { get; set; } 
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class TimeDownloadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string? Nome { get; set; }
        public string? LogoUrl { get; set; }
        public int? CampeonatoId { get; set; } 
        public string? Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? Regiao { get; set; }
        public int PontuacaoTotal { get; set; }
        public int Vitorias { get; set; }
        public int Derrotas { get; set; }
        public int Empates { get; set; }
        public int? CapitaoId { get; set; } 
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class ConviteDownloadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string? ConvidadoEmail { get; set; }
        public DateTime DataEnvio { get; set; }
        public int IdSolicitanteId { get; set; } 
        public int TimeId { get; set; } 
        public StatusConvite Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class UsuarioCampeonatoFavoritoDownloadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public int UsuarioId { get; set; } 
        public int CampeonatoId { get; set; } 
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }
}