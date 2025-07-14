using ArenaVirtual.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels {
    public partial class LoginViewModel : ObservableObject {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string senha;

        [RelayCommand]
        private async Task EntrarAsync() {
            await Shell.Current.DisplayAlert("Login", "Login concluído", "OK");
            await Shell.Current.GoToAsync("PerfilPage"); 
        }

        [RelayCommand]
        private async Task RegistrarAsync() {
            await Shell.Current.GoToAsync("RegisterPage");
        }
    }
}
