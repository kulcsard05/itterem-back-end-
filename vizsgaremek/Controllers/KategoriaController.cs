using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KategoriaController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetKategoria()
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = await cx.Kategoras.Include(f => f.Keszeteleks).Select(f => new
                    {
                        f.Nev,
                        
                        Kesziteleks = f.Keszeteleks.Select(k => new
                        {
                            
                            k.Id,
                            k.Nev,
                            k.Leiras,
                            k.Elerheto,
                            Kep = k.Kep != null && k.Kep.Length > 0 ? Program.ImageConvert(k.Kep) : null
                        })
                    }).ToListAsync();
                    
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpPost]
        public IActionResult PostKategoria(string nev)
        {

            using (var cx = new BackEndAlapContext())
            {

                try
                {
                    if (cx.Kategoras.FirstOrDefault(f => f.Nev == nev) != null)
                    {
                        return Ok("Létezik ilyen Kategória!");
                    }
                    Kategora kat = new Kategora();
                    kat.Nev = nev;
                    cx.Kategoras.Add(kat);
                    cx.SaveChanges();
                    return Ok("Sikeres kategória  Mentés");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex);
                }
            }


        }
        [HttpPut]


        public async Task<IActionResult> PutKategoria(int id, string? nev)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var kat = await cx.Kategoras.FirstOrDefaultAsync(k => k.Id == id);
                    if (kat == null)
                    {
                        return NotFound("Nincs ilyen kategória!");
                    }

                    if (!string.IsNullOrWhiteSpace(nev))
                    {
                        kat.Nev = nev;
                    }

                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Kategória módosítás");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteKategoria(int id)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    Kategora kat = new Kategora { Id = id };
                    cx.Remove(kat);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres Kategória törlés.");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
