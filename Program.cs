using TietokantaViikko;

namespace TietokantaAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");
        app.MapGet("/tuote", () => Varasto.ListaaTuotteet());

        app.Run();
    }
}
