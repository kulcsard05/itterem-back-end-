using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Macs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using vizsgaremek.DTOs;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        
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
        [Authorize(Policy = "Admin_Felhasznalo")]
        [HttpPut]
        public IActionResult Putuser(int id, string? nev, string? email, string? telefonszam)
        {
            try
            {
                // SECURITY NOTE:
                // This endpoint modifies user data. We must ensure that a normal user can't update someone else's record
                // by simply passing a different `id`.
                //
                // We do this by comparing:
                // - the target user id coming from the request (`id` parameter)
                // - the authenticated user's id stored inside the JWT token (`sub` claim)
                //
                // Only admins are allowed to modify any user. Non-admin users can only modify themselves.

                // `jogosultsag` is a custom claim you add to the JWT at login time. In your app:
                // - "3" means Admin
                // - other values are non-admin
                var lvl = User.FindFirst("jogosultsag")?.Value;

                // `sub` (subject) is a standard JWT claim. You put `response.Id` into it when generating the token.
                // So it uniquely identifies the logged-in user.
                // NOTE: depending on JWT handler settings, the `sub` claim can be mapped to ClaimTypes.NameIdentifier.
                // For safety we check both.
                var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Admin can edit any user record.
                var isAdmin = lvl == "3";

                // If the caller is NOT admin, enforce "ownership": token user id must match the requested id.
                if (!isAdmin)
                {
                    // If token doesn't contain a usable user id, treat this as not authenticated properly.
                    if (!int.TryParse(sub, out var tokenUserId))
                    {
                        return Unauthorized();
                    }

                    // If `sub` does not match the requested `id`, block the request.
                    if (tokenUserId != id)
                    {
                        //403 Forbidden: authenticated, but not allowed to do this action.
                        return Forbid();
                    }
                }

                using (var cx = new BackEndAlapContext())
                {
                    var users = cx.Users.FirstOrDefault(k => k.Id == id);
                    if (users == null) return BadRequest("Nincs ilyen felhasználó");
                    if (!string.IsNullOrEmpty(nev)) users.TeljesNev = nev;
                    if (email != null) users.Email = email;
                    if (!string.IsNullOrEmpty(telefonszam)) users.Telefonszam = telefonszam;

                    // Persist the changes to the database
                    cx.SaveChanges();

                    return Ok(users);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
