using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System; 
using System.Globalization;

namespace ArenaVirtual.ViewModels.Patrocinio {

    public partial class SolicitacaoPatrocinioItemViewModel : ObservableObject {

        public PropostaPatrocinio PropostaOriginal { get; }
        public Usuario Patrocinador { get; }

        [ObservableProperty]
        private string nomePatrocinador;

        public string NomeEmpresa { get; }

        public string DetalhesProposta { get; }

        public decimal ValorProposto { get; } 

        public string FotoPatrocinadorUrl { get; }
        public string ValorPropostoFormatado { get; }
        public decimal ValorPropostoMonetario { get; }
        public DateTime DataInicio { get; } 
        public DateTime DataFim { get; }

        public string PeriodoPropostoFormatado { get; }

        // --- Construtor ---
        public SolicitacaoPatrocinioItemViewModel(PropostaPatrocinio proposta, Usuario patrocinador) {

            PropostaOriginal = proposta;
            Patrocinador = patrocinador;

            NomePatrocinador = Patrocinador.Nome;
            NomeEmpresa = Patrocinador.NomeEmpresa;

            ValorPropostoMonetario = PropostaOriginal.ValorMonetario;

            ValorPropostoFormatado = $"VALOR PROPOSTO: {ValorPropostoMonetario.ToString("C", new CultureInfo("pt-BR"))}";

            DetalhesProposta = PropostaOriginal.Mensagem;

            DataFim = proposta.DataFim;
            DataInicio = proposta.DataInicio;

            PeriodoPropostoFormatado = $"PERÍODO: {DataInicio:dd/MM/yyyy} a {DataFim:dd/MM/yyyy}";

            FotoPatrocinadorUrl = Patrocinador.ImagemPath;
        }
    }
}