namespace ArenaVirtualAPI.Models {
    public interface ISyncable {
        int Id { get; set; }
        DateTime UpdatedAt { get; set; }
        bool IsSynced { get; set; }
    }
}