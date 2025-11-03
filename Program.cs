using System.Data.Common;
using TietokantaViikko;

namespace TietokantaAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        var Varasto = new VarastoDB();

        // Listaa kaikki varastot   GET http://localhost:5000/varastot
        app.MapGet("/varastot", () =>
        {
            var varastot = Varasto.HaeVarastot();
            return Results.Ok(varastot);
        });

        // Luo uusi varasto ja aseta se aktiiviseksi POST http://localhost:5000/varasto BODY: JSON
        app.MapPost("/varasto", (string nimi) =>
        {
            int id = Varasto.LuoVarasto(nimi);
            return Results.Ok(new { Id = id, Nimi = nimi });
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

         // Vaihda aktiivista varastoa (valinnainen endpoint)
        app.MapPut("/varasto/aktiivinen/{id}", (int id) =>
        {
            Varasto.AsetaAktiivinenVarasto(id);
            return Results.Ok($"Varasto {id} asetettu aktiiviseksi.");
        });


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

        app.MapGet("/etsituotteet", (string column, string value, VarastoDB db) =>
        {
            var results = db.EtsiTuotteet(column, value);
            return Results.Ok(results);
        });

        app.Run();
    }
}
