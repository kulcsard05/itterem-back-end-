using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vizsgaremek.DTOs;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpGet("GetSalt")]

        public IActionResult GetSalt(string nev)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    var response = cx.Users.FirstOrDefault(f => f.TeljesNev == nev);
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

                    var jwt = HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                        .GetSection("Jwt");

                    var key = new SymmetricSecurityKey(Convert.FromBase64String(jwt["Key"]!));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, response.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Email, response.Email),
    new Claim("jogosultsag", response.Jogosultsag.ToString()),
    new Claim("nev", response.TeljesNev)
};

                    var token = new JwtSecurityToken(
                        issuer: jwt["Issuer"],
                        audience: jwt["Audience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!)),
                        signingCredentials: creds);

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    // Bejelentkezés sikeres, létrehozzuk a LoggedUser objektumot
                    LoggedUser loggeduser = new LoggedUser
                    {
                        TeljesNev = response.TeljesNev,
                        Email = response.Email,
                        Jogosultsag = response.Jogosultsag,
                        Telefonszam = response.Telefonszam,

                        Token = tokenString
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
        [HttpPut]
        public IActionResult Putuser(int id, string? nev, string? email, string? telefonszam)
        {
            try
            {
                using (var cx = new BackEndAlapContext())
                {
                    var users = cx.Users.FirstOrDefault(k => k.Id == id);
                    if (users == null) return BadRequest("Nincs ilyen felhasználó");
                    if (!string.IsNullOrEmpty(nev)) users.TeljesNev = nev;
                    if (email != null) users.Email = email;
                    if (!string.IsNullOrEmpty(telefonszam)) users.Telefonszam = telefonszam;
                    return (Ok(users));

                }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        
    }
}
