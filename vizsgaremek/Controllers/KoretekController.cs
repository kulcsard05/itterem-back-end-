using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Digests;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KoretekController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Getkoretek()
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = await cx.Koreteks.ToListAsync();

                    var result = response.Select(k => new
                    {
                        k.Id,
                        k.Nev,
                        k.Leiras,
                        k.Elerheto,
                        k.Ar,
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
                using (var cx = new BackEndAlapContext()) {
                    var response = await cx.Koreteks.FirstOrDefaultAsync(f=>f.Id == id);
                    if (response == null) return BadRequest("nincs ilyen");
                    return Ok(response);
            }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Postkoret(string nev, string leiras, int ar, int? elerheto, IFormFile kep)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    if (await cx.Koreteks.AnyAsync(f => f.Nev == nev))
                    {
                        return Ok("Létezik ilyen Köret!");
                    }

                    Koretek koret = new Koretek();

                    if (kep != null && kep.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await kep.CopyToAsync(memoryStream);
                            koret.Kep = memoryStream.ToArray();
                        }
                    }

                    koret.Nev = nev;
                    koret.Leiras = leiras;
                    koret.Ar = ar;
                    koret.Elerheto = elerheto ?? 0;

                    await cx.Koreteks.AddAsync(koret);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Köret mentés");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex);
                }
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutKoretek(int id, string? nev, string? leiras, int? ar, int? elerheto, IFormFile? kep)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var koret = await cx.Koreteks.FirstOrDefaultAsync(k => k.Id == id);
                    if (koret == null)
                    {
                        return NotFound("Nincs ilyen Köret!");
                    }

                    if (!string.IsNullOrWhiteSpace(nev))
                    {
                        koret.Nev = nev;
                    }
                    if (!string.IsNullOrWhiteSpace(leiras))
                    {
                        koret.Leiras = leiras;
                    }
                    if (ar.HasValue)
                    {
                        koret.Ar = ar.Value;
                    }
                    if (elerheto.HasValue)
                    {
                        koret.Elerheto = elerheto.Value;
                    }
                    if (kep != null && kep.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await kep.CopyToAsync(memoryStream);
                            koret.Kep = memoryStream.ToArray();
                        }
                    }

                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Köret módosítás");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKoret(int id)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    Koretek koret = new Koretek { Id = id };
                    cx.Remove(koret);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Köret törlés.");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
