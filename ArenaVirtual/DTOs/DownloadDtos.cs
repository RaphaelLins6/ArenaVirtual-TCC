using ArenaVirtual.Models;
using System.ComponentModel.DataAnnotations;

namespace ArenaVirtual.DTOs {
    // DTOs de download, com o Id sendo do tipo int
    // pois é o que a API envia.

    public class CampeonatoDownloadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string? Nome { get; set; }
        public string? Local { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int OrganizadorId { get; set; } // O Id do organizador no servidor
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
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public TipoPerfil Perfil { get; set; }
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
        public int? TimeId { get; set; } // O Id do time no servidor
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class TimeDownloadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string? Nome { get; set; }
        public string? LogoUrl { get; set; }
        public int? CampeonatoId { get; set; } // O Id do campeonato no servidor
        public string? Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? Regiao { get; set; }
        public int PontuacaoTotal { get; set; }
        public int Vitorias { get; set; }
        public int Derrotas { get; set; }
        public int Empates { get; set; }
        public int? CapitaoId { get; set; } // O Id do capitão no servidor
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class ConviteDownloadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string? ConvidadoEmail { get; set; }
        public DateTime DataEnvio { get; set; }
        public int IdSolicitanteId { get; set; } // O Id do solicitante no servidor
        public int TimeId { get; set; } // O Id do time no servidor
        public StatusConvite Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class UsuarioCampeonatoFavoritoDownloadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public int UsuarioId { get; set; } // O Id do usuário no servidor
        public int CampeonatoId { get; set; } // O Id do campeonato no servidor
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }
}