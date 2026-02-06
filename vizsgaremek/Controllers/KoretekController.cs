using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [HttpPost]
        public async Task<IActionResult> Postkoret(string nev, string leiras, int? elerheto, IFormFile kep)
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
                        koret.Elerheto = elerheto ?? 0;


                    await cx.Koreteks.AddAsync(koret);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Köret Mentés");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex);
                }
            }


        }
        [HttpPut]


        public async Task<IActionResult> PutKoretek(int id, string? nev, string? leiras,int? elerheto, IFormFile?  kep)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var Koret = await cx.Koreteks.FirstOrDefaultAsync(k => k.Id == id);
                    if (Koret == null)
                    {
                        return NotFound("Nincs ilyen Köret!");
                    }

                    if (!string.IsNullOrWhiteSpace(nev))
                    {
                        Koret.Nev = nev;
                    }
                    if (!string.IsNullOrWhiteSpace(leiras))
                    {
                        Koret.Leiras = leiras;
                    }
                    if (elerheto.HasValue)
                    {
                        Koret.Elerheto = elerheto.Value;
                    }
                    if (kep != null && kep.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await kep.CopyToAsync(memoryStream);
                            Koret.Kep = memoryStream.ToArray();
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
