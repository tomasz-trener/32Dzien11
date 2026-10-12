// P01Pogoda – aplikacja konsolowa (.NET 10, top-level statements)
// Cel: pobrać temperaturę dla miasta z usługi wttr.in, przetworzyć tekst
// (string + wyrażenie regularne) i przeliczyć jednostki.
//
// Uruchomienie z konsoli:  dotnet run --project P01Pogoda
// Zbudowanie:              dotnet build

using P01Pogoda;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== P01Pogoda – temperatura z wttr.in ===");

var manager = new WeatherManager();

while (true)
{
    Console.Write("\nPodaj nazwę miasta (Enter = koniec): ");
    string? city = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(city))
    {
        break;
    }

    try
    {
        // await w top-level statements działa, bo Program jest kompilowany jako async Task Main
        double celsius = await manager.GetTemperatureAsync(city, TemperatureUnit.Celsius);
        double fahrenheit = await manager.GetTemperatureAsync(city, TemperatureUnit.Fahrenheit);
        double kelvin = await manager.GetTemperatureAsync(city, TemperatureUnit.Kelvin);

        Console.WriteLine($"{city}: {celsius:F1} °C | {fahrenheit:F1} °F | {kelvin:F1} K");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Nie udało się połączyć z usługą pogodową: {ex.Message}");
    }
    catch (FormatException ex)
    {
        Console.WriteLine($"Nie udało się odczytać temperatury: {ex.Message}");
    }
}

Console.WriteLine("Do zobaczenia!");
