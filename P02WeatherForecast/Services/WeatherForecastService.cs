using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace P02WeatherForecast.Services;

/// <summary>
/// Serwis pogodowy w trzech wariantach, żeby pokazać różnicę między
/// kodem synchronicznym a asynchronicznym w aplikacji z interfejsem graficznym.
/// </summary>
public class WeatherForecastService
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly Regex _temperatureRegex =
        new(@"(?<value>[+-]?\d+(\.\d+)?)\s*°C", RegexOptions.Compiled);

    private static string BuildUrl(string city) =>
        $"https://wttr.in/{Uri.EscapeDataString(city.Trim())}?format=%t&m";

    /// <summary>
    /// Wariant 1: SYNCHRONICZNY. Blokuje wątek, który go wywołał.
    /// W WPF oznacza to zamrożony interfejs do czasu odpowiedzi serwera.
    /// HttpClient.Send (bez Async) istnieje od .NET 5 właśnie do takich porównań.
    /// </summary>
    public double GetTemperature(string city)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(city));
        using HttpResponseMessage response = _httpClient.Send(request);
        response.EnsureSuccessStatusCode();

        using var reader = new StreamReader(response.Content.ReadAsStream());
        string text = reader.ReadToEnd();

        return ParseCelsius(text);
    }

    /// <summary>
    /// Wariant 2: ASYNCHRONICZNY "prawdziwy". Korzystamy z gotowej metody GetStringAsync,
    /// która nie blokuje żadnego wątku podczas czekania na sieć (I/O-bound).
    /// </summary>
    public async Task<double> GetTemperatureAsync(string city)
    {
        string text = await _httpClient.GetStringAsync(BuildUrl(city));
        return ParseCelsius(text);
    }

    /// <summary>
    /// Wariant 3: ASYNCHRONICZNY "opakowany". Gdy biblioteka nie ma metody async,
    /// uruchamiamy jej synchroniczną wersję w wątku z puli (Task.Run).
    /// Zajmuje wątek, ale nie wątek interfejsu – okno nadal reaguje.
    /// </summary>
    public Task<double> GetTemperatureWrappedAsync(string city)
    {
        return Task.Run(() => GetTemperature(city));
    }

    private static double ParseCelsius(string text)
    {
        Match match = _temperatureRegex.Match(text);
        if (!match.Success)
        {
            throw new FormatException($"Odpowiedź '{text.Trim()}' nie zawiera temperatury.");
        }

        return double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
    }
}
