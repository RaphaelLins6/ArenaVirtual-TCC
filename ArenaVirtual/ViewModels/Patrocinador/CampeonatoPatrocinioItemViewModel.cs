using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics; // Importação necessária para Color
using System;

namespace ArenaVirtual.ViewModels.Patrocinador {

    // Novo Enum para representar o estado do patrocínio/proposta
    public enum PatrocinioStatus {
        Disponivel, // Nenhuma ação tomada
        Pendente,   // Proposta enviada, aguardando aprovação
        Aceito      // Patrocínio ativo
    }

    public partial class CampeonatoPatrocinioItemViewModel : ObservableObject {
        public Campeonato Campeonato { get; set; }
        public string Nome => Campeonato?.Nome;
        public string Descricao => Campeonato?.Descricao;
        public string LogoUrl => Campeonato?.LogoUrl;

        // NOVO: Status que define a aparência do botão
        [ObservableProperty]
        private PatrocinioStatus statusAtual = PatrocinioStatus.Disponivel;

        [ObservableProperty]
        private string buttonText = "Propor Patrocínio";

        [ObservableProperty]
        private bool isButtonEnabled = true;

        // Cor base: Laranja principal
        [ObservableProperty]
        private Color buttonColor = Color.FromArgb("#FFA500");

        public CampeonatoPatrocinioItemViewModel(Campeonato campeonato) {
            Campeonato = campeonato;
        }

        // Lógica para atualizar texto, cor e estado ao mudar o StatusAtual
        partial void OnStatusAtualChanged(PatrocinioStatus value) {
            switch (value) {
                case PatrocinioStatus.Pendente:
                    ButtonText = "Proposta Pendente";
                    ButtonColor = Color.FromArgb("#808080"); // Cinza (como solicitado)
                    IsButtonEnabled = false; // Desabilita o botão para não enviar duas vezes
                    break;
                case PatrocinioStatus.Aceito:
                    ButtonText = "Patrocínio Ativo";
                    ButtonColor = Color.FromArgb("#008000"); // Verde (Exemplo de Aceito)
                    IsButtonEnabled = false;
                    break;
                case PatrocinioStatus.Disponivel:
                default:
                    ButtonText = "Propor Patrocínio";
                    ButtonColor = Color.FromArgb("#FFA500"); // Laranja principal
                    IsButtonEnabled = true;
                    break;
            }
        }
    }
}