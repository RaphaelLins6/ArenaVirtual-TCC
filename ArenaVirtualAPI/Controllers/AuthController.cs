using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Dtos;
using ArenaVirtualAPI.Data;

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

            var ok = BCrypt.Net.BCrypt.Verify(dto.Senha, user.SenhaHash);
            if (!ok) return Unauthorized("Credenciais inválidas.");

            // retorna o usuário (sem SenhaHash)
            return Ok(new UsuarioReadDto {
                Id = user.Id,
                Nome = user.Nome,
                Email = user.Email,
                Perfil = user.Perfil,
                ImagemPath = user.ImagemPath,
                Localizacao = user.Localizacao,
                Telefone = user.Telefone,
                LinkRedeSocial = user.LinkRedeSocial,
                DataNascimento = user.DataNascimento,
                Genero = user.Genero,
                NomeEmpresa = user.NomeEmpresa,
                CNPJ = user.CNPJ,
                Peso = user.Peso,
                Altura = user.Altura,
                FaixaOrcamentoPatrocinio = user.FaixaOrcamentoPatrocinio,
                TimeId = user.TimeId
            });
        }
    }
}
