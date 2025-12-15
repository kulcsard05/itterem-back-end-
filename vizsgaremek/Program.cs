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
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            //// Teszt: Kapcsolatok ellenõrzése
            //using (var context = new BackEndAlapContext())
            //{
            //    Console.WriteLine("=== ÚJ FELHASZNÁLÓ HOZZÁADÁSA ===");

            //    // Új felhasználó létrehozása
            //    string jelszo = "teszt123";
            //    string salt = GenerateSalt();
            //    string hash = CreateSHA256(jelszo + salt);

            //    var ujFelhasznalo = new Users
            //    {
            //        TeljesNev = "Kovács János2",
            //        Email = "kovacs.janos2@email.com",
            //        TelefonSzam = "+36 20 1234567",
            //        Jogosultsag = 0, // Felhasználó jogosultság (szint 0)
            //        Salt = salt,
            //        Hash = hash
            //    };

            //    // Hozzáadás az adatbázishoz
            //    context.Users.Add(ujFelhasznalo);
            //    context.SaveChanges();

            //    Console.WriteLine("Új felhasználó sikeresen hozzáadva!");
            //    Console.WriteLine($"ID: {ujFelhasznalo.Id}");
            //    Console.WriteLine($"Név: {ujFelhasznalo.TeljesNev}");
            //    Console.WriteLine($"Email: {ujFelhasznalo.Email}");
            //    Console.WriteLine($"Telefon: {ujFelhasznalo.TelefonSzam}");
            //    Console.WriteLine($"Jogosultság szint: {ujFelhasznalo.Jogosultsag}");
            //    Console.WriteLine($"Jogosultság szint: {ujFelhasznalo.Hash}");
            //    Console.WriteLine($"Jogosultság szint: {ujFelhasznalo.Salt}");
            //    Console.WriteLine();

            //    Console.WriteLine("=== ÖSSZES FELHASZNÁLÓ LISTÁZÁSA ===");

            //    var users = context.Users
            //        .Include(u => u.JogosultsagNavigation)
            //        .ToList();

            //    foreach (var user in users)
            //    {
            //        Console.WriteLine($"Felhasználó: {user.TeljesNev}");
            //        Console.WriteLine($"  - ID: {user.Id}");
            //        Console.WriteLine($"  - Email: {user.Email}");
            //        Console.WriteLine($"  - Telefonszám: {user.TelefonSzam}");
            //        Console.WriteLine($"  - Jogosultság: {user.JogosultsagNavigation?.Nev ?? "Nincs jogosultság"}");
            //        Console.WriteLine($"  - Jogosultság szint: {user.Jogosultsag}");
            //        Console.WriteLine();
            //    }

            //    Console.WriteLine("=== MENÜK TESZTELÉSE ===");

            //    // Menük lekérdezése kapcsolódó adatokkal
            //    var menuk = context.Menuks
            //        .Include(m => m.Keszetelek)
            //        .Include(m => m.Koretek)
            //        .Include(m => m.Uditok)
            //        .ToList();

            //    foreach (var menu in menuk)
            //    {
            //        Console.WriteLine($"Menü: {menu.Menu_Nev}");
            //        Console.WriteLine($"  - Készétel: {menu.Keszetelek.Nev}");
            //        Console.WriteLine($"  - Köret: {menu.Koretek.Nev}");
            //        Console.WriteLine($"  - Üdítõ: {menu.Uditok.Nev}");
            //        Console.WriteLine();
            //    }

            //    Console.WriteLine("=== KÉSZÉTELEK ÉS HOZZÁVALÓK TESZTELÉSE ===");

            //    // Készételek lekérdezése hozzávalókkal
            //    var keszetelek = context.Keszeteleks.Include(k => k.Hozzavaloks).ToList();
            //    foreach (var keszetel in keszetelek)
            //    {
            //        Console.WriteLine($"Készétel: {keszetel.Nev}");
            //        foreach (var hozzavalo in keszetel.Hozzavaloks)
            //        {
            //            Console.WriteLine($"  - Hozzávaló: {hozzavalo.Hozzavalo_Nev}");
            //        }
            //        Console.WriteLine();
            //    }
            //}

            app.Run();
        }
    }
}
