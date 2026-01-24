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
        public async Task<IActionResult> GetKeszetelek()
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = await cx.Keszeteleks.ToListAsync();
                    return Ok(response);
                }
                catch(Exception ex)  
                {
                    return BadRequest(ex);
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostKeszetelek(Keszetelek keszetel)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    if (cx.Keszeteleks.FirstOrDefault(f => f.Nev == keszetel.Nev) != null)
                    {
                        return Ok("Létezik ilyen készétel!");
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
        public async Task<IActionResult> Put(int id, string? nev, string? leiras, int? elerheto)
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

                    if (elerheto.HasValue)
                    {
                        keszetel.Elerheto = elerheto.Value;
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
            using(var cx = new BackEndAlapContext())
            {
                try
                {
                    Keszetelek keszetel = new Keszetelek { Id = id };
                    cx.Remove(keszetel);
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
