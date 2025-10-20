using ArenaVirtual.ViewModels;
using ArenaVirtual.Models;
using ArenaVirtual.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Collections.Generic; // Adicionar para o Picker

namespace ArenaVirtual.Popups;

public partial class EditarPerfilPopup : ContentPage, INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // =========================================================================
    // PROPRIEDADES DE BINDING (Mapeamento dos dados do XAML)
    // =========================================================================

    private string _nome = string.Empty;
    public string Nome { // Usado por: Text="{Binding Nome}"
        get => _nome;
        set {
            if (_nome != value) {
                _nome = value;
                OnPropertyChanged(nameof(Nome));
            }
        }
    }

    private string _email = string.Empty;
    public string Email { // Usado por: Text="{Binding Email}"
        get => _email;
        set {
            if (_email != value) {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
    }

    // ... (Repetir a estrutura acima para todos os campos comuns)
    private string _telefone = string.Empty;
    public string Telefone {
        get => _telefone;
        set {
            if (_telefone != value) {
                _telefone = value;
                OnPropertyChanged(nameof(Telefone));
            }
        }
    }

    private string _localizacao = string.Empty;
    public string Localizacao {
        get => _localizacao;
        set {
            if (_localizacao != value) {
                _localizacao = value;
                OnPropertyChanged(nameof(Localizacao));
            }
        }
    }

    private string _linkRedeSocial = string.Empty;
    public string LinkRedeSocial {
        get => _linkRedeSocial;
        set {
            if (_linkRedeSocial != value) {
                _linkRedeSocial = value;
                OnPropertyChanged(nameof(LinkRedeSocial));
            }
        }
    }


    // PROPRIEDADES DE VISIBILIDADE (Usadas por IsVisible="{Binding Is...}")
    private bool _isAtleta;
    public bool IsAtleta {
        get => _isAtleta;
        set {
            if (_isAtleta != value) {
                _isAtleta = value;
                OnPropertyChanged(nameof(IsAtleta));
            }
        }
    }

    private bool _isOrganizador;
    public bool IsOrganizador {
        get => _isOrganizador;
        set {
            if (_isOrganizador != value) {
                _isOrganizador = value;
                OnPropertyChanged(nameof(IsOrganizador));
            }
        }
    }

    private bool _isPatrocinador;
    public bool IsPatrocinador {
        get => _isPatrocinador;
        set {
            if (_isPatrocinador != value) {
                _isPatrocinador = value;
                OnPropertyChanged(nameof(IsPatrocinador));
            }
        }
    }

    private bool _isArbitro;
    public bool IsArbitro {
        get => _isArbitro;
        set {
            if (_isArbitro != value) {
                _isArbitro = value;
                OnPropertyChanged(nameof(IsArbitro));
            }
        }
    }

    // PROPRIEDADE ISBUSY
    private bool _isBusy;
    public bool IsBusy {
        get => _isBusy;
        set {
            if (_isBusy != value) {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }


    // PROPRIEDADES ESPECÍFICAS (Organizador/Patrocinador)
    private string _nomeEmpresa = string.Empty;
    public string NomeEmpresa { // Usado por Organizador
        get => _nomeEmpresa;
        set {
            if (_nomeEmpresa != value) {
                _nomeEmpresa = value;
                OnPropertyChanged(nameof(NomeEmpresa));
            }
        }
    }

    private string _cnpj = string.Empty;
    public string Cnpj { // Usado por Organizador
        get => _cnpj;
        set {
            if (_cnpj != value) {
                _cnpj = value;
                OnPropertyChanged(nameof(Cnpj));
            }
        }
    }

    private string _nomeEmpresaPatrocinador = string.Empty;
    public string NomeEmpresaPatrocinador { // Usado por Patrocinador
        get => _nomeEmpresaPatrocinador;
        set {
            if (_nomeEmpresaPatrocinador != value) {
                _nomeEmpresaPatrocinador = value;
                OnPropertyChanged(nameof(NomeEmpresaPatrocinador));
            }
        }
    }

    private string _cnpjCpfPatrocinador = string.Empty;
    public string CnpjCpfPatrocinador { // Usado por Patrocinador
        get => _cnpjCpfPatrocinador;
        set {
            if (_cnpjCpfPatrocinador != value) {
                _cnpjCpfPatrocinador = value;
                OnPropertyChanged(nameof(CnpjCpfPatrocinador));
            }
        }
    }

    private string _faixaOrcamentoPatrocinio = string.Empty;
    public string FaixaOrcamentoPatrocinio { // Usado por Patrocinador
        get => _faixaOrcamentoPatrocinio;
        set {
            if (_faixaOrcamentoPatrocinio != value) {
                _faixaOrcamentoPatrocinio = value;
                OnPropertyChanged(nameof(FaixaOrcamentoPatrocinio));
            }
        }
    }

    // PROPRIEDADES ESPECÍFICAS (Atleta/Arbitro)
    private DateTime _dataNascimento = DateTime.Now;
    public DateTime DataNascimento { // Usado por Atleta
        get => _dataNascimento;
        set {
            if (_dataNascimento != value) {
                _dataNascimento = value;
                OnPropertyChanged(nameof(DataNascimento));
            }
        }
    }

    private GeneroEnum _generoSelecionado;
    public GeneroEnum GeneroSelecionado { // Usado por Atleta
        get => _generoSelecionado;
        set {
            if (_generoSelecionado != value) {
                _generoSelecionado = value;
                OnPropertyChanged(nameof(GeneroSelecionado));
            }
        }
    }

    private double? _peso;
    public double? Peso { // Usado por Atleta
        get => _peso;
        set {
            if (_peso != value) {
                _peso = value;
                OnPropertyChanged(nameof(Peso));
            }
        }
    }

    private double? _altura;
    public double? Altura { // Usado por Atleta
        get => _altura;
        set {
            if (_altura != value) {
                _altura = value;
                OnPropertyChanged(nameof(Altura));
            }
        }
    }

    private DateTime _dataNascimentoArbitro = DateTime.Now;
    public DateTime DataNascimentoArbitro { // Usado por Arbitro
        get => _dataNascimentoArbitro;
        set {
            if (_dataNascimentoArbitro != value) {
                _dataNascimentoArbitro = value;
                OnPropertyChanged(nameof(DataNascimentoArbitro));
            }
        }
    }

    private GeneroEnum _generoArbitroSelecionado;
    public GeneroEnum GeneroArbitroSelecionado { // Usado por Arbitro
        get => _generoArbitroSelecionado;
        set {
            if (_generoArbitroSelecionado != value) {
                _generoArbitroSelecionado = value;
                OnPropertyChanged(nameof(GeneroArbitroSelecionado));
            }
        }
    }

    // Propriedade para o ItemsSource do Picker
    public List<GeneroEnum> Generos { get; } = Enum.GetValues<GeneroEnum>().Cast<GeneroEnum>().ToList();

    // =========================================================================
    // CONSTRUTOR (Mapear dados do Usuario para as Propriedades)
    // =========================================================================

    private readonly Usuario _usuario;
    private readonly IAlertService _alertService;
    private readonly DatabaseService _databaseService;
    private readonly SyncService _syncService;

    public EditarPerfilPopup(Usuario usuario, IAlertService alertService, DatabaseService databaseService, SyncService syncService) {
        InitializeComponent();
        _usuario = usuario;
        _alertService = alertService;
        _databaseService = databaseService;
        _syncService = syncService;

        // É crucial definir o BindingContext para que o XAML "veja" as propriedades do Code-Behind.
        BindingContext = this;

        // Mapeamento: Usuario -> Propriedades Públicas (usadas no XAML)
        Nome = _usuario.Nome;
        Email = _usuario.Email;
        Telefone = _usuario.Telefone;
        Localizacao = _usuario.Localizacao;
        LinkRedeSocial = _usuario.LinkRedeSocial;

        // Configuração de Visibilidade
        IsAtleta = _usuario.Perfil == TipoPerfil.Atleta;
        IsOrganizador = _usuario.Perfil == TipoPerfil.Organizador;
        IsPatrocinador = _usuario.Perfil == TipoPerfil.Patrocinador;
        IsArbitro = _usuario.Perfil == TipoPerfil.Arbitro;

        // Configuração de Dados Específicos
        if (IsAtleta) {
            GeneroSelecionado = _usuario.Genero ?? Generos.First();
            DataNascimento = _usuario.DataNascimento ?? DateTime.Now;
            Peso = _usuario.Peso;
            Altura = _usuario.Altura;
        }
        if (IsOrganizador) {
            NomeEmpresa = _usuario.NomeEmpresa;
            Cnpj = _usuario.CNPJ;
        }
        if (IsPatrocinador) {
            NomeEmpresaPatrocinador = _usuario.NomeEmpresa;
            CnpjCpfPatrocinador = _usuario.CNPJ;
            FaixaOrcamentoPatrocinio = _usuario.FaixaOrcamentoPatrocinio;
        }
        if (IsArbitro) {
            GeneroArbitroSelecionado = _usuario.Genero ?? Generos.First();
            DataNascimentoArbitro = _usuario.DataNascimento ?? DateTime.Now;
        }

        // O XAML já possui os elementos visuais definidos, agora é só garantir
        // que o BindingContext esteja correto para que as propriedades Is... funcionem.
    }

    // =========================================================================
    // MÉTODOS CLICKED (Agora usam as propriedades mapeadas)
    // =========================================================================

    private async void Cancelar_Clicked(object sender, EventArgs e) {
        await Navigation.PopModalAsync();
    }

    private async void Salvar_Clicked(object sender, EventArgs e) {
        if (IsBusy) return;

        IsBusy = true; // Ativa o indicador de carregamento.

        try {
            // Mapeamento Reverso: Propriedades Públicas -> Objeto Usuario
            _usuario.Nome = Nome;
            _usuario.Email = Email;
            _usuario.Telefone = Telefone;
            _usuario.Localizacao = Localizacao;
            _usuario.LinkRedeSocial = LinkRedeSocial;

            if (IsAtleta) {
                _usuario.DataNascimento = DataNascimento;
                _usuario.Genero = GeneroSelecionado;
                _usuario.Peso = Peso;
                _usuario.Altura = Altura;
            }
            if (IsOrganizador) {
                _usuario.NomeEmpresa = NomeEmpresa;
                _usuario.CNPJ = Cnpj;
            }
            if (IsPatrocinador) {
                _usuario.NomeEmpresa = NomeEmpresaPatrocinador;
                _usuario.CNPJ = CnpjCpfPatrocinador;
                _usuario.FaixaOrcamentoPatrocinio = FaixaOrcamentoPatrocinio;
            }
            if (IsArbitro) {
                _usuario.DataNascimento = DataNascimentoArbitro;
                _usuario.Genero = GeneroArbitroSelecionado;
            }

            _usuario.IsSynced = false;
            _usuario.UpdatedAt = DateTime.UtcNow;

            await _databaseService.AtualizarUsuarioAsync(_usuario);

            Debug.WriteLine("[EditarPerfilPopup] Perfil de usuário atualizado localmente. Disparando sincronização...");
            await _syncService.SyncAsync(new Progress<string>());

            MessagingCenter.Send(this, "Perfil Atualizado", _usuario);

            await _alertService.DisplayAlert("Sucesso", "Perfil atualizado com sucesso!", "OK");
            await Navigation.PopModalAsync();
        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Ocorreu um erro ao salvar o perfil: {ex.Message}", "OK");
        } finally {
            IsBusy = false; // Desativa o indicador de carregamento, mesmo em caso de erro.
        }
    }

    private async void ExcluirConta_Clicked(object sender, EventArgs e) {
        if (IsBusy) return;

        // 1. Confirmação do Usuário. Agora funciona com a sobrecarga de 4 argumentos no IAlertService.
        bool confirmacao = await _alertService.DisplayAlert(
            "Confirmação de Exclusão",
            "Tem certeza que deseja EXCLUIR sua conta? Esta ação é irreversível e você perderá todos os seus dados locais.",
            "Sim, Excluir", // Botão de aceitar
            "Cancelar"); // Botão de cancelar

        if (!confirmacao) {
            return;
        }

        IsBusy = true; // Ativa o indicador de carregamento.

        try {
            // 2. Exclusão Local
            int linhasDeletadas = await _databaseService.DeletarUsuarioAsync(_usuario);

            if (linhasDeletadas > 0) {
                Debug.WriteLine($"[EditarPerfilPopup] Usuário ID {_usuario.Id} excluído localmente.");

                // Notificar sobre o logout e exclusão.
                MessagingCenter.Send(this, "Conta Excluída");

                await _alertService.DisplayAlert("Conta Excluída", "Sua conta foi excluída localmente. Você será desconectado e precisará entrar novamente para finalizar a sincronização.", "OK");

                // 3. Redirecionar para a tela inicial (Login/Abertura)
                await Navigation.PopModalAsync(); // Fecha o Popup.

            } else {
                await _alertService.DisplayAlert("Erro", "Não foi possível excluir o usuário. O registro não foi encontrado no banco de dados local.", "OK");
            }

        } catch (Exception ex) {
            await _alertService.DisplayAlert("Erro", $"Ocorreu um erro ao excluir a conta: {ex.Message}", "OK");
        } finally {
            IsBusy = false; // Desativa o indicador de carregamento.
        }
    }
}