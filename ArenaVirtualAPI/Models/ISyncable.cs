// Models/ISyncable.cs (ou um arquivo de interfaces genéricas)
namespace ArenaVirtualAPI.Models {
    public interface ISyncable {
        int Id { get; set; }
        DateTime UpdatedAt { get; set; }
        bool IsSynced { get; set; } // Opcional, dependendo da sua estratégia de sync
    }
}