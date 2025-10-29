using ArenaVirtual.Models;
using System.ComponentModel.DataAnnotations;
using System;

namespace ArenaVirtual.DTOs {
    public interface ISyncableDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class JogoSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        // As FKs devem ser Guid ClientAppIds
        public Guid TimeAClientAppId { get; set; }
        public Guid TimeBClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }
        public Guid? ArbitroClientAppId { get; set; } // O foco da correção

        public string? Local { get; set; }
        public DateTime DataHora { get; set; }
        public int PlacarA { get; set; }
        public int PlacarB { get; set; }

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class ConviteSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }
        public string? ConvidadoEmail { get; set; }
        public DateTime DataEnvio { get; set; }
        public Guid? IdSolicitanteClientAppId { get; set; }
        public Guid? TimeClientAppId { get; set; }
        public StatusConvite Status { get; set; }
        public DateTime UpdatedAt { set; get; }
        public bool IsSynced { set; get; }
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
        public string Nome { get; set; }
        public string Email { get; set; }
        public TipoPerfil Perfil { get; set; }
        public string ImagemPath { get; set; }
        public string Localizacao { get; set; }
        public string Telefone { get; set; }
        public string LinkRedeSocial { get; set; }
        public DateTime? DataNascimento { get; set; }
        public GeneroEnum? Genero { get; set; }
        public string NomeEmpresa { get; set; }
        public string CNPJ { get; set; }
        public double? Peso { get; set; }
        public double? Altura { get; set; }
        public string FaixaOrcamentoPatrocinio { get; set; }
        public Guid? TimeClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class CampeonatoSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Local { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public Guid OrganizadorClientAppId { get; set; }
        public string LogoUrl { get; set; }
        public string NomeOrganizador { get; set; }
        public string EmailOrganizador { get; set; }
        public string TelefoneOrganizador { get; set; }
        public int NumeroMaximoEquipes { get; set; }
        public decimal ValorTaxaInscricao { get; set; }
        public string? FormatoCampeonato { get; set; }
        public string? LocaisDosJogos { get; set; }
        public bool HaveraPremiacao { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DataTermino { get; set; }
        public int NumeroEquipes { get; set; }
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

    public class RodadaDeJogosSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }
        public string NomeRodada { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class InscricaoSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        // Chaves estrangeiras usando Guid ClientAppIds
        public Guid TimeClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }

        public string? Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class EstatisticaPartidaSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        // Chaves estrangeiras usando ClientAppId do Usuário, Jogo e Time
        public Guid UsuarioClientAppId { get; set; }
        public Guid JogoClientAppId { get; set; }
        public Guid TimeClientAppId { get; set; }

        public int Pontos { get; set; }
        public int Rebotes { get; set; }
        public int Assistencias { get; set; }
        public int Roubos { get; set; }
        public int Bloqueios { get; set; }
        public int Faltas { get; set; }
        public int Turnovers { get; set; }
        public int Arremessos2PontosConvertidos { get; set; }
        public int Arremessos2PontosTentados { get; set; }
        public int Arremessos3PontosConvertidos { get; set; }
        public int Arremessos3PontosTentados { get; set; }
        public int LancesLivresConvertidos { get; set; }
        public int LancesLivresTentados { get; set; }

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class AvaliacaoArbitroSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        // Chaves estrangeiras usando ClientAppId do Arbitro (Usuário) e Jogo
        public Guid ArbitroClientAppId { get; set; }
        public Guid JogoClientAppId { get; set; }

        public string Comentarios { get; set; } = string.Empty;
        public int Nota { get; set; }

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class CampanhaPatrocinioSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        public string? ImagemPatrocinador { get; set; }
        public string Nome { get; set; } = string.Empty;

        // Chaves estrangeiras usando ClientAppId do Patrocinador (Usuário) e Campeonato
        public Guid PatrocinadorClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }

        public decimal ValorProposta { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string Descricao { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class PropostaPatrocinioSyncDto : ISyncableDto {
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        // Chaves estrangeiras usando ClientAppId do Patrocinador (Usuário) e Campeonato
        public Guid PatrocinadorClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }

        public string NomePatrocinador { get; set; } = string.Empty;
        public string ImagemPatrocinador { get; set; } = string.Empty;
        public string LinkPatrocinador { get; set; } = string.Empty;
        public decimal ValorMonetario { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public bool Aprovada { get; set; }

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class PatrocinioDetalheSyncDto : ISyncableDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public decimal ValorMonetario { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public bool Aprovada { get; set; }
        public Guid PatrocinadorClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }
        public string NomePatrocinador { get; set; } = string.Empty;
        public string ImagemPatrocinador { get; set; } = string.Empty;
        public Guid? CampanhaClientAppId { get; set; }
        public string? CampanhaNome { get; set; }
        public decimal? CampanhaValorProposta { get; set; }
        public DateTime? CampanhaInicio { get; set; }
        public DateTime? CampanhaFim { get; set; }
        public string? CampanhaDescricao { get; set; }
    }

}