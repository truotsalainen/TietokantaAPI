using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using TietokantaAPI;
using TietokantaAPI.Services;

// ========================================
// 📌 Program.cs - Sovelluksen käynnistys ja endpointit
// ========================================
// Tässä tiedostossa määritellään sovelluksen DI, JWT-asetukset,
// middlewaret sekä kaikki Minimal API -endpointit.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Rekisteröidään palvelut
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<JwtService>();

// ========================================
// 🛢️ TIETOKANNAN REKISTERÖINTI
// ========================================
builder.Services.AddSingleton(sp =>
{
    string dbPath = Path.Combine(AppContext.BaseDirectory, "varasto.db");
    return new VarastoDB(dbPath);
});

// ========================================
// 🔑 JWT-KONFIGURAATIO
// ========================================
var jwtKey = builder.Configuration["Jwt:Key"] ?? "b7f9a3c4d5e6f1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VarastoAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VarastoClient";

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        $"VIRHE: JWT key on liian lyhyt ({keyBytes.Length * 8} bittia). " +
        $"HS256 vaatii vähintään 256-bittisen avaimen (32 merkkiä UTF8 ASCII). " +
        $"Aseta 'Jwt:Key' appsettings.json:issa riittävän pitkäksi.");
}

// Konfiguroi autentikaatio ja autorisaatio
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Apumetodi userId:n hakemiseen tokenista
int GetUserId(HttpContext ctx)
{
    var userIdClaim = ctx.User.FindFirst("userId");
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
        throw new UnauthorizedAccessException("Käyttäjätunnusta ei löytynyt tokenista.");
    }
    return userId;
}

// ========================================
// 👤 KÄYTTÄJÄT (REGISTER & LOGIN)
// ========================================

app.MapPost("/register", (VarastoDB db, IAuthService authService, RegisterRequest req) =>
{
    var existing = db.GetUser(req.Username);
    if (existing is not null)
        return Results.Conflict("Käyttäjä on jo olemassa.");

    string hashedPassword = authService.HashPassword(req.Password);
    db.AddUser(req.Username, hashedPassword);
    return Results.Ok("Käyttäjä luotu.");
});

app.MapPost("/login", (VarastoDB db, IAuthService authService, JwtService jwtService, LoginRequest req) =>
{
    var user = db.GetUser(req.Username);
    if (user is null || !authService.VerifyPassword(req.Password, user.Value.PasswordHash))
        return Results.Unauthorized();

    var uv = user.Value;
    string token = jwtService.GenerateToken(uv.Id, uv.Username);
    return Results.Ok(new { token });
});

// ========================================
// 🏢 VARASTOT
// ========================================

app.MapGet("/varastot", [Authorize] (HttpContext ctx, VarastoDB db) =>
{
    int userId = GetUserId(ctx);
    return Results.Ok(db.GetVarastot(userId));
});

app.MapPost("/varastot", [Authorize] (HttpContext ctx, VarastoDB db, CreateVarastoRequest req) =>
{
    int userId = GetUserId(ctx);
    int id = db.LuoVarasto(req.Nimi, userId);
    return Results.Ok(new { Id = id, Nimi = req.Nimi });
});

app.MapDelete("/varastot/{id}", [Authorize] (HttpContext ctx, VarastoDB db, int id) =>
{
    int userId = GetUserId(ctx);
    bool ok = db.PoistaVarasto(id, userId);

    return ok 
        ? Results.Ok("Varasto poistettu.")
        : Results.NotFound("Varastoa ei löytynyt tai ei kuulu käyttäjälle.");
});

// ========================================
// 📦 TUOTTEET
// ========================================

app.MapGet("/varastot/{varastoId}/tuotteet", [Authorize] (HttpContext ctx, VarastoDB db, int varastoId) =>
{
    try
    {
        int userId = GetUserId(ctx);
        return Results.Ok(db.HaeTuotteet(varastoId, userId));
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/varastot/{varastoId}/tuotteet", [Authorize] (HttpContext ctx, VarastoDB db, int varastoId, Tuote tuote) =>
{
    try
    {
        int userId = GetUserId(ctx);
        db.LisaaTaiPaivitaTuote(varastoId, tuote, userId);
        return Results.Ok("Tuote lisätty tai päivitetty.");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

app.MapPut("/varastot/{varastoId}/tuotteet/{tuoteId}", [Authorize] (HttpContext ctx, VarastoDB db, int varastoId, int tuoteId, Tuote tuote) =>
{
    try
    {
        int userId = GetUserId(ctx);
        bool ok = db.MuokkaaTuote(tuoteId, varastoId, tuote, userId);

        return ok
            ? Results.Ok("Tuote päivitetty.")
            : Results.NotFound("Tuotetta ei löytynyt.");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

app.MapDelete("/varastot/{varastoId}/tuotteet/{tuoteId}", [Authorize] (HttpContext ctx, VarastoDB db, int varastoId, int tuoteId) =>
{
    try
    {
        int userId = GetUserId(ctx);
        bool ok = db.PoistaTuote(tuoteId, varastoId, userId);

        return ok
            ? Results.Ok("Tuote poistettu.")
            : Results.NotFound("Tuotetta ei löytynyt.");
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

app.MapDelete("/varastot/{varastoId}/tuotteet", [Authorize] (HttpContext ctx, VarastoDB db, int varastoId, string? tuotteenNimi) =>
{
    if (string.IsNullOrWhiteSpace(tuotteenNimi))
        return Results.BadRequest(new { message = "Tuotteen nimi puuttuu." });

    try
    {
        int userId = GetUserId(ctx);
        db.PoistaTuote(tuotteenNimi, varastoId, userId);
        return Results.Ok(new { message = $"Tuote nimeltä '{tuotteenNimi}' poistettu varastosta {varastoId}." });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapDelete("/admin/users/{userIdToDelete}", [Authorize] (HttpContext ctx, VarastoDB db, int userIdToDelete) =>
{
    try
    {
        int adminId = GetUserId(ctx);
        if (!db.IsUserAdmin(adminId))
            return Results.Forbid();

        if (adminId == userIdToDelete)
            return Results.BadRequest(new { message = "Et voi poistaa omaa tunnustasi." });

        bool deleted = db.DeleteUserById(userIdToDelete);
        return deleted
            ? Results.Ok(new { message = $"Käyttäjä {userIdToDelete} poistettu." })
            : Results.NotFound(new { message = "Käyttäjää ei löytynyt." });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();