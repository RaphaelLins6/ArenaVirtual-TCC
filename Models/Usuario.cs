using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaVirtual.Models {
    public class Usuario {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty; // Inicializado para evitar valores nulos
        public string Email { get; set; } = string.Empty; // Inicializado para evitar valores nulos
        public string Senha { get; set; } = string.Empty; // Inicializado para evitar valores nulos
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
        public string Perfil { get; set; } = string.Empty; // Inicializado para evitar valores nulos

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
