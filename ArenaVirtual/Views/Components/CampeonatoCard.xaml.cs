using ArenaVirtual.Models; 
using System.Windows.Input; 
using System.Diagnostics; 

namespace ArenaVirtual.Views.Components {
    public partial class CampeonatoCard : ContentView {

        public static readonly BindableProperty CampeonatoItemProperty =
            BindableProperty.Create(
                propertyName: nameof(CampeonatoItem),
                returnType: typeof(Campeonato),
                declaringType: typeof(CampeonatoCard),
                defaultValue: null,
                defaultBindingMode: BindingMode.OneWay
            );

        public Campeonato CampeonatoItem {
            get => (Campeonato)GetValue(CampeonatoItemProperty);
            set => SetValue(CampeonatoItemProperty, value);
        }

        public static readonly BindableProperty FavoritarCommandProperty =
            BindableProperty.Create(nameof(FavoritarCommand), typeof(ICommand), typeof(CampeonatoCard), null);

        public ICommand FavoritarCommand {
            get => (ICommand)GetValue(FavoritarCommandProperty);
            set => SetValue(FavoritarCommandProperty, value);
        }

        public static readonly BindableProperty ParticiparCommandProperty =
            BindableProperty.Create(nameof(ParticiparCommand), typeof(ICommand), typeof(CampeonatoCard), null);

        public ICommand ParticiparCommand {
            get => (ICommand)GetValue(ParticiparCommandProperty);
            set => SetValue(ParticiparCommandProperty, value);
        }

        public static readonly BindableProperty VerCampeonatoCommandProperty =
            BindableProperty.Create(nameof(VerCampeonatoCommand), typeof(ICommand), typeof(CampeonatoCard), null);

        public ICommand VerCampeonatoCommand {
            get => (ICommand)GetValue(VerCampeonatoCommandProperty);
            set => SetValue(VerCampeonatoCommandProperty, value);
        }

        public CampeonatoCard() {
            InitializeComponent();
            //Debug.WriteLine($"[CampeonatoCard] Instanciado. BindingContext inicial: {this.BindingContext?.GetType().Name ?? "Nulo"}");
        }

        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();
            if (this.BindingContext is Campeonato camp) {
                //Debug.WriteLine($"[CampeonatoCard] BindingContext alterado para Campeonato: {camp.Nome ?? "N/A"}, ID: {camp.Id}");
            } else {
                //Debug.WriteLine($"[CampeonatoCard] BindingContext alterado para tipo: {this.BindingContext?.GetType().Name ?? "Nulo"}");
            }
        }
    }
}
