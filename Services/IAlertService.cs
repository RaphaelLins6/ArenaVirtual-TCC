// Services/AlertService.cs
using Microsoft.Maui.Controls;

namespace ArenaVirtual.Services {
    public class AlertService : IAlertService {
        public async Task DisplayAlert(string title, string message, string cancel) {
            // Gambiarra: atraso para testar timing de inicialização
            await Task.Delay(100);

            if (Application.Current?.MainPage != null) {
                System.Diagnostics.Debug.WriteLine($"[AlertService] MainPage: {Application.Current.MainPage.GetType().Name}");
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            } else {
                System.Diagnostics.Debug.WriteLine($"ALERTA: MainPage é nulo ao tentar exibir '{title} - {message}'");
            }
        }

        public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons) {
            return MainThread.InvokeOnMainThreadAsync(() => {
                if (Application.Current?.MainPage != null) {
                    System.Diagnostics.Debug.WriteLine($"[AlertService] MainPage: {Application.Current.MainPage.GetType().Name}");
                    return Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons);
                }
                System.Diagnostics.Debug.WriteLine("[AlertService] MainPage está nulo!");
                return Task.FromResult<string>(null);
            });
        }
    }
    public interface IAlertService {
        Task DisplayAlert(string title, string message, string cancel);
    }
}