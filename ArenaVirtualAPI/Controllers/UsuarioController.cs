using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs;
using ArenaVirtualAPI.Data;
using System.Linq;

namespace ArenaVirtualAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController(ApiDbContext context) : ControllerBase {
        private readonly ApiDbContext _context = context;

        // GET: api/usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioReadDto>>> GetUsuarios() {
            var usuarios = await _context.Usuarios.AsNoTracking().ToListAsync();
            return Ok(usuarios.Select(MapToReadDto));
        }

        // GET: api/usuarios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioReadDto>> GetUsuario(int id) {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null) return NotFound();
            return Ok(MapToReadDto(u));
        }

        // POST: api/usuarios
        [HttpPost]
        public async Task<ActionResult<UsuarioReadDto>> PostUsuario([FromBody] UsuarioCreateDto dto) {
            if (await _context.Usuarios.AnyAsync(x => x.Email == dto.Email))
                return BadRequest("Email já cadastrado.");

            var entity = new Usuario {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha, workFactor: 12),
                Perfil = dto.Perfil,
                ImagemPath = dto.ImagemPath,
                Localizacao = dto.Localizacao,
                Telefone = dto.Telefone,
                LinkRedeSocial = dto.LinkRedeSocial,
                DataNascimento = dto.DataNascimento,
                Genero = dto.Genero,
                NomeEmpresa = dto.NomeEmpresa,
                CNPJ = dto.CNPJ,
                Peso = dto.Peso,
                Altura = dto.Altura,
                FaixaOrcamentoPatrocinio = dto.FaixaOrcamentoPatrocinio,
                TimeClientAppId = dto.TimeClientAppId // Corrigido para usar TimeClientAppId
            };

            _context.Usuarios.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = entity.Id }, MapToReadDto(entity));
        }

        // PUT: api/usuarios/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] UsuarioUpdateDto dto) {
            if (id != dto.Id) return BadRequest();

            var entity = await _context.Usuarios.FindAsync(id);
            if (entity == null) return NotFound();

            // Atualiza campos
            entity.Nome = dto.Nome;
            entity.Email = dto.Email;
            entity.Perfil = dto.Perfil;
            entity.ImagemPath = dto.ImagemPath;
            entity.Localizacao = dto.Localizacao;
            entity.Telefone = dto.Telefone;
            entity.LinkRedeSocial = dto.LinkRedeSocial;
            entity.DataNascimento = dto.DataNascimento;
            entity.Genero = dto.Genero;
            entity.NomeEmpresa = dto.NomeEmpresa;
            entity.CNPJ = dto.CNPJ;
            entity.Peso = dto.Peso;
            entity.Altura = dto.Altura;
            entity.FaixaOrcamentoPatrocinio = dto.FaixaOrcamentoPatrocinio;
            entity.TimeClientAppId = dto.TimeClientAppId; // Corrigido para usar TimeClientAppId

            // se o cliente mandou nova senha, re-hash
            if (!string.IsNullOrWhiteSpace(dto.NovaSenha)) {
                entity.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha, workFactor: 12);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/usuarios/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUsuario(int id) {
            var entity = await _context.Usuarios.FindAsync(id);
            if (entity == null) return NotFound();

            _context.Usuarios.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static UsuarioReadDto MapToReadDto(Usuario u) => new() {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            Perfil = u.Perfil.ToString(), // Converte Perfil para string
            ImagemPath = u.ImagemPath,
            Localizacao = u.Localizacao,
            Telefone = u.Telefone,
            LinkRedeSocial = u.LinkRedeSocial,
            DataNascimento = u.DataNascimento,
            Genero = u.Genero?.ToString(), // Converte Genero para string (com verificação de nulo)
            NomeEmpresa = u.NomeEmpresa,
            CNPJ = u.CNPJ,
            Peso = u.Peso,
            Altura = u.Altura,
            FaixaOrcamentoPatrocinio = u.FaixaOrcamentoPatrocinio,
            TimeClientAppId = u.TimeClientAppId, // Corrigido para usar TimeClientAppId
            ClientAppId = u.ClientAppId
        };
    }
}