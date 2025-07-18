using ArenaVirtual.Models;
using ArenaVirtual.Popups;
using ArenaVirtual.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        private Usuario _usuarioLogado;

        public PerfilViewModel(Usuario usuario, IAlertService alertService) {
            _usuarioLogado = usuario;
            _alertService = alertService;
            CarregarDadosDoUsuario();
        }

        public void CarregarDadosDoUsuario() {
            if (_usuarioLogado != null) {
                // Campos comuns
                if (DateTime.Now.Hour < 12) Saudacao = "Bom dia,";
                else if (DateTime.Now.Hour < 18) Saudacao = "Boa tarde,";
                else Saudacao = "Boa noite,";

                NomeUsuario = _usuarioLogado.Nome;
                EmailUsuario = _usuarioLogado.Email;
                TipoPerfilUsuario = _usuarioLogado.Perfil.ToString();
                LocalizacaoUsuario = _usuarioLogado.Localizacao;
                telefoneUsuario = _usuarioLogado.Telefone;
                linkRedeSocialUsuario = _usuarioLogado.LinkRedeSocial;

                // Limpa campos específicos
                dataNascimentoUsuario = null;
                generoUsuario = null;
                nomeEmpresaUsuario = string.Empty;
                cnpjUsuario = string.Empty;
                pesoUsuario = null;
                alturaUsuario = null;
                faixaOrcamentoPatrocinioUsuario = string.Empty;

                // Preenche campos específicos conforme o perfil
                switch (_usuarioLogado.Perfil) {
                    case TipoPerfil.Atleta:
                        dataNascimentoUsuario = _usuarioLogado.DataNascimento;
                        generoUsuario = _usuarioLogado.Genero;
                        pesoUsuario = _usuarioLogado.Peso;
                        alturaUsuario = _usuarioLogado.Altura;
                        break;
                    case TipoPerfil.Organizador:
                        nomeEmpresaUsuario = _usuarioLogado.NomeEmpresa;
                        cnpjUsuario = _usuarioLogado.CNPJ;
                        break;
                    case TipoPerfil.Arbitro:
                        dataNascimentoUsuario = _usuarioLogado.DataNascimento;
                        break;
                    case TipoPerfil.Patrocinador:
                        nomeEmpresaUsuario = _usuarioLogado.NomeEmpresa;
                        cnpjUsuario = _usuarioLogado.CNPJ;
                        faixaOrcamentoPatrocinioUsuario = _usuarioLogado.FaixaOrcamentoPatrocinio;
                        break;
                }

                IsAtleta = _usuarioLogado.Perfil == TipoPerfil.Atleta;
                IsOrganizador = _usuarioLogado.Perfil == TipoPerfil.Organizador;
                IsArbitro = _usuarioLogado.Perfil == TipoPerfil.Arbitro;
                IsPatrocinador = _usuarioLogado.Perfil == TipoPerfil.Patrocinador;
            }
        }

        [RelayCommand]
        private async Task EditarPerfil() {
            var popup = new EditarPerfilPopup(_usuarioLogado, _alertService);
            popup.PerfilAtualizado += (s, usuarioAtualizado) => {
                _usuarioLogado = usuarioAtualizado;
                CarregarDadosDoUsuario();
            };

            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        [RelayCommand]
        private async Task AlterarSenha() {
            var popup = new AlterarSenhaPopup(_usuarioLogado, _alertService);
            await Shell.Current.Navigation.PushModalAsync(popup);
        }

        public string DataNascimentoUsuarioFormatado =>
            DataNascimentoUsuario.HasValue
                ? DataNascimentoUsuario.Value.ToString("dd/MM/yyyy")
                : string.Empty;
    }
}
