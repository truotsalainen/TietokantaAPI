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

        // Listaa kaikki varastot
        app.MapGet("/varastot", () =>
        {
            var varastot = Varasto.HaeVarastot();
            return Results.Ok(varastot);
        });

        // Luo uusi varasto
        app.MapPost("/varasto", (string nimi) =>
        {
            int id = Varasto.LuoVarasto(nimi);
            return Results.Ok(new { Id = id, Nimi = nimi });
        });

        // Poista varasto
        app.MapDelete("/varasto/{id}", (int id) =>
        {
            bool success = Varasto.PoistaVarasto(id);
            if (success)
                return Results.Ok($"Varasto {id} poistettu onnistuneesti.");
            else
                return Results.NotFound($"Varastoa {id} ei löytynyt.");
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
