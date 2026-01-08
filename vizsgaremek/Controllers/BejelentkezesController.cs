using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vizsgaremek.DTOs;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpGet("GetSalt")]

        public IActionResult GetSalt(string email)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = cx.Users.FirstOrDefault(f => f.Email == email);
                    if (response == null)
                    {
                        return NotFound("Nincs ilyen nevű felhasználó.");
                    }
                    return StatusCode(StatusCodes.Status200OK, response.Salt);
                }
                catch (Exception ex)
                {
                    return StatusCode(400, ex.Message);
                }
            }
        }

        [HttpPost]
        public IActionResult Login(LoginDTO login)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    // Lekérjük a felhasználót az email alapján
                    var response = cx.Users.FirstOrDefault(f => f.Email == login.email);
                    if (response == null)
                    {
                        return Ok("Hibás név vagy jelszó");
                    }

                    // Szerver oldali hash-elés a Salt használatával
                    string calculatedHash = Program.CreateSHA256(login.passwd + response.Salt);

                    // Összehasonlítjuk a hash-eket
                    if (response.Hash != calculatedHash)
                    {
                        return Ok("Hibás név vagy jelszó");
                    }

                    // Ellenőrizzük, hogy a felhasználó aktív-e
                    if (response.Aktiv == 0)
                    {
                        return Ok("Jelenleg nem aktív a státusza");
                    }

                    // Bejelentkezés sikeres, létrehozzuk a LoggedUser objektumot
                    LoggedUser loggeduser = new LoggedUser
                    {
                        TeljesNev = response.TeljesNev,
                        Email = response.Email,
                        Jogosultsag = response.Jogosultsag,
                        Token = Guid.NewGuid().ToString()
                    };

                    // Hozzáadjuk a felhasználót a bejelentkezett felhasználók listájához
                    lock (Program.LoggedUsers)
                    {
                        Program.LoggedUsers.Add(loggeduser.Token, response);
                    }

                    return Ok(loggeduser);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
