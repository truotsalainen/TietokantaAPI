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
            return Varasto.ListaaTuotteet();
        });

        app.Run();
    }
}
