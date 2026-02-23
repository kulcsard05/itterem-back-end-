using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vizsgaremek;
using vizsgaremek.Modells;

namespace BackendAlap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Registration(User user)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    if (cx.Users.FirstOrDefault(f => f.Email == user.Email) != null)
                    {
                        return Ok("Már létezik ez az email cím!");
                    }
                    user.TeljesNev = user.TeljesNev;
                    user.Email = user.Email;
                    user.Hash = user.Hash;
                    user.Telefonszam = user.Telefonszam;
                    user.Jogosultsag = 1;
                    user.Aktiv = 2;
                    user.Salt = Program.GenerateSalt();
                    user.Hash = Program.CreatePasswordHash(user.Hash, user.Salt);
                    await cx.Users.AddAsync(user);
                    await cx.SaveChangesAsync();
                    return Ok("Sikeres regisztráció.");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
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
                        return BadRequest("Sikertelen regisztráció");
                    }
                    else
                    {
                        if (user.Aktiv != 2)
                        {
                            return Ok("A regisztráció már megtörtént!");
                        }
                        else
                        {
                            user.Aktiv = 1;
                            cx.Users.Update(user);
                            await cx.SaveChangesAsync();
                            return Ok("Sikeres fiók aktiválás.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

    }
}
