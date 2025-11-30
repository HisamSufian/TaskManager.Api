using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------
// Add Services
// ------------------------------------------

// Add DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(
        "Data Source=tasks.db",
        sqliteOptions => sqliteOptions.MigrationsAssembly("TaskManager.Api")
    );
});


// Add Controllers (REQUIRED)
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_A_SUPER_SECRET_KEY_12345"))
        };
    });

builder.Services.AddAuthorization();

// ------------------------------------------
var app = builder.Build();
// ------------------------------------------

// Enable Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply EF Core Migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


// Map Controllers (REQUIRED)
app.MapControllers();

app.Run();
