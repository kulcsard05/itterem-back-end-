using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizsgaremek.DTOs;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class HozzavalokController : ControllerBase
    {
        
        [HttpGet]
        public async Task<IActionResult> GetHozzavalok()
        {
            
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = await cx.Hozzavaloks.ToListAsync();
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        //[Authorize(Policy = "level1")]
        [HttpPost]
        public IActionResult PostHozzavalok(string nev)
        {

            using (var cx = new BackEndAlapContext())
            {
                Hozzavalok hozzavalo = new Hozzavalok();


                try
                {
                    if (cx.Hozzavaloks.FirstOrDefault(f => f.HozzavaloNev == nev) != null)
                    {
                        return Ok("Létezik ilyen Hozzávaló!");
                    }
                    hozzavalo.HozzavaloNev = nev;

                    cx.Hozzavaloks.Add(hozzavalo);
                    cx.SaveChanges();
                    return Ok("Sikeres Hozzávaló Mentés");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex);
                }
            }


        }
        [HttpPut]


        public async Task<IActionResult> PutHozzavalok(int id, string? nev)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var hozzavalo = await cx.Hozzavaloks.FirstOrDefaultAsync(k => k.Id == id);
                    if (hozzavalo == null)
                    {
                        return NotFound("Nincs ilyen Hozzávaló!");
                    }

                    if (!string.IsNullOrWhiteSpace(nev))
                    {
                        hozzavalo.HozzavaloNev = nev;
                    }
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Hozzávaló módosítás");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteHozzavalok(int id)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    Hozzavalok hozzavalo = new Hozzavalok { Id = id };
                    cx.Remove(hozzavalo);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Hozzávaló törlés.");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
