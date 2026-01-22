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

                    var computedHash = Program.CreateSHA256($"{response.Salt}:{login.passwd}");
                    if (response.Hash != computedHash)
                    { 
                        return Ok("Hibás név vagy jelszó"); 
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
