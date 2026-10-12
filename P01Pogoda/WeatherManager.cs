using System.Globalization;
using System.Text.RegularExpressions;

namespace P01Pogoda;

/// <summary>
/// Jednostki temperatury obsługiwane przez aplikację.
/// Enum zamiast "magicznych" napisów "C", "F", "K" – kompilator pilnuje poprawności.
/// </summary>
public enum TemperatureUnit
{
    Celsius,
    Fahrenheit,
    Kelvin
}

/// <summary>
/// Pobiera temperaturę z usługi wttr.in i przelicza ją na wybraną jednostkę.
/// </summary>
public class WeatherManager
{
    // Jeden HttpClient na całą aplikację – tworzenie nowego przy każdym zapytaniu
    // wyczerpuje gniazda sieciowe (socket exhaustion). WebClient jest przestarzały.
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    // Przykładowa odpowiedź wttr.in dla ?format=%t&m to np. "+12°C" albo "-3°C".
    // Wyrażenie regularne wyciąga samą liczbę (ze znakiem i częścią dziesiętną).
    // Grupa (…) o nazwie "value" przechowuje dopasowany fragment.
    private static readonly Regex _temperatureRegex =
        new(@"(?<value>[+-]?\d+(\.\d+)?)\s*°C", RegexOptions.Compiled);

    public async Task<double> GetTemperatureAsync(string city, TemperatureUnit unit)
    {
        // Uri.EscapeDataString zamienia np. spację na %20 – adres musi być poprawny
        string url = $"https://wttr.in/{Uri.EscapeDataString(city)}?format=%t&m";

        // GetStringAsync rzuci HttpRequestException przy błędzie sieci lub kodzie 4xx/5xx
        string response = await _httpClient.GetStringAsync(url);

        double celsius = ParseCelsius(response);
        return Convert(celsius, unit);
    }

    /// <summary>
    /// Zamienia tekst w stylu "+12°C" na liczbę 12.0.
    /// </summary>
    public static double ParseCelsius(string text)
    {
        Match match = _temperatureRegex.Match(text);

        if (!match.Success)
        {
            throw new FormatException($"Odpowiedź '{text.Trim()}' nie zawiera temperatury.");
        }

        // InvariantCulture: kropka jako separator dziesiętny niezależnie od ustawień Windows
        return double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Przelicza stopnie Celsjusza na wybraną jednostkę.
    /// Wyrażenie switch (C# 8+) zamiast łańcucha if/else.
    /// </summary>
    public static double Convert(double celsius, TemperatureUnit unit) => unit switch
    {
        TemperatureUnit.Celsius => celsius,
        TemperatureUnit.Fahrenheit => celsius * 1.8 + 32,
        TemperatureUnit.Kelvin => celsius + 273.15,
        _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Nieznana jednostka temperatury")
    };
}
