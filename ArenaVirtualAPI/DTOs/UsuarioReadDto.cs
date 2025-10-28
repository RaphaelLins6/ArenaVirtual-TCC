using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.DTOs {
    public class UsuarioReadDto {
        public int Id { get; set; }
        public Guid ClientAppId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Perfil { get; set; }
        public string ImagemPath { get; set; }
        public string Localizacao { get; set; }
        public string Telefone { get; set; }
        public string LinkRedeSocial { get; set; }
        public DateTime? DataNascimento { get; set; }
        public GeneroEnum? Genero { get; set; }
        public string NomeEmpresa { get; set; }
        public string CNPJ { get; set; }
        public double? Peso { get; set; }
        public double? Altura { get; set; }
        public string FaixaOrcamentoPatrocinio { get; set; }
        public int TimeId { get; set; }
        public Guid? TimeClientAppId { get; set; }

    }
}
