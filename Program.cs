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

        var app = builder.Build();

        var Varasto = new VarastoDB();

        // testi
        app.MapGet("/hello", () => 
        {
            return Results.Ok("Hello world");
        });

        // Listaa kaikki varastot   GET http://localhost:5000/varastot
        app.MapGet("/varastot", () =>
        {
            var varastot = Varasto.HaeVarastot();
            return Results.Ok(varastot);
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

        // Listaa tuotteet.
        app.MapGet("/tuote", () =>
        {
            return Results.Ok(Varasto.ListaaTuotteet());
        });

        // Lisää uusi tuote.
        app.MapPost("/tuote", (Tuote tuote) =>
        {
            Varasto.LisaaTuote(tuote.tag, tuote.nimi, tuote.maara, tuote.kunto);
            return Results.Ok($"Tuote '{tuote.nimi}' lisätty!");
        });

        // Etsii tuotteet.
        app.MapGet("/etsituotteet", (string column, string value) =>
        {
            var results = Varasto.EtsiTuotteet(column, value);
            return Results.Ok(results);
        });

        // Muokkaa tuotetta.

        app.MapPut("/tuote/{id}", (int id, Tuote muokattuTuote) =>
        {
            Varasto.MuokkaaTuote(id, muokattuTuote);
            return Results.Ok($"Tuote {id} päivitetty!");
        });


        app.Run();
    }
}
