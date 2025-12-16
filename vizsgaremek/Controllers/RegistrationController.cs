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
        public async Task<IActionResult> Registration(Users user)
        {
            using (var cx = new BackEndAlapContext())
            {
                try
                {
                    // Ellenőrizzük, hogy létezik-e már a felhasználó teljes név alapján
                    if (cx.Users.FirstOrDefault(f => f.TeljesNev == user.TeljesNev) != null)
                    {
                        return Ok("MÁR VAN ILYEN FELHASZNÁLÓ");
                    }

                    // Ellenőrizzük, hogy az email már regisztrálva van-e
                    if (cx.Users.FirstOrDefault(f => f.Email == user.Email) != null)
                    {
                        return Ok("EZT AZ EMAIL MÁR REGISZTRÁLTÁK");
                    }

                    // Ellenőrizzük, hogy a telefonszám már regisztrálva van-e
                    if (cx.Users.FirstOrDefault(f => f.TelefonSzam == user.TelefonSzam) != null)
                    {
                        return Ok("EZT A TELEFONSZÁMOT MÁR REGISZTRÁLTÁK");
                    }

                    // Alapértelmezett értékek beállítása
                    user.Jogosultsag = 1; // Alap jogosultság
                    user.Aktiv = 2; // Inaktív státusz
                    user.Hash = Program.CreateSHA256(user.Hash); // Jelszó hash-elése

                    // Felhasználó hozzáadása az adatbázishoz
                    await cx.Users.AddAsync(user);
                    await cx.SaveChangesAsync();

                    return Ok("Sikeres regisztráció");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
