using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JogosultsagController : ControllerBase
    {
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
        [HttpPost]
        public IActionResult PostJogosultsag(Jogok jog)
        {

            using (var cx = new BackEndAlapContext())
            {

                try
                {
                    if (cx.Keszeteleks.FirstOrDefault(f => f.Nev == jog.Nev) != null)
                    {
                        return Ok("Létezik ilyen készétel!");
                    }


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
        [HttpPut]


        public async Task<IActionResult> Put(int id, string? nev, string? leiras)
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

