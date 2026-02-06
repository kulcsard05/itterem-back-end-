using System.Security.Cryptography;
using System.Text;
using vizsgaremek.Modells;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace vizsgaremek
{
    public class Program
    {

        public static Dictionary<string, User> LoggedUsers = new Dictionary<string, User>();

        const int SaltLength = 64;



        public static string GenerateSalt(int lengthInBytes = 16)
        {
            RNGCryptoServiceProvider asd = new RNGCryptoServiceProvider();
            byte[] saltBytes = new byte[lengthInBytes];
            asd.GetBytes(saltBytes);
            string saltString = Convert.ToBase64String(saltBytes);
            asd.Dispose();

            return saltString;
        }

        public static string ImageConvert(byte[] image)
        {
            string imgBaseData = Convert.ToBase64String(image);
            string imageURL = string.Format($"data:image/jpg;base64,{imgBaseData}");
            return imageURL;
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
        
        public static string CreatePasswordHash(string password, string salt)
        {
            return CreateSHA256($"{salt}:{password}");
        }

        private static bool TryCheckDatabaseConnection(out string? error)
        {
            try
            {
                using var context = new BackEndAlapContext();
                var canConnect = context.Database.CanConnect();
                if (!canConnect)
                {
                    error = "Cannot connect to the MySQL database (Database.CanConnect() returned false).";
                    return false;
                }

                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
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

           
                
            

            app.Run();
        }
    }
}
