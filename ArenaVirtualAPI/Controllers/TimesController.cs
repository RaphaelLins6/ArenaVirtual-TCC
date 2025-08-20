using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArenaVirtualAPI.Models;
using ArenaVirtualAPI.Data;

namespace ArenaVirtualAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimesController(AppDbContext db) : ControllerBase {
    private readonly AppDbContext _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Time>>> Get() =>
        await _db.Times.Include(t => t.Membros).AsNoTracking().ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Time>> GetById(int id) {
        var item = await _db.Times.Include(t => t.Membros).AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Time>> Create(Time time) {
        _db.Times.Add(time);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = time.Id }, time);
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
        var item = await _db.Times.FindAsync(id);
        if (item is null) return NotFound();
        _db.Times.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}