using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Data;
using ArenaVirtualAPI.Models;

namespace ArenaVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampeonatosController : ControllerBase {
    private readonly ApiDbContext _db;
    public CampeonatosController(ApiDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Campeonato>>> Get() =>
        await _db.Campeonatos.AsNoTracking().ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Campeonato>> GetById(int id) {
        var item = await _db.Campeonatos.FindAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Campeonato>> Create(Campeonato c) {
        _db.Campeonatos.Add(c);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, c);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Campeonato c) {
        if (id != c.Id) return BadRequest();
        _db.Entry(c).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) {
        var item = await _db.Campeonatos.FindAsync(id);
        if (item is null) return NotFound();
        _db.Campeonatos.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
