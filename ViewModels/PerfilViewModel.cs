using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;

namespace ArenaVirtual.ViewModels {
    public partial class PerfilViewModel : ObservableObject {
        private readonly IAlertService _alertService;

        // Campos comuns
        [ObservableProperty] private string saudacao = string.Empty;
        [ObservableProperty] private string nomeUsuario = string.Empty;
        [ObservableProperty] private string emailUsuario = string.Empty;
        [ObservableProperty] private string tipoPerfilUsuario = string.Empty;
        [ObservableProperty] private string localizacaoUsuario = string.Empty;
        [ObservableProperty] private string telefoneUsuario = string.Empty;
        [ObservableProperty] private string linkRedeSocialUsuario = string.Empty;

        // Campos específicos
        [ObservableProperty] private DateTime? dataNascimentoUsuario;
        [ObservableProperty] private GeneroEnum? generoUsuario;
        [ObservableProperty] private string nomeEmpresaUsuario = string.Empty;
        [ObservableProperty] private string cnpjUsuario = string.Empty;
        [ObservableProperty] private double? pesoUsuario;
        [ObservableProperty] private double? alturaUsuario = 0;
        [ObservableProperty] private string faixaOrcamentoPatrocinioUsuario = string.Empty;

        // Exemplo de propriedades de visibilidade no ViewModel
        [ObservableProperty] private bool isAtleta;
        [ObservableProperty] private bool isOrganizador;
        [ObservableProperty] private bool isArbitro;
        [ObservableProperty] private bool isPatrocinador;

        [ObservableProperty]
        private Usuario usuarioLogado;

        public PerfilViewModel(Usuario usuario, IAlertService alertService) {
            UsuarioLogado = usuario; // Use sempre a propriedade gerada!
            _alertService = alertService;
            CarregarDadosDoUsuario();

            MessagingCenter.Subscribe<object, Usuario>(this, "PerfilAtualizado", (sender, usuarioAtualizado) => {
                MainThread.BeginInvokeOnMainThread(() => {
                    UsuarioLogado = usuarioAtualizado; // Correto: usa a propriedade!
                    CarregarDadosDoUsuario();
                });
            });
        }

        public void CarregarDadosDoUsuario() {
            if (UsuarioLogado != null) {
                if (DateTime.Now.Hour < 12) Saudacao = "Bom dia,";
                else if (DateTime.Now.Hour < 18) Saudacao = "Boa tarde,";
                else Saudacao = "Boa noite,";

                NomeUsuario = UsuarioLogado.Nome;
                EmailUsuario = UsuarioLogado.Email;
                TipoPerfilUsuario = UsuarioLogado.Perfil.ToString();
                LocalizacaoUsuario = UsuarioLogado.Localizacao;
                TelefoneUsuario = UsuarioLogado.Telefone;
                LinkRedeSocialUsuario = UsuarioLogado.LinkRedeSocial;

                // Limpa campos específicos
                DataNascimentoUsuario = null;
                GeneroUsuario = null;
                NomeEmpresaUsuario = string.Empty;
                CnpjUsuario = string.Empty;
                PesoUsuario = null;
                AlturaUsuario = null;
                FaixaOrcamentoPatrocinioUsuario = string.Empty;

                // Preenche campos específicos conforme o perfil
                switch (UsuarioLogado.Perfil) {
                    case TipoPerfil.Atleta:
                        DataNascimentoUsuario = UsuarioLogado.DataNascimento;
                        GeneroUsuario = UsuarioLogado.Genero;
                        PesoUsuario = UsuarioLogado.Peso;
                        AlturaUsuario = UsuarioLogado.Altura;
                        break;
                    case TipoPerfil.Organizador:
                        NomeEmpresaUsuario = UsuarioLogado.NomeEmpresa;
                        CnpjUsuario = UsuarioLogado.CNPJ;
                        break;
                    case TipoPerfil.Arbitro:
                        DataNascimentoUsuario = UsuarioLogado.DataNascimento;
                        GeneroUsuario = UsuarioLogado.Genero;
                        break;
                    case TipoPerfil.Patrocinador:
                        NomeEmpresaUsuario = UsuarioLogado.NomeEmpresa;
                        CnpjUsuario = UsuarioLogado.CNPJ;
                        FaixaOrcamentoPatrocinioUsuario = UsuarioLogado.FaixaOrcamentoPatrocinio;
                        break;
                }

                IsAtleta = UsuarioLogado.Perfil == TipoPerfil.Atleta;
                IsOrganizador = UsuarioLogado.Perfil == TipoPerfil.Organizador;
                IsArbitro = UsuarioLogado.Perfil == TipoPerfil.Arbitro;
                IsPatrocinador = UsuarioLogado.Perfil == TipoPerfil.Patrocinador;
            }
        }

        [RelayCommand]
        private async Task EditarPerfil() {
            // Obtenha os serviços necessários do contêiner de injeção de dependência
            var services = App.Current?.Handler?.MauiContext?.Services;
            if (services == null) {
                // Lidar com o caso em que os serviços não estão disponíveis
                return;
            }

            var databaseService = services.GetRequiredService<DatabaseService>();
            var syncService = services.GetRequiredService<SyncService>();

            // Agora, passe todos os argumentos necessários para o construtor do popup
            var popup = new EditarPerfilPopup(UsuarioLogado, _alertService, databaseService, syncService);
            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        [RelayCommand]
        private async Task AlterarSenha() {
            // Obtenha os serviços necessários do contêiner de injeção de dependência
            var services = App.Current?.Handler?.MauiContext?.Services;
            if (services == null) {
                // Lidar com o caso em que os serviços não estão disponíveis
                return;
            }

            var databaseService = services.GetRequiredService<DatabaseService>();
            var syncService = services.GetRequiredService<SyncService>();

            // Agora, passe todos os argumentos necessários para o construtor do popup
            var popup = new AlterarSenhaPopup(UsuarioLogado, _alertService, databaseService, syncService);
            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        public string DataNascimentoUsuarioFormatado =>
            DataNascimentoUsuario.HasValue
                ? DataNascimentoUsuario.Value.ToString("dd/MM/yyyy")
                : string.Empty;
    }
}
