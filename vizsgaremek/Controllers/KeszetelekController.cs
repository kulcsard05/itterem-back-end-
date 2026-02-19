using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices.Marshalling;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeszetelekController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Getkeszeteleks()
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = await cx.Keszeteleks.Include(f => f.Kategoria).ToListAsync();

                    var result = response.Select(k => new
                    {
                        k.Id,
                        k.Nev,
                        k.Leiras,
                        k.Elerheto,
                        k.Ar,
                        k.KategoriaId,
                        // Ha van Kep mező a Koretek táblában
                        Kep = k.Kep != null && k.Kep.Length > 0
                            ? Program.ImageConvert(k.Kep)
                            : null
                    }).ToList();

                    return Ok(result);
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
                    var response = await cx.Keszeteleks.FirstOrDefaultAsync(f=>f.Id == id);
                    if (response == null) return BadRequest("nincs ilyen ");
                    return Ok(response);
                }
            }
            catch (Exception ex )
            {

                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostKeszetelek(string nev, string leiras, int ar, int? elerheto, int katid, IFormFile? kep)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    if (cx.Keszeteleks.FirstOrDefault(f => f.Nev == nev) != null)
                    {
                        return Ok("Létezik ilyen készétel!");
                    }

                    Keszetelek keszetel = new Keszetelek();
                    keszetel.Nev = nev;
                    keszetel.Leiras = leiras;
                    keszetel.Ar = ar;
                    keszetel.Elerheto = elerheto ?? 0;
                    keszetel.KategoriaId = katid;

                    // Kép feldolgozása, ha van
                    if (kep != null && kep.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await kep.CopyToAsync(memoryStream);
                            keszetel.Kep = memoryStream.ToArray();
                        }
                    }

                    await cx.Keszeteleks.AddAsync(keszetel);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres készétel Mentés");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex);
                }
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutKeszetelek(int id, string? nev, string? leiras, int? ar, int? elerheto, int? Kategora, IFormFile? kep)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var keszetel = await cx.Keszeteleks.FirstOrDefaultAsync(k => k.Id == id);
                    if (keszetel == null)
                    {
                        return NotFound("Nincs ilyen készétel!");
                    }

                    if (!string.IsNullOrWhiteSpace(nev))
                    {
                        keszetel.Nev = nev;
                    }

                    if (!string.IsNullOrWhiteSpace(leiras))
                    {
                        keszetel.Leiras = leiras;
                    }

                    if (ar.HasValue)
                    {
                        keszetel.Ar = ar.Value;
                    }

                    if (elerheto.HasValue)
                    {
                        keszetel.Elerheto = elerheto.Value;
                    }
                    if (Kategora.HasValue)
                    {
                        keszetel.KategoriaId = Kategora.Value;
                    }
                    // Kép feldolgozása, ha van
                    if (kep != null && kep.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await kep.CopyToAsync(memoryStream);
                            keszetel.Kep = memoryStream.ToArray();
                        }
                    }

                    await cx.SaveChangesAsync();
                    return Ok("Sikeres készétel módosítás");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKEszetel(int id)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    // 1) delete dependent Menuk rows (if any)
                    var menuk = await cx.Menuks
                        .Where(m => m.KeszetelId == id)
                        .ToListAsync();
                    cx.Menuks.RemoveRange(menuk);

                    // 2) delete join table rows
                    var kapcs = await cx.KeszetelHozzavalokKapcsolos
                        .Where(x => x.KeszetelId == id)
                        .ToListAsync();
                    cx.KeszetelHozzavalokKapcsolos.RemoveRange(kapcs);

                    // 3) delete the Keszetelek row
                    var keszetel = await cx.Keszeteleks.FirstOrDefaultAsync(k => k.Id == id);
                    if (keszetel == null)
                        return NotFound("Nincs ilyen készétel!");

                    cx.Keszeteleks.Remove(keszetel);

                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Készétel törlés.");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
