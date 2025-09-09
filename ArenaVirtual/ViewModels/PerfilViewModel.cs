using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace ArenaVirtual.ViewModels {
    // Injeção de dependência via construtor, uma prática mais limpa
    public partial class PerfilViewModel(IAlertService alertService, DatabaseService databaseService, SyncService syncService) : ObservableObject {

        private readonly IAlertService _alertService = alertService;
        private readonly DatabaseService _databaseService = databaseService;
        private readonly SyncService _syncService = syncService;

        // Variável de controle para evitar recursão
        private bool _isUpdating = false;

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

        public PerfilViewModel(Usuario usuario, IAlertService alertService, DatabaseService databaseService, SyncService syncService) : this(alertService, databaseService, syncService) {
            UsuarioLogado = usuario;
            CarregarDadosDoUsuario();

            MessagingCenter.Subscribe<object, Usuario>(this, "PerfilAtualizado", (sender, usuarioAtualizado) => {
                // Checa a variável de controle para evitar a recursão
                if (_isUpdating) return;

                MainThread.BeginInvokeOnMainThread(() => {
                    _isUpdating = true; // Inicia o bloqueio

                    UsuarioLogado = usuarioAtualizado;
                    CarregarDadosDoUsuario();

                    _isUpdating = false; // Libera o bloqueio
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
            // Os serviços já estão disponíveis via injeção de dependência no construtor
            var popup = new EditarPerfilPopup(UsuarioLogado, _alertService, _databaseService, _syncService);
            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        [RelayCommand]
        private async Task AlterarSenha() {
            // Os serviços já estão disponíveis via injeção de dependência no construtor
            var popup = new AlterarSenhaPopup(UsuarioLogado, _alertService, _databaseService, _syncService);
            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        public string DataNascimentoUsuarioFormatado =>
            DataNascimentoUsuario.HasValue
                ? DataNascimentoUsuario.Value.ToString("dd/MM/yyyy")
                : string.Empty;
    }
}