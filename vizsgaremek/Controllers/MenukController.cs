using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenukController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMenuks()
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = await cx.Menuks
                        .Include(m => m.Keszetel)
                        .Include(m => m.Koret)
                        .Include(m => m.Udito)
                        .Select(m => new
                        {
                            m.Id,
                            m.MenuNev,
                            m.Ar,
                            KeszetelId = m.Keszetel.Id,
                            KoretId = m.Koret.Id,
                            UditoId = m.Udito.Id,
                            m.Elerheto,
                            Kep = m.Kep != null && m.Kep.Length > 0 ? Program.ImageConvert(m.Kep) : null
                        })
                        .ToListAsync();

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var cx = new BackEndAlapContext())
                {
                    var response = await cx.Menuks
                        .Include(m => m.Keszetel)
                        .Include(m => m.Koret)
                        .Include(m => m.Udito)
                        .Where(m => m.Id == id)
                        .Select(m => new
                        {
                            m.Id,
                            m.MenuNev,
                            m.Ar,
                            Keszetelid = m.Keszetel.Id,
                            Koretid = m.Koret.Id,
                            Uditoid= m.Udito.Id,
                            m.Elerheto,
                            Kep = m.Kep != null && m.Kep.Length > 0 ? Program.ImageConvert(m.Kep) : null
                        })
                        .FirstOrDefaultAsync();

                    if (response == null)
                    {
                        return NotFound("Nincs ilyen Menü!");
                    }

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PostMenu(string menuNev, int ar, int? keszetelId, int? koretId, int? uditoId, int? elerheto, IFormFile? kep)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    if (await cx.Menuks.AnyAsync(m => m.MenuNev == menuNev))
                    {
                        return Ok("Létezik ilyen Menü!");
                    }

                    // Validate foreign keys
                    if (!await cx.Keszeteleks.AnyAsync(k => k.Id == keszetelId))
                    {
                        return BadRequest("Invalid KeszetelId");
                    }
                    if (koretId.HasValue && !await cx.Koreteks.AnyAsync(k => k.Id == koretId))
                    {
                        return BadRequest("Invalid KoretId");
                    }
                    if (!await cx.Uditoks.AnyAsync(u => u.Id == uditoId))
                    {
                        return BadRequest("Invalid UditoId");
                    }

                    Menuk menu = new Menuk();
                    menu.MenuNev = menuNev;
                    menu.Ar = ar;
                    menu.KeszetelId = keszetelId.Value;
                    if (koretId.HasValue)
                        menu.KoretId = koretId.Value;
                    menu.UditoId = uditoId;
                    menu.Elerheto = elerheto ?? 0;

                    if (kep != null && kep.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await kep.CopyToAsync(memoryStream);
                            menu.Kep = memoryStream.ToArray();
                        }
                    }

                    await cx.Menuks.AddAsync(menu);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Menü Mentés");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex);
                }
            }
        }
        [Authorize(Policy = "Admin_Dolgozo")]
        [HttpPut]
        public async Task<IActionResult> PutMenu(int id, string? menuNev, int? ar, int? keszetelId, int? koretId, int? uditoId, int? elerheto, IFormFile? kep)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var menu = await cx.Menuks.FirstOrDefaultAsync(m => m.Id == id);
                    if (menu == null)
                    {
                        return NotFound("Nincs ilyen Menü!");
                    }

                    if (!string.IsNullOrWhiteSpace(menuNev))
                    {
                        menu.MenuNev = menuNev;
                    }

                    if (ar.HasValue)
                    {
                        menu.Ar = ar.Value;
                    }

                    if (keszetelId.HasValue)
                    {
                        if (!await cx.Keszeteleks.AnyAsync(k => k.Id == keszetelId))
                        {
                            return BadRequest("Invalid KeszetelId");
                        }
                        menu.KeszetelId = keszetelId.Value;
                    }
                    if (koretId.HasValue)
                    {
                        if (!await cx.Koreteks.AnyAsync(k => k.Id == koretId))
                        {
                            return BadRequest("Invalid KoretId");
                        }
                        menu.KoretId = koretId.Value;
                    }
                    if (uditoId.HasValue)
                    {
                        if (!await cx.Uditoks.AnyAsync(u => u.Id == uditoId))
                        {
                            return BadRequest("Invalid UditoId");
                        }
                        menu.UditoId = uditoId.Value;
                    }
                    if (elerheto.HasValue)
                    {
                        menu.Elerheto = elerheto.Value;
                    }
                    if (kep != null && kep.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await kep.CopyToAsync(memoryStream);
                            menu.Kep = memoryStream.ToArray();
                        }
                    }

                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Menü módosítás");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    Menuk menu = new Menuk { Id = id };
                    cx.Remove(menu);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Menü törlés.");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
