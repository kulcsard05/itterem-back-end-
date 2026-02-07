using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RendelesekController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetRendelesek()
        {
            try
            {
                using (var cx = new BackEndAlapContext())
                {
                    var response = await cx.Rendeleseks
                        .Include(r => r.Felhasznalo)
                        .Include(r => r.RendelesElemeks)
                            .ThenInclude(e => e.Keszetel)
                        .Include(r => r.RendelesElemeks)
                            .ThenInclude(e => e.Udito)
                        .Include(r => r.RendelesElemeks)
                            .ThenInclude(e => e.Menu)
                        .Select(r => new
                        {
                            r.Id,
                            r.FelhasznaloId,
                            FelhasznaloNev = r.Felhasznalo.TeljesNev,
                            r.Datum,
                            r.Statusz,
                            RendelesElemeks = r.RendelesElemeks.Select(e => new
                            {
                                e.Id,
                                e.RendelesId,
                                KeszetelNev = e.Keszetel != null ? e.Keszetel.Nev : null,
                                UditoNev = e.Udito != null ? e.Udito.Nev : null,
                                MenuNev = e.Menu != null ? e.Menu.MenuNev : null,
                                e.Mennyiseg
                            }).ToList()
                        })
                        .ToListAsync();

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
