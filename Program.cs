using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using TietokantaAPI;

// ========================================
// 🚀 API-KÄYNNISTYS
// ========================================

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------------------------------
// 🛢 VarastoDB
// ----------------------------------------
builder.Services.AddSingleton(sp =>
{
    // Luo tietokannan polku
    string dbPath = Path.Combine(AppContext.BaseDirectory, "varasto.db");
    // Huom: Käytetään nyt VarastoDB-luokkaa, joka on määritelty yllä
    return new VarastoDB(dbPath);
});

// ----------------------------------------
// 🔐 JWT-asetukset
// ----------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKey1234567890"; 
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VarastoAPI";

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// Validate key length for HS256: must be at least 256 bits (32 bytes)
if (keyBytes.Length < 32)
{
    // Provide a clear startup error explaining how to fix the configuration
    throw new InvalidOperationException($"JWT key is too short ({keyBytes.Length * 8} bits). " +
        "HS256 requires a key of at least 256 bits (32 characters when using UTF8 ASCII). " +
        "Set configuration 'Jwt:Key' to a sufficiently long secret (e.g. 32+ characters) in appsettings.json.");
}

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

// ----------------------------------------
// Swagger
// ----------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------
// Apumetodi userId:n hakemiseen tokenista
// ----------------------------------------
int GetUserId(HttpContext ctx)
{
    var userIdClaim = ctx.User.FindFirst("userId");
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
        // Jos userId-claim puuttuu tai on virheellinen (ei saisi tapahtua Authorize-attribuutilla),
        // heitetään poikkeus.
        throw new UnauthorizedAccessException("Käyttäjätunnusta ei löytynyt tokenista.");
    }
    return userId;
}

// ========================================
// 👤 KÄYTTÄJÄT (REGISTER & LOGIN)
// ========================================

app.MapPost("/register", (VarastoDB db, RegisterRequest req) =>
{
    var existing = db.GetUser(req.Username);
    if (existing is not null)
        return Results.Conflict("Käyttäjä on jo olemassa.");

    db.AddUser(req.Username, req.Password);
    return Results.Ok("Käyttäjä luotu.");
});

app.MapPost("/login", (VarastoDB db, LoginRequest req) =>
{
    var user = db.GetUser(req.Username);
    // Huom: Käytännössä tarkistettaisiin salasanahash
    if (user is null || user.Value.PasswordHash != req.Password)
        return Results.Unauthorized();

    var claims = new[]
    {
        new Claim("userId", user.Value.Id.ToString()),
        new Claim(ClaimTypes.Name, req.Username)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: null,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(6),
        signingCredentials: creds
    );

    string jwt = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new { token = jwt });
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

app.MapGet("/varastot/{varastoId}/tuotteet", 
[Authorize] (HttpContext ctx, VarastoDB db, int varastoId) =>
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

app.MapPost("/varastot/{varastoId}/tuotteet",
[Authorize] (HttpContext ctx, VarastoDB db, int varastoId, Tuote tuote) =>
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

app.MapPut("/varastot/{varastoId}/tuotteet/{tuoteId}",
[Authorize] (HttpContext ctx, VarastoDB db, int varastoId, int tuoteId, Tuote tuote) =>
{
    try
    {
        int userId = GetUserId(ctx);
        bool ok = db.MuokkaaTuote(tuoteId, varastoId, tuote, userId);

        //Etsi tuotteet
        app.MapGet("/etsituotteet", (string column, string value) =>
        {
            var results = Varasto.EtsiTuotteet(column, value);
            return Results.Ok(results);
        });

// ----------------------------------------
// 🗑️ 1. Poisto ID:n perusteella (suositeltu)
// ----------------------------------------
app.MapDelete("/varastot/{varastoId}/tuotteet/{tuoteId}",
[Authorize] (HttpContext ctx, VarastoDB db, int varastoId, int tuoteId) =>
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


// ----------------------------------------
// 🗑️ 2. Poisto NIMEN perusteella (Kuten pyysit: PoistaTuote(string nimi))
// ----------------------------------------
// Endpoint: DELETE /varastot/{varastoId}/tuotteet?nimi=esimerkkituote
app.MapDelete("/varastot/{varastoId}/tuotteet",
[Authorize] (HttpContext ctx, VarastoDB db, int varastoId, string? nimi) =>
{
    if (string.IsNullOrWhiteSpace(nimi))
    {
        return Results.BadRequest(new { message = "Tuotteen nimi puuttuu." });
    }
    
    try
    {
        int userId = GetUserId(ctx);
        
        // Kutsutaan metodia, joka käyttää tuotteen nimeä (string) poistoon
        db.PoistaTuote(nimi, varastoId, userId); 
        
        return Results.Ok(new { message = $"Tuote(et) nimeltä '{nimi}' poistettu varastosta {varastoId}." });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});


app.Run();