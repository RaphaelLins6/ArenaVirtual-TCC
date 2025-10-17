// EM ArenaVirtual.ViewModels.Patrocinio.SolicitacaoPatrocinioItemViewModel.cs

using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
// System.Globalization pode ser útil para formatação, mas não é estritamente necessário 
// para a estrutura da classe, então vamos removê-lo a menos que seja usado.

namespace ArenaVirtual.ViewModels.Patrocinio {

    // A classe precisa ser 'public partial'
    public partial class SolicitacaoPatrocinioItemViewModel : ObservableObject {

        // --- Campos de Referência ---
        // Referências aos modelos originais (tornadas públicas, como no seu ajuste inicial)
        public PropostaPatrocinio PropostaOriginal { get; }
        public Usuario Patrocinador { get; }

        // --- Propriedades de Binding no XAML ---

        // 1. Nome do Patrocinador (usa ObservableProperty para notificação, se necessário)
        [ObservableProperty]
        private string nomePatrocinador;

        // 2. Nome da Empresa (Não precisa ser Observable se só muda no construtor)
        public string NomeEmpresa { get; } // Bind: Text="{Binding NomeEmpresa}"

        // 3. Detalhes/Mensagem da Proposta (Usamos 'Mensagem' do Model, mas 'DetalhesProposta' no Binding)
        public string DetalhesProposta { get; } // Bind: Text="{Binding DetalhesProposta}"

        // 4. Valor Proposto (Usamos decimal para valores monetários)
        // Se a propriedade no Model for 'Valor', altere para o tipo correto (decimal ou double)
        public decimal ValorProposto { get; } // Bind: Text="{Binding ValorProposto, StringFormat='Valor: R$ {0}'}"
        public string FotoPatrocinadorUrl { get; }
        // --- Construtor ---
        public SolicitacaoPatrocinioItemViewModel(PropostaPatrocinio proposta, Usuario patrocinador) {

            // Atribuição de referências
            PropostaOriginal = proposta;
            Patrocinador = patrocinador;

            // Atribuição das propriedades de Binding
            NomePatrocinador = Patrocinador.Nome; // Patrocinador é um Usuario.
            NomeEmpresa = Patrocinador.NomeEmpresa; // Propriedade específica do Usuario Patrocinador.

            // Assumindo que PropostaPatrocinio tem as propriedades Mensagem e Valor
            DetalhesProposta = PropostaOriginal.Mensagem;
            FotoPatrocinadorUrl = Patrocinador.ImagemPath;
        }
    }
}