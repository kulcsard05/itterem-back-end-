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
                    return StatusCode(200, response);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
        }

        [Authorize(Policy = "Admin")]
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
                        return StatusCode(409, "Létezik ilyen Hozzávaló!");
                    }
                    hozzavalo.HozzavaloNev = nev;

                    cx.Hozzavaloks.Add(hozzavalo);
                    cx.SaveChanges();
                    return StatusCode(200, "Sikeres Hozzávaló Mentés");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }


        }
		[Authorize(Policy = "Admin")]
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
                        return StatusCode(404, "Nincs ilyen Hozzávaló!");
                    }

                    if (!string.IsNullOrWhiteSpace(nev))
                    {
                        hozzavalo.HozzavaloNev = nev;
                    }
                    await cx.SaveChangesAsync();
                    return StatusCode(200, "Sikeres Hozzávaló módosítás");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
        }
		[Authorize(Policy = "Admin")]
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
                    return StatusCode(200, "Sikeres Hozzávaló törlés.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
        }
    }
}
