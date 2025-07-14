using ArenaVirtual.Models;
using ArenaVirtual.Views;
using MvvmHelpers;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace ArenaVirtual.ViewModels {
    public partial class LoginViewModel : ObservableObject {
        public ICommand RegisterCommand { get; }

        public LoginViewModel() {
            RegisterCommand = new RelayCommand(OnRegister);
        }

        private async void OnRegister() {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}