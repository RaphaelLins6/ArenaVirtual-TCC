namespace ArenaVirtual.Models {
    public interface ISyncable {
        int Id { get; set; }
        Guid ClientAppId { get; set; } // Adicione esta linha
        DateTime UpdatedAt { get; set; }
        bool IsSynced { get; set; }
    }
}