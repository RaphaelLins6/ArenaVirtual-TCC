using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;
using System.Diagnostics;

// Assumindo que você tem uma interface ISyncable e um enum JogoStatus definidos no mesmo namespace.
// O namespace deve corresponder ao seu projeto
namespace ArenaVirtual.Models {

    // A classe Jogo precisa ser parcial para que o CommunityToolkit.Mvvm gere os métodos e propriedades
    public partial class Jogo : ObservableObject, ISyncable {

        // --- Propriedades Reativas Geradas (MVVM Toolkit) ---

        // Chave Primária Local
        [ObservableProperty]
        [property: PrimaryKey, AutoIncrement]
        private int id;

        // ID de sincronização (deve ser a implementação de ISyncable.ClientAppId)
        [ObservableProperty]
        private Guid clientAppId;

        // ArbitroId
        [ObservableProperty]
        private Guid? arbitroId;

        [ObservableProperty]
        private int timeAId;

        [ObservableProperty]
        private int timeBId;

        [ObservableProperty]
        private DateTime dataHora;

        [ObservableProperty]
        private int campeonatoId;

        [ObservableProperty]
        private string local = string.Empty;

        [ObservableProperty]
        private string placarA = string.Empty;

        [ObservableProperty]
        private string placarB = string.Empty;

        // O tipo JogoStatus deve estar definido em outro arquivo no namespace ArenaVirtual.Models
        [ObservableProperty]
        private JogoStatus status;

        [ObservableProperty]
        private int placarTimeAInt;

        [ObservableProperty]
        private int placarTimeBInt;

        // Propriedades Ignoradas pelo SQLite (apenas para exibição/lógica)
        [ObservableProperty]
        [property: Ignore]
        private Time? timeA;

        [ObservableProperty]
        [property: Ignore]
        private Time? timeB;

        [ObservableProperty]
        [property: Ignore] // Nome do árbitro é carregado do usuário, não armazenado aqui.
        private string nomeArbitro = string.Empty;

        [ObservableProperty]
        [property: Ignore]
        private bool isOrganizador;

        // --- Propriedades POCO/Interfaces ---
        // Estas não são reativas (não usam ObservableProperty)
        public int Rodada { get; set; }
        public string NomeCampeonato { get; set; } = string.Empty;
        public bool IsSynced { get; set; } // Propriedade da interface ISyncable
        public DateTime UpdatedAt { get; set; } // Propriedade da interface ISyncable


        // --- Construtor ---

        public Jogo() {
            // Inicialização de valores padrão
            this.ClientAppId = Guid.NewGuid(); // Sempre defina um ID de app cliente
            this.IsSynced = false;
            this.UpdatedAt = DateTime.UtcNow;

            this.Status = JogoStatus.Agendado;
            this.IsOrganizador = false;
        }


        // --- PROPRIEDADES CALCULADAS E CALLBACKS ---

        // CRÍTICA: Se ArbitroId é nulo OU é Guid.Empty, o árbitro não está atribuído.
        private bool ArbitroAtribuido => ArbitroId.HasValue && ArbitroId.Value != Guid.Empty;

        [Ignore]
        // NOVA LÓGICA: O botão está habilitado APENAS se for o Organizador.
        public bool BotaoArbitroHabilitado => IsOrganizador;


        [Ignore]
        // TEXTO DO BOTÃO: Depende do IsOrganizador e ArbitroAtribuido.
        public string TextoBotaoArbitro => !IsOrganizador
    // 1. Não-Organizador: Exibe o árbitro ou só 'Detalhes'
                ? (ArbitroAtribuido && !string.IsNullOrEmpty(NomeArbitro) ? $"Árbitro: {NomeArbitro}" : "Detalhes")
    // 2. Organizador: Exibe o árbitro ou 'Anexar'
                : (ArbitroAtribuido
            ? $"Árbitro: {(!string.IsNullOrEmpty(NomeArbitro) ? NomeArbitro : "Anexado")}"
            : "Anexar Árbitros");


        [Ignore]
        // PROPRIEDADE DE COMPATIBILIDADE: A desabilitação é o oposto de BotaoArbitroHabilitado.
        // Se não for Organizador (BotaoArbitroHabilitado é false), o botão fica desabilitado
        // (Isso não impede que o usuário veja a página de detalhes, mas impede o clique para atribuir/trocar).
        // CORREÇÃO: Usar a negação direta é mais seguro:
        public bool BotaoArbitroDesabilitado => !BotaoArbitroHabilitado;


        // --- CALLBACKS DO MVVM TOOLKIT ---

        partial void OnArbitroIdChanged(Guid? value) {
            Debug.WriteLine($"[JOGO MODEL] ArbitroId alterado para: {value}. Chamando Notificação.");
            NotifyArbitroStatusChanged();
        }

        partial void OnNomeArbitroChanged(string value) {
            Debug.WriteLine($"[JOGO MODEL] NomeArbitro alterado para: {value}. Chamando Notificação.");
            NotifyArbitroStatusChanged();
        }

        partial void OnIsOrganizadorChanged(bool value) {
            Debug.WriteLine($"[JOGO MODEL] IsOrganizador alterado para: {value}. Chamando Notificação.");
            NotifyArbitroStatusChanged();
        }

        // --- MÉTODO DE NOTIFICAÇÃO PÚBLICO ---

        /// <summary>
        /// Força a UI a reavaliar as propriedades dependentes do status do árbitro (texto e estado do botão).
        /// Deve ser chamado sempre que uma mudança externa impactar o estado do árbitro (ex: após salvar o árbitro no DB).
        /// </summary>
        public void NotifyArbitroStatusChanged() {
            // Notificar as propriedades calculadas.
            OnPropertyChanged(nameof(TextoBotaoArbitro));
            OnPropertyChanged(nameof(BotaoArbitroHabilitado));
            OnPropertyChanged(nameof(BotaoArbitroDesabilitado));

            // Notificar as propriedades base (útil para debug ou caso algum binding direto exista).
            OnPropertyChanged(nameof(ArbitroId));
            OnPropertyChanged(nameof(NomeArbitro));

            Debug.WriteLine($"[JOGO MODEL] Notificações de Status de Árbitro disparadas. Texto Botão: {TextoBotaoArbitro}");
        }
    }
}