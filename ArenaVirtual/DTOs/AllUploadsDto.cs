namespace ArenaVirtual.DTOs {
    public class AllUploadsDto {
        public List<UsuarioSyncDto>? Usuarios { get; set; }
        public List<CampeonatoSyncDto>? Campeonatos { get; set; }
        public List<TimeSyncDto>? Times { get; set; }
        public List<ConviteSyncDto>? Convites { get; set; }
        public List<UsuarioCampeonatoFavoritoSyncDto>? UsuarioCampeonatoFavoritos { get; set; }
    }
}