using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KategoriaController : ControllerBase
    {
        private static bool HeaderMatches(string headerValue, string currentEtag)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                return false;
            }

            var values = headerValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return values.Any(v => v == "*" || string.Equals(v, currentEtag, StringComparison.Ordinal));
        }

        private bool IfNoneMatchMatches(string currentEtag)
        {
            return HeaderMatches(Request.Headers[HeaderNames.IfNoneMatch].ToString(), currentEtag);
        }

        [HttpGet]
        public async Task<IActionResult> GetKategoria()
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var categories = await cx.Kategoria
                        .AsNoTracking()
                        .ToListAsync();

                    var dishes = await cx.Keszeteleks
                        .AsNoTracking()
                        .ToListAsync();

                    var response = categories.Select(c => new
                    {
                        c.Id,
                        c.Nev,
                        Kesziteleks = dishes.Where(d => d.KategoriaId == c.Id).Select(d => new
                        {
                            d.Id,
                            d.Nev,
                            d.Leiras,
                            d.Elerheto,
                            Kep = d.Kep != null && d.Kep.Length > 0 ? Program.ImageConvert(d.Kep) : null
                        }).ToList()
                    }).ToList();

                    var etag = Program.CreateWeakEtag(response);
                    Response.Headers[HeaderNames.ETag] = etag;

                    if (IfNoneMatchMatches(etag))
                    {
                        return StatusCode(StatusCodes.Status304NotModified);
                    }

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> getbyid(int id)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var categories = await cx.Kategoria
                        .AsNoTracking()
                        .FirstOrDefaultAsync(f => f.Id == id);

                    var dishes = await cx.Keszeteleks
                        .AsNoTracking()
                        .ToListAsync();

                    if (categories == null)
                    {
                        return BadRequest("nincs ilyen kategoria");
                    }

                    var response = new
                    {
                        categories.Id,
                        categories.Nev,
                        Kesziteleks = dishes.Where(d => d.KategoriaId == categories.Id).Select(d => new
                        {
                            d.Id,
                            d.Nev,
                            d.Leiras,
                            d.Elerheto,
                            Kep = d.Kep != null && d.Kep.Length > 0 ? Program.ImageConvert(d.Kep) : null
                        }).ToList()
                    };

                    var etag = Program.CreateWeakEtag(response);
                    Response.Headers[HeaderNames.ETag] = etag;

                    if (IfNoneMatchMatches(etag))
                    {
                        return StatusCode(StatusCodes.Status304NotModified);
                    }

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
        public IActionResult PostKategoria(string nev)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    if (cx.Kategoria.FirstOrDefault(f => f.Nev == nev) != null)
                    {
                        return Ok("Létezik ilyen Kategória!");
                    }
                    Kategoria kat = new Kategoria();
                    kat.Nev = nev;
                    cx.Kategoria.Add(kat);
                    cx.SaveChanges();
                    return Ok("Sikeres kategória  Mentés");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex);
                }
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Putkategoria(int id, string? nev)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var kat = await cx.Kategoria.FirstOrDefaultAsync(k => k.Id == id);
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

                [Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKategoria(int id)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    Kategoria kat = new Kategoria { Id = id };
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
