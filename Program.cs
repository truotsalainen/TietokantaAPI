using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
namespace TietokantaAPI;

public class Program
{
    public record VarastoNimiRequest(string nimi);
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseUrls("http://0.0.0.0:5000");

        

        var Varasto = new VarastoDB();
        

        // Add CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()    // Allow any origin (development only)
                    .AllowAnyMethod()    // Allow GET, POST, etc.
                    .AllowAnyHeader();   // Allow headers like Content-Type
            });
        });

        var app = builder.Build();

        // Use the CORS policy
        app.UseCors("AllowAll");

        // testi
        app.MapGet("/hello", () => 
        {
            return Results.Ok("Hello world");
        });

        // Listaa kaikki varastot   GET http://localhost:5000/varastot
        app.MapGet("/varastot", () =>
        {
            try
            {
                var varastot = Varasto.HaeVarastot();
                var kaikkiTuotteet = Varasto.ListaaTuotteet();

                var varastotWithItems = varastot.Select(v => new
                {
                    id = v.Id,
                    nimi = v.Nimi,
                    items = kaikkiTuotteet
                            .Where(t => t.VarastoId == v.Id)
                            .Select(t => new
                            {
                                id = t.Id,
                                tag = t.Tag,
                                nimi = t.Nimi,
                                maara = t.Maara,
                                kunto = t.Kunto
                            }).ToList()
                }).ToList();

                return Results.Ok(varastotWithItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in /varastot: {ex}");
                return Results.Problem("Internal server error");
            }
        });

        // Luo uusi varasto ja aseta se aktiiviseksi POST http://localhost:5000/varasto BODY: JSON

        app.MapPost("/varasto", ([FromBody] VarastoNimiRequest request) =>
        {
            int id = Varasto.LuoVarasto(request.nimi);
            return Results.Ok(new { Id = id, Nimi = request.nimi });
        });



        // Poista varasto       DEL http://localhost:5000/varasto/3
        app.MapDelete("/varasto/{id}", (int id) =>
        {
            bool success = Varasto.PoistaVarasto(id);
            if (success)
                return Results.Ok($"Varasto {id} poistettu onnistuneesti.");
            else
                return Results.NotFound($"Varastoa {id} ei löytynyt.");
        });

        // Vaihda aktiivista varastoa  http://localhost:5000/varasto/aktiivinen/1
        app.MapPut("/varasto/aktiivinen/{id}", (int id) =>
        {
            Varasto.AsetaAktiivinenVarasto(id);
            return Results.Ok($"Varasto {id} asetettu aktiiviseksi.");
        });

        // Hae aktiivinen varasto http://localhost:5000/varasto/aktiivinen
        app.MapGet("/varasto/aktiivinen", () =>
        {
            var aktiivinen = Varasto.HaeAktiivinenVarasto();

            if (aktiivinen == null)
                return Results.NotFound("Ei aktiivista varastoa.");

            return Results.Ok(aktiivinen);
        });

        //muokkaa varaston nimeä
        app.MapPut("/varasto/{id}", (int id, VarastoTiedot updated) =>
        {
            return Varasto.MuokkaaVarastoNimi(id, updated.Nimi);
        });

        // Listaa tuotteet.
        app.MapGet("/tuote", () =>
        {
            return Results.Ok(Varasto.ListaaTuotteet());
        });

        // Lisää uusi tuote.
        app.MapPost("/tuote", (Tuote tuote) =>
        {
            Varasto.LisaaTuote(tuote.Tag, tuote.Nimi, tuote.Maara, tuote.Kunto);
            return Results.Ok($"Tuote '{tuote.Nimi}' lisätty!");
        });

        //Etsi tuotteet
        app.MapGet("/etsituotteet", (string column, string value) =>
        {
            var results = Varasto.EtsiTuotteet(column, value);
            return Results.Ok(results);
        });

        // Muokkaa tuotetta.

        app.MapPut("/tuote/{id}", (int id, Tuote muokattuTuote) =>
        {
            Varasto.MuokkaaTuote(id, muokattuTuote);
        });

        // Poista tuote DELETE http://localhost:5000/tuote/5
        app.MapDelete("/tuote/{id}", (int id) =>
        {
            bool success = Varasto.PoistaTuote(id);
            if (success)
                return Results.Ok($"Tuote {id} poistettu onnistuneesti.");
            else
                return Results.NotFound($"Tuotetta {id} ei löytynyt.");
        });

        // Hae varaston tuotteet GET /varasto/{id}/items
        app.MapGet("/varasto/{id}/items", (int id) =>
        {
            var kaikki = Varasto.ListaaTuotteet();
            var tuotteet = kaikki
                .Where(t => t.VarastoId == id)
                .Select(t => new
                {
                    id = t.Id,
                    tag = t.Tag,
                    nimi = t.Nimi,
                    maara = t.Maara,
                    kunto = t.Kunto
                })
                .ToList();

            return Results.Ok(tuotteet);
        });


        app.Run();
    }
}
