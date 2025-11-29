using ArenaVirtual.Models;
using System.ComponentModel.DataAnnotations;
using System; 

namespace ArenaVirtual.DTOs {

    public class JogoDownloadDto : ISyncableDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; } 

        public DateTime DataHora { get; set; }
        public string? Local { get; set; } 
        public int PlacarA { get; set; }
        public int PlacarB { get; set; }

        public int? TimeAId { get; set; }
        public int? TimeBId { get; set; }
        public int? CampeonatoId { get; set; }
        public int? ArbitroId { get; set; }

        public int Rodada { get; set; }
        public JogoStatus Status { get; set; }
    }

    public class CampeonatoDownloadDto : ISyncableDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }

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
        public string? Descricao { get; set; }
        public string? Modalidade { get; set; }
        public string? Regras { get; set; }
        public DateTime? DataTermino { get; set; }
        public int? NumeroEquipes { get; set; }
    }

    public class UsuarioDownloadDto : ISyncableDto {
        public int Id { get; set; } 
        public Guid ClientAppId { get; set; } 
        public DateTime UpdatedAt { get; set; } 
        public bool IsSynced { get; set; }

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
    }

    public class TimeDownloadDto : ISyncableDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }

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
    }

    public class ConviteDownloadDto : ISyncableDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }

        public string? ConvidadoEmail { get; set; }
        public DateTime DataEnvio { get; set; }
        public int IdSolicitanteId { get; set; }
        public int TimeId { get; set; }
        public StatusConvite Status { get; set; }
    }

    public class UsuarioCampeonatoFavoritoDownloadDto : ISyncableDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }

        public int UsuarioId { get; set; }
        public int CampeonatoId { get; set; }
    }

    public class RodadaDeJogosDownloadDto : ISyncableDto { 
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }
        public string NomeRodada { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class InscricaoDownloadDto : ISyncableDto { 
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        public int TimeId { get; set; } 
        public int CampeonatoId { get; set; } 
        public Guid TimeClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }

        public string? Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class EstatisticaPartidaDownloadDto : ISyncableDto { 
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public int JogoId { get; set; }
        public int TimeId { get; set; }
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

    public class AvaliacaoArbitroDownloadDto : ISyncableDto { 
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        public int ArbitroId { get; set; }
        public int JogoId { get; set; }
        public Guid ArbitroClientAppId { get; set; }
        public Guid JogoClientAppId { get; set; }

        public string Comentarios { get; set; } = string.Empty;
        public int Nota { get; set; }

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class CampanhaPatrocinioDownloadDto : ISyncableDto { 
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        public string? ImagemPatrocinador { get; set; }
        public string Nome { get; set; } = string.Empty;

        public int PatrocinadorId { get; set; }
        public int CampeonatoId { get; set; }
        public Guid PatrocinadorClientAppId { get; set; }
        public Guid CampeonatoClientAppId { get; set; }

        public decimal ValorProposta { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string Descricao { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
    }

    public class PropostaPatrocinioDownloadDto : ISyncableDto { 
        public Guid ClientAppId { get; set; }
        public int Id { get; set; }

        public int PatrocinadorId { get; set; }
        public int CampeonatoId { get; set; }
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

    public class PatrocinioDetalheDownloadDto : ISyncableDto { 
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