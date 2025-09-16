using Microsoft.Maui.Networking;
using System;

namespace ArenaVirtual.Services {
    public class ConnectivityService {
        // Propriedade para verificar o status de conexão
        public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        // Evento que você pode assinar de fora da classe
        public event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;

        public ConnectivityService() {
            // Assina o evento nativo do MAUI
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e) {
            // Aciona o seu próprio evento para que os ViewModels possam se atualizar
            ConnectivityChanged?.Invoke(this, e);
        }
    }
}