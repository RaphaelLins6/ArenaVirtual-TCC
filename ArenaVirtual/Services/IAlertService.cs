namespace ArenaVirtual.Services {
    public class AlertService : IAlertService {
        public async Task DisplayAlert(string title, string message, string cancel) {
            await Task.Delay(100);

            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;

            if (mainPage != null) {
                //System.Diagnostics.Debug.WriteLine($"[AlertService] MainPage: {mainPage.GetType().Name}");
                await mainPage.DisplayAlert(title, message, cancel);
            } else {
                //System.Diagnostics.Debug.WriteLine($"ALERTA: MainPage é nulo ao tentar exibir '{title} - {message}'");
            }
        }

        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel) {
            await Task.Delay(100);

            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;

            if (mainPage != null) {
                //System.Diagnostics.Debug.WriteLine($"[AlertService] MainPage: {mainPage.GetType().Name}");
                return await mainPage.DisplayAlert(title, message, accept, cancel); 
            } else {
                //System.Diagnostics.Debug.WriteLine($"ALERTA: MainPage é nulo ao tentar exibir confirmação '{title} - {message}'");
                return false; 
            }
        }

        public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons) {
            return MainThread.InvokeOnMainThreadAsync(() => {
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;

                if (mainPage != null) {
                    //System.Diagnostics.Debug.WriteLine($"[AlertService] MainPage: {mainPage.GetType().Name}");
                    return mainPage.DisplayActionSheet(title, cancel, destruction, buttons);
                }
                //System.Diagnostics.Debug.WriteLine("[AlertService] MainPage está nulo!");
                return Task.FromResult(string.Empty); 
            });
        }
    }

    public interface IAlertService {
        Task DisplayAlert(string title, string message, string cancel);
        Task<bool> DisplayAlert(string title, string message, string accept, string cancel); // NOVO MÉTODO
        Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);
    }
}