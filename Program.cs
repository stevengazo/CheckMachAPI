using CheckMachAPI.Data;
using CheckMachAPI.Services;
using CheckMachAPI.Settings;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext & Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.Services.AddScoped<FileManagerService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024 * 1024 * 200; // 200 MB
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT
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

#region Configuración de servicios personalizados

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


app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Files")),
    RequestPath = "/files"
});

// 🔹 Verificar si la base de datos existe y crearla si no existe
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Crea la base de datos si no existe (sin aplicar migraciones)
    // dbContext.Database.EnsureCreated();

    // 🔸 O mejor: aplica automáticamente las migraciones pendientes (recomendado)
    dbContext.Database.Migrate();
}


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
                        font-family: 'Segoe UI', Roboto, sans-serif;
                        background: linear-gradient(135deg, #1e90ff, #00bcd4);
                        color: #fff;
                        text-align: center;
                        padding-top: 10%;
                    }
                    h1 {
                        font-size: 2.5em;
                        margin-bottom: 20px;
                    }
                    button {
                        background-color: #fff;
                        color: #1e90ff;
                        border: none;
                        padding: 12px 25px;
                        border-radius: 8px;
                        font-size: 1.1em;
                        cursor: pointer;
                        transition: background-color 0.3s ease, transform 0.2s ease;
                    }
                    button:hover {
                        background-color: #f1f1f1;
                        transform: scale(1.05);
                    }
                </style>
            </head>
            <body>
                <h1>🚀 Bienvenido a <strong>CheckMach API</strong></h1>
                <button onclick=""window.location.href='/swagger'"">Ir a Swagger UI</button>
            </body>
            </html>";

    await context.Response.WriteAsync(html);
});

app.Run();
