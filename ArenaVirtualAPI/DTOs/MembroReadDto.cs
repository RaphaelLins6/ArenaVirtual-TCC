namespace ArenaVirtualAPI.DTOs {
    public class MembroReadDto {
        public Guid ClientAppId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? ImagemPath { get; set; } 
        public bool IsCapitao { get; set; } 
    }
}
