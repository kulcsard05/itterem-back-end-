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
        public async Task<IActionResult> Getkoretek()
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = await cx.Keszeteleks.ToListAsync();

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
        //Front end atalakitas
        /*
         function indexKepBeallit() {
        const fileInput = document.getElementById('indexKepGomb').files[0];
        const preview = document.getElementById('indexKep');
        let reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            indexKep = fileInput;
        }
        reader.readAsDataURL(fileInput);
    }
        */

        [HttpPost]
        public async Task<IActionResult> PostKeszetelek(string nev,string leiras, int? elerheto,int katid)
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
                    keszetel.Elerheto = elerheto ?? 0;
                    keszetel.KategoriaId = katid;
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
        public async Task<IActionResult> PutKezsetelek(int id, string? nev, string? leiras, int? elerheto,int? Kategora)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var keszetel = await cx.Keszeteleks.FirstOrDefaultAsync(k => k.Nev == nev);
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
                    if (Kategora.HasValue)
                    {
                        keszetel.KategoriaId = Kategora.Value;
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
