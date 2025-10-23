namespace ArenaVirtual.Models {
    public interface ISyncable {
        int Id { get; set; }
        Guid ClientAppId { get; set; } 
        DateTime UpdatedAt { get; set; }
        bool IsSynced { get; set; }
    }
}