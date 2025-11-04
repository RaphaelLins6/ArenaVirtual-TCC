using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.DTOs; 
using System.Linq;
using System.Collections.Generic; 

namespace ArenaVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimesController(ApiDbContext db) : ControllerBase {
    private readonly ApiDbContext _db = db;

    // GET: api/times
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimeReadDto>>> Get() {
        var items = await _db.Time
            .Include(t => t.Capitao)
            .Include(t => t.Membros)
            .AsNoTracking()
            .ToListAsync();

        return Ok(items.Select(MapToReadDto));
    }

    // GET: api/times/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TimeReadDto>> GetById(int id) {
        var item = await _db.Time
            .Include(t => t.Capitao)
            .Include(t => t.Membros)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return item is null ? NotFound() : Ok(MapToReadDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<TimeReadDto>> Create(Time time) {
        _db.Time.Add(time);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = time.Id }, MapToReadDto(time));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Time time) {
        if (id != time.Id) return BadRequest();
        _db.Entry(time).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) {
        var item = await _db.Time.FindAsync(id);
        if (item is null) return NotFound();
        _db.Time.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static IEnumerable<MembroReadDto> MapToMembroReadDto(Time t) {
        if (t.Membros == null) {
            return Enumerable.Empty<MembroReadDto>();
        }

        return t.Membros.Select(m => new MembroReadDto {
            ClientAppId = m.ClientAppId,
            Nome = m.Nome,
            ImagemPath = m.ImagemPath,
            IsCapitao = m.Id == t.CapitaoId
        });
    }

    private static TimeReadDto MapToReadDto(Time t) {
        var membrosDto = MapToMembroReadDto(t).ToList();
        return new() {
            Id = t.Id,
            ClientAppId = t.ClientAppId,
            Nome = t.Nome,
            LogoUrl = t.LogoUrl,
            Descricao = t.Descricao,
            DataCriacao = t.DataCriacao,
            CapitaoId = t.CapitaoId,
            CapitaoClientAppId = t.Capitao?.ClientAppId,
            QuantidadeMembros = membrosDto.Count,
            Membros = membrosDto
        };
    }
}