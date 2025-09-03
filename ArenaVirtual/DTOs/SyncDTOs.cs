using System;
using ArenaVirtual.Models;

namespace ArenaVirtual.DTOs {
    public interface ISyncableDto {
        public int Id { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // DTO para sincronizar dados de campeonatos no aplicativo.
    public class CampeonatoSyncDto : ISyncableDto {
        public int Id { get; set; }
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

    // DTO para sincronizar convites, como convites para equipes.
    public class ConviteSyncDto : ISyncableDto {
        public int Id { get; set; }
        public int IdSolicitante { get; set; }
        public int TimeId { get; set; }
        public DateTime DataEnvio { get; set; }
        public StatusConvite StatusConvite { get; set; }
        public string? ConvidadoEmail { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    // DTO para sincronizar dados de equipes.
    public class TimeSyncDto : ISyncableDto {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
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

    // DTO para sincronizar dados de usuários.
    public class UsuarioSyncDto : ISyncableDto {
        public int Id { get; set; }
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
        public int? TimeId { get; set; }
        public string? NovaSenha { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class UsuarioCampeonatoFavoritoSyncDto : ISyncableDto {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CampeonatoId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }
}
