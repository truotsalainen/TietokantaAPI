using TietokantaViikko;

namespace TietokantaAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        var Varasto = new VarastoDB();

        app.MapGet("/", () => "Hello World!");
        app.MapGet("/tuote", () => 
        {
            return Results.Ok(Varasto.ListaaTuotteet());
        });

        app.Run();
    }
}
