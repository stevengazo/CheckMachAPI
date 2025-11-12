using CheckMachAPI.Data;
using CheckMachAPI.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext & Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    var html = @"<!DOCTYPE html>
        <html lang='es'>
        <head><meta charset='UTF-8'><title>CheckMach API</title></head>
        <body><h1>🚀 Bienvenido a CheckMach API</h1>
        <button onclick=""window.location.href='/swagger'"">Ir a Swagger UI</button>
        </body></html>";
    await context.Response.WriteAsync(html);
});

app.Run();
