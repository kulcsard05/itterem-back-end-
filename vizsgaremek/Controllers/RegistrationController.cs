using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizsgaremek;
using vizsgaremek.DTOs;
using vizsgaremek.Modells;

namespace BackendAlap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Registration([FromBody] RegistrationDto dto)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    if (dto == null)
                    {
                        return StatusCode(500, "Hiányzó adatok.");
                    }

                    if (await cx.Users.AnyAsync(f => f.Email == dto.Email))
                    {
                        return StatusCode(409, "Már létezik ez az email cím!");
                    }

                    var salt = Program.GenerateSalt();
                    var hash = Program.CreatePasswordHash(dto.Jelszo, salt);

                    var user = new User
                    {
                        TeljesNev = dto.TeljesNev,
                        Email = dto.Email,
                        Telefonszam = dto.Telefonszam,
                        Jogosultsag = 1,
                        Aktiv = 2,
                        Salt = salt,
                        Hash = hash
                    };

                    await cx.Users.AddAsync(user);
                    await cx.SaveChangesAsync();
                    return StatusCode(200, "Sikeres regisztráció.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> ActivateAccount(string felhasznaloNev, string email)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    User? user = cx.Users.FirstOrDefault(f => f.Email == email);
                    if (user == null)
                    {
                        return StatusCode(404, "Sikertelen regisztráció");
                    }
                    else
                    {
                        if (user.Aktiv != 2)
                        {
                            return StatusCode(200, "A regisztráció már megtörtént!");
                        }
                        else
                        {
                            user.Aktiv = 1;
                            cx.Users.Update(user);
                            await cx.SaveChangesAsync();
                            return StatusCode(200, "Sikeres fiók aktiválás.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
        }
    }
}
