using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Policy name: "level1"
    // Only users with claim "jogosultsag" == "2" are allowed.
    options.AddPolicy("level1", policy =>
        policy.RequireClaim("jogosultsag", "2"));

    // Policy name: "Level1Or2"
    // Custom check: allow users whose "jogosultsag" is "1" OR "2".
    options.AddPolicy("Level1Or2", policy =>
        policy.RequireAssertion(ctx =>
        {
            var lvl = ctx.User.FindFirst("jogosultsag")?.Value;
            return lvl == "1" || lvl == "2";
        }));
});

// Register Swagger endpoints
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger UI + JWT "Authorize" button
builder.Services.AddSwaggerGen(c =>
{
    // Basic Swagger doc metadata
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "vizsgaremek API",
        Version = "v1"
    });

    // Define JWT Bearer scheme for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",                 // Header name
        Type = SecuritySchemeType.Http,         // HTTP auth
        Scheme = "Bearer",                      // "Bearer" scheme
        BearerFormat = "JWT",                   // Token format
        In = ParameterLocation.Header,          // Sent in headers
        Description = "Enter: Bearer {your JWT}"
    });

    // Require the JWT scheme globally in Swagger
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Later in pipeline:
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();