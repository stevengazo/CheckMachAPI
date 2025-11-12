using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CheckMachAPI.Data;
using CheckMachAPI.Settings;


var builder = WebApplication.CreateBuilder(args);

// DbContext & Identity
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

// JWT (opcional si ya lo configuras)
builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

#region  // Configuración de servicios personalizados


// Email 
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton(resolver =>
    new EmailSender(builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>()));

// Blob Storage Azure
builder.Services.Configure<BlobSettings>(builder.Configuration.GetSection("BlobSettings"));
builder.Services.AddSingleton(resolver =>
    new BlobService(builder.Configuration.GetSection("BlobSettings").Get<BlobSettings>()));

// Notification Hubs Azure
builder.Services.Configure<NotificationSettings>(
    builder.Configuration.GetSection("NotificationHub")
);

#endregion


// Controladores + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CheckMachAPI", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header usando Bearer. Ejemplo: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";

    var html = @"
            <!DOCTYPE html>
            <html lang='es'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>CheckMach API</title>
                <style>
                    body {
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        background: linear-gradient(135deg, #1e3c72, #2a5298);
                        color: white;
                        height: 100vh;
                        margin: 0;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        text-align: center;
                    }
                    .container {
                        background: rgba(255, 255, 255, 0.1);
                        padding: 40px;
                        border-radius: 15px;
                        box-shadow: 0 0 20px rgba(0, 0, 0, 0.3);
                        max-width: 400px;
                    }
                    h1 {
                        margin-bottom: 10px;
                        font-size: 2.2em;
                    }
                    p {
                        margin-bottom: 30px;
                        color: #e0e0e0;
                    }
                    button {
                        background-color: #00b4d8;
                        border: none;
                        color: white;
                        padding: 12px 25px;
                        font-size: 1em;
                        border-radius: 8px;
                        cursor: pointer;
                        transition: background 0.3s ease;
                    }
                    button:hover {
                        background-color: #0096c7;
                    }
                    footer {
                        margin-top: 20px;
                        font-size: 0.9em;
                        color: #b0c4de;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <h1>🚀 Bienvenido a CheckMach API</h1>
                    <p>API para gestión de mantenimiento, inventario y proyectos.</p>
                    <button onclick=""window.location.href='/swagger'"">Ir a Swagger UI</button>
                    <footer>© 2025 CheckMachAPI</footer>
                </div>
            </body>
            </html>";

    await context.Response.WriteAsync(html);
});

app.Run();
