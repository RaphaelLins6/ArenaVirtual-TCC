using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArenaVirtual.ViewModels.Patrocinador {

    public enum PatrocinioStatus {
        Disponivel, 
        Pendente,   
        Aceito      
    }

    public partial class CampeonatoPatrocinioItemViewModel : ObservableObject {
        public Campeonato Campeonato { get; set; }
        public string Nome => Campeonato?.Nome;
        public string Descricao => Campeonato?.Descricao;
        public string LogoUrl => Campeonato?.LogoUrl;

        [ObservableProperty]
        private PatrocinioStatus statusAtual = PatrocinioStatus.Disponivel;

        [ObservableProperty]
        private string buttonText = "Propor Patrocínio";

        [ObservableProperty]
        private bool isButtonEnabled = true;

        [ObservableProperty]
        private Color buttonColor = Color.FromArgb("#FFA500");

        public CampeonatoPatrocinioItemViewModel(Campeonato campeonato) {
            Campeonato = campeonato;
        }

        partial void OnStatusAtualChanged(PatrocinioStatus value) {
            switch (value) {
                case PatrocinioStatus.Pendente:
                    ButtonText = "Proposta Pendente";
                    ButtonColor = Color.FromArgb("#808080"); 
                    IsButtonEnabled = false; 
                    break;
                case PatrocinioStatus.Aceito:
                    ButtonText = "Patrocínio Ativo";
                    ButtonColor = Color.FromArgb("#008000"); 
                    IsButtonEnabled = false;
                    break;
                case PatrocinioStatus.Disponivel:
                default:
                    ButtonText = "Propor Patrocínio";
                    ButtonColor = Color.FromArgb("#FFA500"); 
                    IsButtonEnabled = true;
                    break;
            }
        }
    }
}