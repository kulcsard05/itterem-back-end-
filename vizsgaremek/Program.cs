using System.Security.Cryptography;
using System.Text;
using vizsgaremek.Modells;
using Microsoft.EntityFrameworkCore;

namespace vizsgaremek
{
    public class Program
    {

        public static Dictionary<string, Users> LoggedUsers = new Dictionary<string, Users>();

        const int SaltLength = 64;

        public static string GenerateSalt()
        {
            Random random = new Random();
            string karakterek = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string salt = "";
            for (int i = 0; i < SaltLength; i++)
            {
                salt += karakterek[random.Next(karakterek.Length)];
            }
            return salt;
        }
        public static string CreateSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Dev-only CORS (e.g. React dev server)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DevCors", policy =>
                {
                    // Prefer a specific origin for dev.
                    policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                    // If you truly need "AllowAnyOrigin", replace the above with:
                    // policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseCors("DevCors");
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // Teszt: Kapcsolatok ellenőrzése
            using (var context = new BackEndAlapContext())
            {
                Console.WriteLine("=== JOGOSULTSÁG ELLENŐRZÉSE ===");

                // Ellenőrizzük, hogy létezik-e a jogosultság
                var existingRole = context.Jogoks.FirstOrDefault(j => j.Szint == 1);
                if (existingRole == null)
                {
                    Console.WriteLine("A jogosultság nem létezik, létrehozás...");
                    var newRole = new Jogok
                    {
                        Szint = 1,
                        Nev = "Felhasználó",
                        Leiras = "Alap jogosultság"
                    };
                    context.Jogoks.Add(newRole);
                    context.SaveChanges();
                    Console.WriteLine("Jogosultság sikeresen létrehozva!");
                }
                else
                {
                    Console.WriteLine("A jogosultság már létezik.");
                }

                Console.WriteLine("=== ÚJ FELHASZNÁLÓ HOZZÁADÁSA ===");

                // Ellenőrizzük, hogy létezik-e már a felhasználó
                var email = "teszt1@teszt.hu";
                var existingUser = context.Users.FirstOrDefault(u => u.Email == email);
                if (existingUser != null)
                {
                    Console.WriteLine("A felhasználó már létezik az adatbázisban.");
                }
                else
                {
                    // Jelszó hash-elése és salt generálása
                    string salt = Program.GenerateSalt();
                    string hashedPassword = Program.CreateSHA256("teszt1");

                    // Új felhasználó létrehozása
                    var newUser = new Users
                    {
                        TeljesNev = "Teszt Felhasználó",
                        Email = email,
                        TelefonSzam = "+36 30 1234567",
                        Hash = hashedPassword,
                        Salt = salt,
                        Aktiv = 1, // Aktív státusz
                        Jogosultsag = 1 // Jogosultság szint
                    };

                    // Felhasználó hozzáadása az adatbázishoz
                    context.Users.Add(newUser);
                    context.SaveChanges();

                    Console.WriteLine("Új felhasználó sikeresen létrehozva!");
                    Console.WriteLine($"Név: {newUser.TeljesNev}");
                    Console.WriteLine($"Email: {newUser.Email}");
                    Console.WriteLine($"Telefon: {newUser.TelefonSzam}");
                    Console.WriteLine($"Jogosultság szint: {newUser.Jogosultsag}");
                    Console.WriteLine($"Aktivitás: {newUser.Aktiv}");
                    Console.WriteLine($"{hashedPassword}");
                }

                Console.WriteLine("=== MENÜK, FELHASZNÁLÓK ÉS ÉTELEK LEKÉRDEZÉSE ===");

                // Lekérdezés a Menuk, Users és Keszetelek táblákból
                var menus = context.Menuks
                    .Include(m => m.Keszetelek) // Menü kapcsolódó ételek
                    .Include(m => m.Koretek)    // Menü kapcsolódó köretek
                    .Include(m => m.Uditok)    // Menü kapcsolódó üdítők
                    .ToList();

                var users = context.Users.ToList(); // Felhasználók lekérdezése

                // Kiírás a konzolra
                foreach (var menu in menus)
                {
                    Console.WriteLine($"Menü: {menu.Menu_Nev}");
                    Console.WriteLine($"  - Készétel: {menu.Keszetelek?.Nev ?? "Nincs készétel"}");
                    Console.WriteLine($"  - Köret: {menu.Koretek?.Nev ?? "Nincs köret"}");
                    Console.WriteLine($"  - Üdítő: {menu.Uditok?.Nev ?? "Nincs üdítő"}");
                    Console.WriteLine();
                }

                Console.WriteLine("=== FELHASZNÁLÓK LISTÁJA ===");
                foreach (var user in users)
                {
                    Console.WriteLine($"Felhasználó: {user.TeljesNev}");
                    Console.WriteLine($"  - Email: {user.Email}");
                    Console.WriteLine($"  - Telefonszám: {user.TelefonSzam}");
                    Console.WriteLine($"  - Jogosultság szint: {user.Jogosultsag}");
                    Console.WriteLine();
                }
            }

            app.Run();
        }
    }
}
