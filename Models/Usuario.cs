using SQLite;

namespace ArenaVirtual.Models {
    public enum TipoPerfil {
        Atleta,
        Arbitro,
        Patrocinador,
        Organizador
    }

    public class Usuario {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
        public TipoPerfil Perfil { get; set; }
        public string ImagemPath { get; set; } = string.Empty; // <-- O NOME CORRETO É IMAGEMPATH!
        public static TipoPerfil[] PerfilTipos => Enum.GetValues<TipoPerfil>();

        public Usuario() {
            DataCadastro = DateTime.Now;
            Ativo = true;
        }

        public void Ativar() {
            Ativo = true;
        }

        public void Desativar() {
            Ativo = false;
        }
    }
}