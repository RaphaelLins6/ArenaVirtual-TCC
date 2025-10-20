using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System; // Import necessário para DateTime
using System.Globalization; // Import útil para formatação de moeda/data

namespace ArenaVirtual.ViewModels.Patrocinio {

    // A classe precisa ser 'public partial'
    public partial class SolicitacaoPatrocinioItemViewModel : ObservableObject {

        // --- Campos de Referência ---
        public PropostaPatrocinio PropostaOriginal { get; }
        public Usuario Patrocinador { get; }

        // --- Propriedades de Binding no XAML ---

        // 1. Nome do Patrocinador
        [ObservableProperty]
        private string nomePatrocinador;

        // 2. Nome da Empresa
        public string NomeEmpresa { get; }

        // 3. Detalhes/Mensagem da Proposta (Este campo CONTÉM a mensagem COMPLETA com valor e período)
        public string DetalhesProposta { get; }

        // 4. Valor Proposto (Agora atribui o valor do Model)
        public decimal ValorProposto { get; } // Bind: Text="{Binding ValorProposto, StringFormat='PROPOSTA: R$ {0}'}"

        public string FotoPatrocinadorUrl { get; }
        public string ValorPropostoFormatado { get; }
        public decimal ValorPropostoMonetario { get; }
        public DateTime DataInicio { get; } // Propriedade do Model
        public DateTime DataFim { get; }    // Propriedade do Model

        // Propriedade formatada para facilitar o Binding no XAML (se o Model não tiver o período completo)
        public string PeriodoPropostoFormatado { get; }

        // --- Construtor ---
        public SolicitacaoPatrocinioItemViewModel(PropostaPatrocinio proposta, Usuario patrocinador) {

            // Atribuição de referências
            PropostaOriginal = proposta;
            Patrocinador = patrocinador;

            // Atribuição das propriedades de Binding
            NomePatrocinador = Patrocinador.Nome;
            NomeEmpresa = Patrocinador.NomeEmpresa;

            // ⭐️ ATRIBUIÇÃO DO VALOR ⭐️
            // Assumindo que o Model PropostaPatrocinio tem a propriedade ValorMonetario
            ValorPropostoMonetario = PropostaOriginal.ValorMonetario;

            // Formatação para exibição: "R$ 5.000,00"
            ValorPropostoFormatado = $"VALOR PROPOSTO: {ValorPropostoMonetario.ToString("C", new CultureInfo("pt-BR"))}";

            // Mensagem: É apenas o texto livre (sem valor e sem período)
            DetalhesProposta = PropostaOriginal.Mensagem;

            // ATRIBUIÇÃO DE DATAS
            DataFim = proposta.DataFim;
            DataInicio = proposta.DataInicio;

            // Formatação do período
            PeriodoPropostoFormatado = $"PERÍODO: {DataInicio:dd/MM/yyyy} a {DataFim:dd/MM/yyyy}";

            FotoPatrocinadorUrl = Patrocinador.ImagemPath;
        }
    }
}