using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Data;
using System.Threading.Tasks;

namespace ArenaVirtualAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly ApiDbContext _context;

        public AuthController(ApiDbContext context) => _context = context;

        [HttpPost("login")]
        public async Task<ActionResult<UsuarioReadDto>> Login([FromBody] LoginDto dto) {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Unauthorized("Credenciais inválidas.");

            //var ok = BCrypt.Net.BCrypt.Verify(dto.Senha, user.SenhaHash);
            //if (!ok) return Unauthorized("Credenciais inválidas.");

            // retorna o usuário (sem SenhaHash)
            return Ok(new UsuarioReadDto {
                Id = user.Id,
                Nome = user.Nome,
                Email = user.Email,

                // CORREÇÃO: Converte o TipoPerfil para string
                Perfil = user.Perfil.ToString(),

                ImagemPath = user.ImagemPath,
                Localizacao = user.Localizacao,
                Telefone = user.Telefone,
                LinkRedeSocial = user.LinkRedeSocial,
                DataNascimento = user.DataNascimento,

                // CORREÇÃO: Converte o GeneroEnum para string
                Genero = user.Genero,

                NomeEmpresa = user.NomeEmpresa,
                CNPJ = user.CNPJ,
                Peso = user.Peso,
                Altura = user.Altura,
                FaixaOrcamentoPatrocinio = user.FaixaOrcamentoPatrocinio,

                // CORREÇÃO: Usa a propriedade TimeClientAppId
                TimeClientAppId = (Guid)user.TimeClientAppId
            });
        }
    }
}