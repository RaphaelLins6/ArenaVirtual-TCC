namespace ArenaVirtualAPI.Models {
    public interface ISyncable {
        int Id { get; set; }
        public Guid ClientAppId { get; set; } // Adicione esta linha
        public bool IsSynced { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}