using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.DTOs {
    public class UsuarioUpdateDto {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? NovaSenha { get; set; }
        public TipoPerfil Perfil { get; set; }
        public string? ImagemPath { get; set; }
        public string? Localizacao { get; set; }
        public string? Telefone { get; set; }
        public string? LinkRedeSocial { get; set; }
        public DateTime? DataNascimento { get; set; }
        public GeneroEnum? Genero { get; set; }
        public string? NomeEmpresa { get; set; }
        public string? CNPJ { get; set; }
        public double? Peso { get; set; }
        public double? Altura { get; set; }
        public string? FaixaOrcamentoPatrocinio { get; set; }
        public Guid? TimeClientAppId { get; set; }
    }
}
