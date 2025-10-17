using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.ViewModels.Patrocinador {
    public partial class CampeonatoPatrocinioItemViewModel : ObservableObject {
        public Campeonato Campeonato { get; set; }
        public string Nome => Campeonato?.Nome;
        public string Descricao => Campeonato?.Descricao;
        public string LogoUrl => Campeonato?.LogoUrl;

        // Propriedade para controlar o estado do botão (se já patrocinou, etc.)
        [ObservableProperty]
        private string buttonText = "Propor Patrocínio";

        [ObservableProperty]
        private bool isButtonEnabled = true;

        // Cor laranja para o botão de ação principal
        [ObservableProperty]
        private Color buttonColor = Color.FromArgb("#FFA500");

        public CampeonatoPatrocinioItemViewModel(Campeonato campeonato) {
            Campeonato = campeonato;
        }
    }
}
