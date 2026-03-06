using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using vizsgaremek.Hubs;
using vizsgaremek.Modells;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace vizsgaremek
{

    public class Program
    {
        public static Dictionary<string, User> LoggedUsers = new Dictionary<string, User>();
        const int SaltLength = 64;

        // --- SEGÉDFÜGGVÉNYEK ---
        public static string CreateWeakEtag<T>(T value)
        {
            var json = JsonSerializer.Serialize(value);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
            return $"W/\"{Convert.ToHexString(hash)}\"";
        }
        public static string GenerateSalt(int lengthInBytes = 16)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] saltBytes = new byte[lengthInBytes];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }
        public static string ImageConvert(byte[] image)
        {
            string imgBaseData = Convert.ToBase64String(image);
            return string.Format($"data:image/jpg;base64,{imgBaseData}");
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

        // --- MAIN ---

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();
            // 1. Szolgáltatások regisztrálása
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Add JWT token as: Bearer {token}"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services.AddResponseCaching();
            // CORS BEÁLLÍTÁS (Engedélyezi a Live Servert és a Reactot is)
            builder.Services.AddCors(options =>
            {
              options.AddPolicy("AllowAll", policy =>
               {
                policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                .AllowAnyHeader()
                   .AllowCredentials();
                });
                });
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var keyValue = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(keyValue))
            {
                throw new InvalidOperationException("Jwt:Key is missing from configuration.");
            }
            var key = Convert.FromBase64String(keyValue);

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };


                    //-----------------------
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/OrderHub", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                    //-----------------------
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Felhasznalo", policy =>
                    policy.RequireClaim("jogosultsag", "1"));

                options.AddPolicy("Admin_Dolgozo", policy =>
                    policy.RequireAssertion(ctx =>
                    {
                        var lvl = ctx.User.FindFirst("jogosultsag")?.Value;
                        return lvl == "2" || lvl == "3";
                    }));
                options.AddPolicy("Admin_Felhasznalo", policy =>
                    policy.RequireAssertion(ctx =>
                    {
                        var lvl = ctx.User.FindFirst("jogosultsag")?.Value;
                        return lvl == "1" || lvl == "3";
                    }));
                options.AddPolicy("Mindenki", policy =>
                    policy.RequireAssertion(ctx =>
                    {
                        var lvl = ctx.User.FindFirst("jogosultsag")?.Value;
                        return lvl == "1" || lvl == "2" || lvl == "3";
                    }));
                options.AddPolicy("Admin", policy =>
                policy.RequireClaim("jogosultsag", "3"));


            });

            var app = builder.Build();

            // 2. Middleware csõvezeték (A SORREND FONTOS)

            // CORS-nak az elején kell lennie
            app.UseCors("AllowAll");

    if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
       app.UseSwaggerUI();
         }

            app.UseHttpsRedirection();
            app.UseAuthentication();
         app.UseAuthorization();
            app.MapControllers();
       app.MapHub<OrderHub>("/OrderHub");
            app.Use(async (c, n) => {
                var s = Stopwatch.StartNew();
                await n();
                Console.WriteLine($"{c.Request.Method} | {c.Request.Path} | {s.ElapsedMilliseconds}ms");
            });
            Env.Load();
            app.UseResponseCaching();
            app.Run();
        }
    }
}