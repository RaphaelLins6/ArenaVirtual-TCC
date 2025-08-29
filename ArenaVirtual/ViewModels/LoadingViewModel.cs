using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArenaVirtual.ViewModels {
    public partial class LoadingViewModel : ObservableObject {
        [ObservableProperty]
        private string message = "Iniciando...";

        public IProgress<string> Progress { get; }

        public LoadingViewModel() {
            Progress = new Progress<string>(newMessage => {
                Message = newMessage;
            });
        }
    }
}
