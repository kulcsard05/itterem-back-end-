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
                using (var cx =new BackEndAlapContext())
                {
                    var response = cx.Uditoks.ToList();
                    return Ok(response);
                }
            }
            catch(Exception ex) 
            { 
            return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public IActionResult PostUdito(string nev, int? elerheto)
        {
            try
            {
                using (var cx = new BackEndAlapContext())
                {
                    Uditok udito = new Uditok();
                    udito.Nev = nev;
                    udito.Elerheto= elerheto ?? 0;
                    cx.Uditoks.Update(udito);
                    cx.SaveChanges();
                    return Ok("sikeres udito mentes");
                }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        public  IActionResult PutUdito(int id, string? nev,int? elerheto)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var udito = cx.Uditoks.FirstOrDefault(k => k.Id == id);
                    if (udito == null)
                    {
                        return NotFound("Nincs ilyen készétel!");
                    }

                    if (!string.IsNullOrWhiteSpace(nev))
                    {
                        udito.Nev = nev;
                    }

                   

                    if (elerheto.HasValue)
                    {
                        udito.Elerheto = elerheto.Value;
                    }


                    cx.SaveChanges();
                    return Ok("Sikeres készétel módosítás");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete]
        public IActionResult DeleteUdito(int id)
        {
            try
            {
                using (var cx = new BackEndAlapContext())
                {
                    var res = cx.Uditoks.FirstOrDefault(f => f.Id == id);
                    if (res == null) return BadRequest("Nincs ilyen udito");
                    cx.Uditoks.Remove(res);
                    cx.SaveChanges();
                    return Ok("sikeres törlés");
                }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}
