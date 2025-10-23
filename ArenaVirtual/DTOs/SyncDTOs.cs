using ArenaVirtual.Models;
using System.ComponentModel.DataAnnotations;
using System;

namespace ArenaVirtual.DTOs {
    public interface ISyncableDto {
        public int Id { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class JogoSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        public Guid TimeAClientAppId { get; set; }
        public Guid TimeBClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }
        public Guid? ArbitroClientAppId { get; set; } 

        public string? Local { get; set; }
        public DateTime DataHora { get; set; }
        public int PlacarA { get; set; }
        public int PlacarB { get; set; }
        public int Rodada { get; set; }
        public JogoStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class ConviteSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }
        public string? ConvidadoEmail { get; set; }
        public DateTime DataEnvio { get; set; }
        public Guid IdSolicitanteClientAppId { get; set; }
        public Guid TimeClientAppId { get; set; }
        public StatusConvite Status { get; set; }
        public DateTime UpdatedAt { get; set; } 
        public bool IsSynced { get; set; }
    }

    public class TimeSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? LogoUrl { get; set; }
        public Guid? CampeonatoClientAppId { get; set; }
        public string? Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? Regiao { get; set; }
        public int PontuacaoTotal { get; set; }
        public int Vitorias { get; set; }
        public int Derrotas { get; set; }
        public int Empates { get; set; }
        public Guid? CapitaoClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class UsuarioSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
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
        public Guid? TimeClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class CampeonatoSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Local { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public Guid OrganizadorClientAppId { get; set; }
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

    public class UsuarioCampeonatoFavoritoSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }
        public Guid UsuarioClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class SolicitacaoCampeonatoSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        public Guid TimeClientAppId { get; set; }

        public Guid CampeonatoClientAppId { get; set; }

        public Guid UsuarioClientAppId { get; set; }

        public DateTime DataSolicitacao { get; set; }

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

}