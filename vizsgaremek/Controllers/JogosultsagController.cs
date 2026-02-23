using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JogosultsagController : ControllerBase
    {
        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetJogosultsagok()
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = await cx.Jogoks.ToListAsync();
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [Authorize(Policy = "Admin")]
        [HttpPost]
        public IActionResult PostJogosultsag(int szint, string nev, string leiras)
        {

            using (var cx = new BackEndAlapContext())
            {

                try
                {
                    if (cx.Keszeteleks.FirstOrDefault(f => f.Nev == nev) != null)
                    {
                        return Ok("Létezik ilyen Jogosultág!");
                    }
                    Jogok jog = new Jogok();
                    jog.Szint = szint;
                    jog.Nev = nev;
                    jog.Leiras = leiras;

                    cx.Jogoks.Add(jog);
                    cx.SaveChanges();
                    return Ok("Sikeres Jogosultág Mentés");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex);
                }
            }


        }
        [Authorize(Policy = "Admin")]
        [HttpPut]


        public async Task<IActionResult> PutJogosultsag(int id, string? nev, string? leiras)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var Jog = await cx.Jogoks.FirstOrDefaultAsync(k => k.Id == id);
                    if (Jog == null)
                    {
                        return NotFound("Nincs ilyen Jogosultág!");
                    }

                    if (!string.IsNullOrWhiteSpace(nev))
                    {
                        Jog.Nev = nev;
                    }

                    if (!string.IsNullOrWhiteSpace(leiras))
                    {
                        Jog.Leiras = leiras;
                    }

                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Jogosultág módosítás");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteJogosultsag(int id)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    Jogok jog = new Jogok { Id = id };
                    cx.Remove(jog);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Jogosultág törlés.");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}

