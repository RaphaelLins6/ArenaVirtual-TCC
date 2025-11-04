namespace ArenaVirtualAPI.DTOs {
    public class TimeReadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string Nome { get; set; }
        public string? LogoUrl { get; set; }
        public string Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public int QuantidadeMembros { get; set; }
        public int CapitaoId { get; set; }

        public Guid? CapitaoClientAppId { get; set; }
        public IEnumerable<MembroReadDto> Membros { get; set; } = new List<MembroReadDto>();
    }
}