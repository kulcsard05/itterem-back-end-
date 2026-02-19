using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UditokController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetUditok()
        {
            try
            {
                using (var cx = new BackEndAlapContext())
                {
                    var response = cx.Uditoks.ToList();

                    var result = response.Select(k => new
                    {
                        k.Id,
                        k.Nev,
                        k.Elerheto,
                        k.Ar,
                        Kep = k.Kep != null && k.Kep.Length > 0
                            ? Program.ImageConvert(k.Kep)
                            : null
                    }).ToList();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostUdito(string nev, int ar, int? elerheto, IFormFile kep)
        {
            try
            {
                using (var cx = new BackEndAlapContext())
                {
                    if (cx.Uditoks.FirstOrDefault(u => u.Nev == nev) != null)
                    {
                        return Ok("Létezik ilyen Üdítő!");
                    }

                    Uditok udito = new Uditok();
                    udito.Nev = nev;
                    udito.Ar = ar;
                    udito.Elerheto = elerheto ?? 0;

                    if (kep != null && kep.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            kep.CopyTo(memoryStream);
                            udito.Kep = memoryStream.ToArray();
                        }
                    }

                    await cx.Uditoks.AddAsync(udito);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres üdítő mentés");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public IActionResult PutUdito(int id, string? nev, int? ar, int? elerheto, IFormFile? kep)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var udito = cx.Uditoks.FirstOrDefault(k => k.Id == id);
                    if (udito == null)
                    {
                        return NotFound("Nincs ilyen üdítő!");
                    }

                    if (!string.IsNullOrWhiteSpace(nev))
                    {
                        udito.Nev = nev;
                    }

                    if (ar.HasValue)
                    {
                        udito.Ar = ar.Value;
                    }

                    if (elerheto.HasValue)
                    {
                        udito.Elerheto = elerheto.Value;
                    }

                    if (kep != null && kep.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            kep.CopyTo(memoryStream);
                            udito.Kep = memoryStream.ToArray();
                        }
                    }

                    cx.SaveChanges();
                    return Ok("Sikeres üdítő módosítás");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUdito(int id)
        {
            try
            {
                using (var cx = new BackEndAlapContext())
                {
                    var res = cx.Uditoks.FirstOrDefault(f => f.Id == id);
                    if (res == null) return BadRequest("Nincs ilyen üdítő");
                    cx.Uditoks.Remove(res);
                    cx.SaveChanges();
                    return Ok("Sikeres törlés");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using (var cx = new BackEndAlapContext())
                {
                    var udito = await cx.Uditoks.FirstOrDefaultAsync(u => u.Id == id);
                    if (udito == null)
                    {
                        return NotFound("Nincs ilyen üdítő!");
                    }

                    var result = new
                    {
                        udito.Id,
                        udito.Nev,
                        udito.Elerheto,
                        udito.Ar,
                        Kep = udito.Kep != null && udito.Kep.Length > 0
                            ? Program.ImageConvert(udito.Kep)
                            : null
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
