using System.Diagnostics;
using System.Windows;
using P02WeatherForecast.Services;

namespace P02WeatherForecast;

/// <summary>
/// Siedem sposobów pobrania temperatury dla listy miast – od blokującego
/// interfejs kodu synchronicznego do zalecanego async/await.
/// </summary>
public partial class MainWindow : Window
{
    private readonly WeatherForecastService _service = new();

    public MainWindow()
    {
        InitializeComponent();
        txtCities.Text = string.Join(Environment.NewLine, "Warszawa", "Kraków", "Gdańsk", "Berlin", "Paryż", "Tokio");
    }

    // ----- pomocnicze -----

    private string[] GetCities() =>
        txtCities.Text
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private void Reset()
    {
        tbResults.Text = string.Empty;
        lvLog.Items.Clear();
        pbProgress.Value = 0;
    }

    private void Log(string message) =>
        lvLog.Items.Add($"{DateTime.Now:HH:mm:ss.fff}  {message}");

    private void AppendResult(string city, double temperature) =>
        tbResults.Text += $"{city}: {temperature:F1} °C{Environment.NewLine}";

    // ----- menu -----

    private void MenuClear_Click(object sender, RoutedEventArgs e) => Reset();

    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

    private void MenuAbout_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show("Dzień 11 – kontenery, style i programowanie asynchroniczne w WPF.", "O programie");

    // ----- 1. SYNCHRONICZNIE -----
    // Wątek UI czeka na każdą odpowiedź serwera. Okno nie reaguje na kliknięcia,
    // nie da się go przesunąć, a Windows po chwili pokazuje "Brak odpowiedzi".
    private void btnSync_Click(object sender, RoutedEventArgs e)
    {
        Reset();
        var stopwatch = Stopwatch.StartNew();

        foreach (string city in GetCities())
        {
            Log($"Pobieram {city}...");
            double temperature = _service.GetTemperature(city);   // blokuje wątek UI
            AppendResult(city, temperature);
        }

        Log($"Gotowe w {stopwatch.ElapsedMilliseconds} ms");
    }

    // ----- 2. ASYNC: jedno miasto po drugim -----
    // Task.Run przenosi pracę do wątku z puli, a await "oddaje" wątek UI na czas czekania.
    // Okno reaguje, ale miasta nadal są przetwarzane sekwencyjnie (łączny czas podobny).
    private async void btnAsync1_Click(object sender, RoutedEventArgs e)
    {
        Reset();
        var stopwatch = Stopwatch.StartNew();

        foreach (string city in GetCities())
        {
            Log($"Pobieram {city}...");

            double temperature = await Task.Run(() => _service.GetTemperature(city));

            // Po await wracamy na wątek UI – możemy bezpiecznie modyfikować kontrolki
            AppendResult(city, temperature);
        }

        Log($"Gotowe w {stopwatch.ElapsedMilliseconds} ms");
    }

    // ----- 3. ASYNC: wszystkie naraz, czekamy na komplet -----
    // Uruchamiamy wszystkie zadania od razu, potem Task.WhenAll czeka, aż każde się skończy.
    // Łączny czas ≈ czas najwolniejszego miasta, a nie suma. Wada: w wyniku nie mamy nazwy miasta.
    private async void btnAsync2_Click(object sender, RoutedEventArgs e)
    {
        Reset();
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task<double>>();

        foreach (string city in GetCities())
        {
            tasks.Add(Task.Run(() => _service.GetTemperature(city)));
        }

        Log("Wystartowały wszystkie zadania");
        double[] temperatures = await Task.WhenAll(tasks);
        Log($"Wszystkie zakończone w {stopwatch.ElapsedMilliseconds} ms");

        foreach (double temperature in temperatures)
        {
            AppendResult("(miasto?)", temperature);
        }
    }

    // ----- 4. ASYNC: WhenAll + krotka -----
    // Task może zwrócić dowolny typ, więc zwracamy krotkę (Temperature, City)
    // i mamy komplet danych do wyświetlenia.
    private async void btnAsync3_Click(object sender, RoutedEventArgs e)
    {
        Reset();
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task<(string City, double Temperature)>>();

        foreach (string city in GetCities())
        {
            tasks.Add(Task.Run(() => (City: city, Temperature: _service.GetTemperature(city))));
        }

        Log("Wystartowały wszystkie zadania");
        var results = await Task.WhenAll(tasks);
        Log($"Wszystkie zakończone w {stopwatch.ElapsedMilliseconds} ms");

        foreach (var (city, temperature) in results)
        {
            AppendResult(city, temperature);
        }
    }

    // ----- 5. ASYNC: wynik od razu, gdy gotowy -----
    // Nie czekamy na komplet: każde zadanie po zakończeniu samo dopisuje wynik.
    // Kontynuacja działa w wątku z puli, więc do kontrolek wchodzimy przez Dispatcher.
    private void btnAsync4_Click(object sender, RoutedEventArgs e)
    {
        Reset();
        var stopwatch = Stopwatch.StartNew();

        foreach (string city in GetCities())
        {
            Log($"Start {city}");

            Task<double> task = Task.Run(() => _service.GetTemperature(city));

            task.ContinueWith(finished =>
            {
                // Tu jesteśmy w wątku z puli – NIE wolno dotykać kontrolek bezpośrednio
                Dispatcher.Invoke(() =>
                {
                    if (finished.IsCompletedSuccessfully)
                    {
                        AppendResult(city, finished.Result);
                        Log($"Koniec {city} po {stopwatch.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        Log($"Błąd {city}: {finished.Exception?.GetBaseException().Message}");
                    }
                });
            });
        }
    }

    // ----- 6. ASYNC: pasek postępu -----
    // Scenariusz 2 wzbogacony o ProgressBar: po każdym mieście aktualizujemy pasek.
    private async void btnAsync5_Click(object sender, RoutedEventArgs e)
    {
        Reset();
        string[] cities = GetCities();
        pbProgress.Maximum = cities.Length;

        foreach (string city in cities)
        {
            Log($"Pobieram {city}...");
            double temperature = await _service.GetTemperatureWrappedAsync(city);
            pbProgress.Value += 1;
            AppendResult(city, temperature);
        }

        Log("Gotowe");
    }

    // ----- 7. ASYNC: gotowa metoda asynchroniczna (ZALECANE) -----
    // Biblioteka ma metodę async (GetStringAsync), więc nie potrzebujemy Task.Run:
    // podczas czekania na sieć żaden wątek nie jest zajęty. Najczystszy i najtańszy wariant.
    private async void btnAsync6_Click(object sender, RoutedEventArgs e)
    {
        Reset();
        string[] cities = GetCities();
        pbProgress.Maximum = cities.Length;
        btnAsync6.IsEnabled = false;   // chronimy się przed podwójnym kliknięciem

        try
        {
            foreach (string city in cities)
            {
                Log($"Pobieram {city}...");
                double temperature = await _service.GetTemperatureAsync(city);
                pbProgress.Value += 1;
                AppendResult(city, temperature);
            }

            Log("Gotowe");
        }
        catch (Exception ex)
        {
            // Wyjątek z metody async "wraca" do await – możemy go złapać zwykłym try/catch
            Log($"Błąd: {ex.Message}");
        }
        finally
        {
            btnAsync6.IsEnabled = true;
        }
    }
}
