using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using P03WeatherForecastWPF.Client.Models;
using P03WeatherForecastWPF.Client.Services;

namespace P03WeatherForecastWPF.Client;

/// <summary>
/// Wersja z dnia 11: logika w code-behind (obsługa zdarzeń w pliku okna).
/// Jutro (dzień 12) przeniesiemy ją do ViewModelu (wzorzec MVVM).
/// </summary>
public partial class MainWindow : Window
{
    private readonly OpenMeteoService _openMeteoService;

    public MainWindow()
    {
        InitializeComponent();

        // Konfiguracja z pliku appsettings.json (pakiet Microsoft.Extensions.Configuration.Json).
        // Plik ma ustawione "Copy to Output Directory", więc leży obok .exe.
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string language = configuration["AppSettings:DefaultLanguage"] ?? "en";

        _openMeteoService = new OpenMeteoService(new HttpClient(), language);
    }

    private async void btnSearch_Click(object sender, RoutedEventArgs e)
    {
        tbStatus.Text = string.Empty;
        btnSearch.IsEnabled = false;

        try
        {
            City[] cities = await _openMeteoService.GetLocationsAsync(txtCity.Text);
            lbCities.ItemsSource = cities;

            if (cities.Length == 0)
            {
                tbStatus.Text = "Nie znaleziono miasta o takiej nazwie.";
            }
        }
        catch (HttpRequestException ex)
        {
            tbStatus.Text = $"Błąd połączenia: {ex.Message}";
        }
        finally
        {
            btnSearch.IsEnabled = true;
        }
    }

    private async void lbCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (lbCities.SelectedItem is not City selectedCity)
        {
            return;
        }

        tbStatus.Text = string.Empty;

        try
        {
            Weather? weather = await _openMeteoService.GetCurrentWeatherAsync(selectedCity.Latitude, selectedCity.Longitude);

            lblCityName.Content = selectedCity.FullName;

            if (weather is null)
            {
                tbStatus.Text = "Brak danych pogodowych dla tej lokalizacji.";
                return;
            }

            lblTemperature.Content = $"{weather.Temperature:F1} °C";
            lblWind.Content = $"{weather.WindSpeed:F0} km/h";
            lblDescription.Content = weather.Description;
            lblTime.Content = weather.Time.ToString("g");
        }
        catch (HttpRequestException ex)
        {
            tbStatus.Text = $"Błąd połączenia: {ex.Message}";
        }
    }
}
