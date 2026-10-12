# Dzień 11: instrukcja dla uczestnika

> **Co dziś zbudujesz:** trzy projekty w .NET 10.
> 1. **P01Pogoda** – aplikacja konsolowa, która pobiera temperaturę z internetu i przelicza jednostki.
> 2. **P02WeatherForecast** – aplikacja WPF, w której zobaczysz, czym różni się kod synchroniczny od asynchronicznego.
> 3. **P03WeatherForecastWPF.Client** – klient usługi pogodowej Open‑Meteo (JSON). Ten projekt będziesz rozwijać przez kolejne 7 dni.
>
> Pracuj po kolei. Każda sekcja ma punkt **Sprawdź** – nie idź dalej, dopóki nie działa.

## Zanim zaczniesz

**Potrzebujesz:**

- Visual Studio 2026 z workloadem **.NET desktop development** (albo VS Code + .NET 10 SDK).
- .NET 10 SDK: w terminalu `dotnet --version` ma pokazać `10.0.x`. Jeśli nie, pobierz z https://dotnet.microsoft.com/download/dotnet/10.0.
- Dostęp do internetu (usługi `wttr.in` i `open-meteo.com`).

**Jak korzystać z tej instrukcji:**

- Bloki kodu wklejasz **w całości** do pliku, którego ścieżka jest w pierwszej linii komentarza.
- Fragmenty oznaczone **Dlaczego tak** wyjaśniają decyzje. Przeczytaj je, bo na tych decyzjach opierają się kolejne dni.
- Jeśli coś nie działa, zajrzyj do **Częste problemy** na końcu sekcji.

**Gotowy kod** jest w repozytorium `tomasz-trainer/32Dzien11`, ale najwięcej nauczysz się, pisząc go sam.

---

## 0. Teoria: .NET Framework a .NET

### Co to jest .NET (dawniej .NET Core)?

Przez 10 dni używaliśmy **.NET Framework**: działa tylko na Windows, ostatnia wersja to 4.8 i nowych nie będzie. **.NET** (bez „Framework”, od wersji 5) to jego następca:

| Cecha | .NET Framework | .NET (5, 6, …, 10) |
|---|---|---|
| Systemy | Windows | Windows, Linux, macOS, Android, iOS |
| Narzędzia | Visual Studio | CLI `dotnet` + dowolny edytor (VS, VS Code, Rider) |
| Desktop | Windows Forms, WPF | WPF, Windows Forms (tylko Windows), MAUI (wieloplatformowo) |
| Rozwój | zakończony | nowa wersja co rok, parzyste = LTS (3 lata wsparcia) |
| Wydajność | – | kilka razy szybszy, zwłaszcza w web |

Używamy **.NET 10**, bo to aktualna wersja **LTS** (Long Term Support).

### Dlaczego WPF, a nie Windows Forms?

WPF opisuje interfejs w języku **XAML** (XML), oddziela wygląd od logiki i ma wbudowany **binding** danych, który jutro pozwoli nam zastosować wzorzec MVVM. Ten sam XAML i MVVM zobaczysz w dniu 18 w **MAUI** na telefonie. Alternatywy: WinUI 3 (nowoczesny wygląd, tylko Windows), Avalonia (WPF‑podobny, wieloplatformowy, firma trzecia).

### Plan dni 11–18

| Dzień | Temat |
|---|---|
| 11 | konsola i WPF, pobieranie danych z internetu, async |
| 12 | MVVM i Dependency Injection |
| 13 | własne Web API i klient |
| 14 | baza danych: EF Core, migracje, CRUD |
| 15 | Azure: baza, API, rozpoznawanie mowy |
| 16 | Razor Pages i MVC |
| 17 | Blazor WebAssembly |
| 18 | MAUI |

Od dnia 13 budujemy jeden system (sklep), więc **kod z każdego dnia jest punktem wyjścia następnego**.

---

## 1. Solucja i projekt z linii poleceń

**Cel:** stworzyć solucję i projekt bez kreatora i zrozumieć, co robią polecenia `dotnet`.

### Teoria w pigułce

`dotnet` to narzędzie wiersza poleceń (CLI). Visual Studio pod spodem wywołuje te same polecenia. Umiejętność pracy z CLI przydaje się na serwerach, w Dockerze i w GitHub Actions, gdzie nie ma Visual Studio.

### Krok po kroku

1. Utwórz folder `32Dzien11` i otwórz w nim terminal (PowerShell).
2. Wykonaj:

```powershell
dotnet new sln -n 32Dzien11 --format slnx
dotnet new console -n P01Pogoda -o P01Pogoda -f net10.0
dotnet sln 32Dzien11.slnx add P01Pogoda/P01Pogoda.csproj
dotnet build
dotnet run --project P01Pogoda
```

| Polecenie | Co robi |
|---|---|
| `dotnet new sln --format slnx` | tworzy solucję w nowym, czytelnym formacie XML |
| `dotnet new console -n … -o … -f net10.0` | tworzy projekt konsolowy (`-n` nazwa, `-o` folder, `-f` wersja .NET) |
| `dotnet sln add` | dopisuje projekt do solucji |
| `dotnet build` / `dotnet run` | kompiluje / uruchamia (`run` sam buduje) |
| `dotnet new list` | lista wszystkich szablonów |

3. Otwórz `32Dzien11.slnx` w Visual Studio i zajrzyj do `P01Pogoda/Program.cs` oraz `P01Pogoda.csproj`.

### Sprawdź

`dotnet run --project P01Pogoda` wypisuje `Hello, World!`.

### Dlaczego tak

- W `Program.cs` nie ma klasy ani `Main`: to **top‑level statements**. Kompilator sam dokłada klasę `Program` i metodę `Main`. Możesz używać `await` bezpośrednio.
- W `.csproj` `ImplicitUsings` dodaje automatycznie `using System;`, `System.Linq`, `System.Net.Http` i kilka innych. `Nullable` włącza ostrzeżenia o możliwych wartościach `null`.

---

## 2. P01Pogoda: HttpClient, regex, jednostki

**Cel:** pobrać tekst przez HTTP, wyciągnąć z niego liczbę, przeliczyć jednostki, obsłużyć błędy.

### Teoria w pigułce

- **`HttpClient`** to klasa do zapytań HTTP. Dawny `WebClient` jest w .NET oznaczony jako przestarzały. `HttpClient` tworzymy **raz** na aplikację: każdy nowy egzemplarz otwiera połączenia, których system nie zwalnia od razu (tzw. socket exhaustion).
- **Wyrażenia regularne (regex)** to wzorce do przeszukiwania tekstu. `\d+` to „jedna lub więcej cyfr”, `[+-]?` to „opcjonalny znak”. Testuj wzorce na https://regex101.com.
- **`enum`** to typ z listą nazwanych wartości. Lepszy niż napisy `"C"`, `"F"`, bo kompilator wyłapie literówkę.
- Usługa **wttr.in** zwraca pogodę jako zwykły tekst. Otwórz w przeglądarce `https://wttr.in/Warszawa?format=%t&m` – zobaczysz np. `+9°C`. Parametr `&m` wymusza Celsjusze.

### Krok po kroku

1. W projekcie P01Pogoda dodaj klasę `WeatherManager.cs`:

```csharp
// P01Pogoda/WeatherManager.cs
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
```

2. Zastąp `Program.cs`:

```csharp
// P01Pogoda/Program.cs
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
```

3. Uruchom: `dotnet run --project P01Pogoda`. Wpisz `Warszawa`, `New York`, potem `xyzxyz`.

### Sprawdź

- Dla miasta widzisz trzy wartości, np. `Warszawa: 9.0 °C | 48.2 °F | 282.1 K`.
- Dla `xyzxyz` widzisz komunikat o błędzie, a program **nie** kończy się wyjątkiem.

### Dlaczego tak

- `static readonly HttpClient` – jeden na aplikację (patrz teoria).
- `(?<value>…)` to **grupa nazwana**: czytelniejsze niż `Groups[1]`.
- `CultureInfo.InvariantCulture` – na polskim Windowsie `double.Parse("12.5")` rzuca wyjątek, bo oczekuje przecinka. Dane z internetu zawsze mają kropkę.
- Dwa `catch`: inny komunikat, gdy nie ma sieci (`HttpRequestException`), inny, gdy odpowiedź jest, ale nie zawiera temperatury (`FormatException`).

### Zadanie dodatkowe

Program pobiera dane **trzy razy** o to samo. Przerób go tak, żeby pobrał raz (w Celsjuszach) i przeliczył metodą `WeatherManager.Convert`.

### Częste problemy

| Objaw | Przyczyna / rozwiązanie |
|---|---|
| `°` wyświetla się jako krzaczki | brakuje `Console.OutputEncoding = UTF8` |
| `+48°F` zamiast Celsjuszy | brakuje `&m` w adresie |
| wyjątek `TaskCanceledException` | przekroczony `Timeout` (10 s) – usługa wttr.in bywa wolna, spróbuj ponownie |

---

## 3. P02WeatherForecast: projekt WPF i kontenery

**Cel:** poznać strukturę projektu WPF, język XAML i trzy podstawowe kontenery.

### Teoria w pigułce

- **XAML** to XML, który opisuje drzewo kontrolek. Każdy element XAML to obiekt C#; `x:Name="txtCities"` tworzy pole `txtCities` w klasie okna.
- Plik `MainWindow.xaml` (wygląd) i `MainWindow.xaml.cs` (**code‑behind**, logika) tworzą jedną klasę `partial`.
- **Kontenery** układają kontrolki:

| Kontener | Zasada | Typowe użycie |
|---|---|---|
| `DockPanel` | „magnes”: `DockPanel.Dock="Top/Left/…"`, ostatni element wypełnia resztę | szkielet okna |
| `Grid` | wiersze i kolumny; `Auto` = tyle, ile treść; `*` = reszta miejsca; `3*` = trzy części | formularze |
| `StackPanel` | elementy jeden pod drugim (lub obok: `Orientation="Horizontal"`) | listy przycisków |

Kontenery można zagnieżdżać.

### Krok po kroku

1. Dodaj do solucji projekt **WPF Application** o nazwie `P02WeatherForecast` (.NET 10). Z konsoli:

```powershell
dotnet new wpf -n P02WeatherForecast -o P02WeatherForecast -f net10.0
dotnet sln 32Dzien11.slnx add P02WeatherForecast/P02WeatherForecast.csproj
```

2. Ustaw P02 jako projekt startowy (prawy przycisk → **Set as Startup Project**) i uruchom F5. Zobaczysz puste okno.
3. Utwórz folder `Images` i wrzuć do niego dowolny obrazek `Logo.png` (w repozytorium jest gotowy). Zaznacz plik → **Properties → Build Action: Resource**. Plik projektu ma wyglądać tak:

```xml
<!-- P02WeatherForecast/P02WeatherForecast.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <!-- Obrazek wbudowany w plik .exe (Build Action: Resource) -->
    <Resource Include="Images\Logo.png" />
  </ItemGroup>

</Project>
```

4. Zastąp `MainWindow.xaml`:

```xml
<!-- P02WeatherForecast/MainWindow.xaml -->
<Window x:Class="P02WeatherForecast.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:P02WeatherForecast"
        mc:Ignorable="d"
        Title="P02 – Pogoda: synchronicznie i asynchronicznie" Height="720" Width="960">

    <!-- Zasoby okna: widoczne tylko w tym oknie. Nadpisują zasoby aplikacji o tym samym kluczu. -->
    <Window.Resources>
        <Style x:Key="LogListStyle" TargetType="ListView" BasedOn="{StaticResource BaseControlStyle}">
            <Setter Property="Background" Value="#F7F7F7" />
            <Setter Property="BorderBrush" Value="LightGray" />
        </Style>
    </Window.Resources>

    <!-- DockPanel: elementy "przyklejamy" do krawędzi (Top, Left...), ostatni wypełnia resztę -->
    <DockPanel>

        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_Plik">
                <MenuItem Header="_Wyczyść wyniki" Click="MenuClear_Click" />
                <Separator />
                <MenuItem Header="_Zamknij" Click="MenuExit_Click" />
            </MenuItem>
            <MenuItem Header="_Pomoc">
                <MenuItem Header="_O programie" Click="MenuAbout_Click" />
            </MenuItem>
        </Menu>

        <!-- Obrazek: Build Action = Resource (ustawione w .csproj), ścieżka względna do projektu -->
        <Image DockPanel.Dock="Left" Width="110" Margin="8" Source="/Images/Logo.png" />

        <!-- Grid: siatka wierszy i kolumn. "*" = część wolnego miejsca, "3*" = trzy części -->
        <Grid Margin="8">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="3*" />
                <ColumnDefinition Width="2*" />
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />
                <RowDefinition Height="*" />
            </Grid.RowDefinitions>

            <!-- StackPanel: elementy jeden pod drugim (Orientation=Vertical to domyślne) -->
            <StackPanel Grid.Row="0" Grid.Column="0">
                <TextBlock Text="Miasta (każde w nowej linii):" FontWeight="Bold" Margin="4" />
                <TextBox x:Name="txtCities" AcceptsReturn="True" Height="110"
                         VerticalScrollBarVisibility="Auto" Style="{StaticResource BaseControlStyle}" />

                <Button x:Name="btnSync" Content="1. Synchronicznie (zamraża okno)" Click="btnSync_Click" />
                <Button x:Name="btnAsync1" Content="2. Async: jedno miasto po drugim (Task.Run + await)" Click="btnAsync1_Click" />
                <Button x:Name="btnAsync2" Content="3. Async: wszystkie naraz, czekamy na komplet (Task.WhenAll)" Click="btnAsync2_Click" />
                <Button x:Name="btnAsync3" Content="4. Async: WhenAll + krotka (temperatura, miasto)" Click="btnAsync3_Click" />
                <Button x:Name="btnAsync4" Content="5. Async: wynik od razu, gdy gotowy (ContinueWith / Dispatcher)" Click="btnAsync4_Click" />
                <Button x:Name="btnAsync5" Content="6. Async: pasek postępu" Click="btnAsync5_Click" />
                <Button x:Name="btnAsync6" Style="{StaticResource AccentButtonStyle}"
                        Content="7. Async: gotowa metoda GetTemperatureAsync (zalecane)" Click="btnAsync6_Click" />

                <ProgressBar x:Name="pbProgress" Height="18" Margin="4" />
            </StackPanel>

            <!-- Wyniki -->
            <GroupBox Grid.Row="0" Grid.Column="1" Header="Wyniki" Margin="4">
                <TextBlock x:Name="tbResults" TextWrapping="Wrap" Margin="4" />
            </GroupBox>

            <!-- Dziennik zdarzeń – zajmuje obie kolumny w dolnym wierszu -->
            <GroupBox Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="2" Header="Dziennik" Margin="4">
                <ListView x:Name="lvLog" Style="{StaticResource LogListStyle}" />
            </GroupBox>
        </Grid>
    </DockPanel>
</Window>
```

5. Zastąp `MainWindow.xaml.cs` wersją tymczasową (puste metody – pełną wkleisz w sekcji 6):

```csharp
// P02WeatherForecast/MainWindow.xaml.cs  (wersja tymczasowa)
using System.Windows;

namespace P02WeatherForecast;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        txtCities.Text = string.Join(Environment.NewLine, "Warszawa", "Kraków", "Gdańsk", "Berlin", "Paryż", "Tokio");
    }

    private void MenuClear_Click(object sender, RoutedEventArgs e) { tbResults.Text = ""; lvLog.Items.Clear(); }
    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();
    private void MenuAbout_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show("Dzień 11 – kontenery, style i programowanie asynchroniczne w WPF.", "O programie");

    private void btnSync_Click(object sender, RoutedEventArgs e) { }
    private void btnAsync1_Click(object sender, RoutedEventArgs e) { }
    private void btnAsync2_Click(object sender, RoutedEventArgs e) { }
    private void btnAsync3_Click(object sender, RoutedEventArgs e) { }
    private void btnAsync4_Click(object sender, RoutedEventArgs e) { }
    private void btnAsync5_Click(object sender, RoutedEventArgs e) { }
    private void btnAsync6_Click(object sender, RoutedEventArgs e) { }
}
```

> Projekt **jeszcze się nie zbuduje**: XAML odwołuje się do stylów `BaseControlStyle` i `AccentButtonStyle`, które dodasz w sekcji 4. To celowe – najpierw układ, potem style.

### Dlaczego tak

- `Resource` wbudowuje obrazek w `.exe` (jeden plik do rozdania). Alternatywa `Content` + `Copy to Output Directory` zostawia obrazek obok `.exe` i pozwala go podmienić bez kompilacji.
- `_` w `Header="_Plik"` to skrót klawiszowy (Alt+P).
- `Grid.ColumnSpan="2"` rozciąga element na dwie kolumny.
- `namespace P02WeatherForecast;` ze średnikiem (file‑scoped namespace, C# 10) oszczędza jedno wcięcie.

---

## 4. P02: style i zasoby

**Cel:** zrozumieć zasoby aplikacji i okna, style niejawne, style z kluczem i dziedziczenie `BasedOn`.

### Teoria w pigułce

- **Zasoby** (`Resources`) to słownik obiektów (kolory, style, konwertery) dostępnych w XAML. `Application.Resources` widzi każde okno; `Window.Resources` tylko jedno okno i ma pierwszeństwo przy tym samym kluczu.
- **Styl niejawny** (`<Style TargetType="Button">` bez `x:Key`) działa na wszystkie przyciski. **Styl z kluczem** trzeba wskazać: `Style="{StaticResource NazwaStylu}"`.
- **`BasedOn`** to dziedziczenie stylów: styl pochodny bierze ustawienia bazowego i dokłada własne.
- `StaticResource` czyta zasób raz przy starcie; `DynamicResource` śledzi zmiany (np. przełączanie motywu).

### Krok po kroku

1. Zastąp `App.xaml`:

```xml
<!-- P02WeatherForecast/App.xaml -->
<Application x:Class="P02WeatherForecast.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:P02WeatherForecast"
             StartupUri="MainWindow.xaml">
    <Application.Resources>

        <!-- Zasoby aplikacji: widoczne w KAŻDYM oknie. -->

        <!-- 1. Pędzel (kolor) z kluczem – użyjemy go w kilku stylach -->
        <SolidColorBrush x:Key="PrimaryBrush" Color="#1E6FBF" />
        <SolidColorBrush x:Key="PrimaryLightBrush" Color="#DCEBFA" />

        <!-- 2. Styl bez klucza (implicit style): dotyczy WSZYSTKICH przycisków w aplikacji -->
        <Style TargetType="Button">
            <Setter Property="Background" Value="{StaticResource PrimaryLightBrush}" />
            <Setter Property="Foreground" Value="{StaticResource PrimaryBrush}" />
            <Setter Property="FontSize" Value="14" />
            <Setter Property="Margin" Value="4" />
            <Setter Property="Padding" Value="8,4" />
            <Setter Property="Cursor" Value="Hand" />
        </Style>

        <!-- 3. Styl bazowy dla dowolnej kontrolki (Control = wspólny przodek Button, TextBox, ListBox...) -->
        <Style x:Key="BaseControlStyle" TargetType="Control">
            <Setter Property="FontFamily" Value="Segoe UI" />
            <Setter Property="FontSize" Value="14" />
            <Setter Property="Margin" Value="4" />
        </Style>

        <!-- 4. Dziedziczenie stylów: BasedOn. Przycisk "główny" rozszerza styl bazowy -->
        <Style x:Key="AccentButtonStyle" TargetType="Button" BasedOn="{StaticResource BaseControlStyle}">
            <Setter Property="Background" Value="{StaticResource PrimaryBrush}" />
            <Setter Property="Foreground" Value="White" />
            <Setter Property="FontWeight" Value="Bold" />
            <Setter Property="Padding" Value="10,6" />
        </Style>

    </Application.Resources>
</Application>
```

2. Uruchom F5.

### Sprawdź

- Okno ma menu, logo z lewej, pole z sześcioma miastami, siedem jasnoniebieskich przycisków (ostatni ciemnoniebieski z białym tekstem), ramki „Wyniki” i „Dziennik”.
- **Pomoc → O programie** pokazuje komunikat.
- Eksperyment: zmień `PrimaryBrush` na `#C0392B` – zmieni się kolor wszystkich elementów, które z niego korzystają.

### Dlaczego tak

`TargetType="Control"` w stylu bazowym pozwala użyć jednego stylu dla `TextBox`, `ListView` i `Button`, bo `Control` jest ich wspólnym przodkiem. Zmiana w stylu bazowym zmienia wszystkie pochodne – tak jak zmiana w klasie bazowej.

---

## 5. P02: serwis synchroniczny i problem zamrożonego okna

**Cel:** zobaczyć na własne oczy, co się dzieje, gdy wątek interfejsu czeka na sieć.

### Teoria w pigułce

WPF ma **jeden wątek interfejsu** (UI thread). On rysuje okno, obsługuje kliknięcia i wykonuje metody `…_Click`. Jeśli w takiej metodzie czekamy 2 sekundy na serwer, przez 2 sekundy nikt nie rysuje okna: nie da się go przesunąć, Windows pokazuje „Brak odpowiedzi”. To jest kod **synchroniczny**.

### Krok po kroku

1. Dodaj folder `Services` i klasę `WeatherForecastService.cs`:

```csharp
// P02WeatherForecast/Services/WeatherForecastService.cs
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
```

2. W `MainWindow.xaml.cs` dodaj pole `private readonly WeatherForecastService _service = new();` (i `using P02WeatherForecast.Services;`), a w `btnSync_Click` wpisz:

```csharp
private void btnSync_Click(object sender, RoutedEventArgs e)
{
    tbResults.Text = ""; lvLog.Items.Clear();
    foreach (string city in txtCities.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
    {
        lvLog.Items.Add($"Pobieram {city}...");
        double temperature = _service.GetTemperature(city);   // blokuje wątek UI
        tbResults.Text += $"{city}: {temperature:F1} °C{Environment.NewLine}";
    }
}
```

3. Uruchom, kliknij **1. Synchronicznie** i w trakcie spróbuj przesunąć okno albo otworzyć menu.

### Sprawdź

Okno nie reaguje, aż wszystkie miasta się pobiorą. Wpisy „Pobieram…” pojawiają się **wszystkie naraz na końcu**, a nie po kolei (wątek nie miał czasu ich narysować).

### Dlaczego tak

Serwis ma trzy metody o tym samym wyniku:

| Metoda | Jak działa | Kiedy używać |
|---|---|---|
| `GetTemperature` | blokuje wątek (`Send` bez `Async`) | nigdy w UI; tu tylko do pokazania problemu |
| `GetTemperatureAsync` | gotowa metoda asynchroniczna biblioteki; podczas czekania **żaden** wątek nie jest zajęty | zawsze, gdy biblioteka ma wersję `…Async` |
| `GetTemperatureWrappedAsync` | `Task.Run` przenosi kod synchroniczny do wątku z puli | gdy biblioteka ma tylko wersję synchroniczną albo liczymy coś ciężkiego |

---

## 6. P02: siedem wariantów asynchronicznych

**Cel:** zrozumieć `async/await`, `Task.Run`, `Task.WhenAll`, krotki, `Dispatcher`, postęp i wyjątki.

### Teoria w pigułce

- **`async`/`await`**: `await` mówi „zwróć wątek wywołującemu, a gdy zadanie się skończy, wróć do tej linii”. W WPF po `await` wracasz na wątek UI, więc możesz bezpiecznie zmieniać kontrolki.
- **`async void`** jest dozwolone **tylko** w obsłudze zdarzeń (bo WPF wymaga `void`). Wszędzie indziej metody asynchroniczne zwracają `Task` lub `Task<T>`.
- **`Task.Run`** uruchamia kod w wątku z puli (ThreadPool). **`Task.WhenAll`** czeka na wiele zadań naraz.
- Z innego wątku **nie wolno** dotykać kontrolek – trzeba użyć `Dispatcher.Invoke`.

### Krok po kroku

1. Zastąp cały `MainWindow.xaml.cs`:

```csharp
// P02WeatherForecast/MainWindow.xaml.cs
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
```

2. Uruchom i klikaj przyciski po kolei. Po każdym kliknięciu spójrz na **Dziennik** (czas w ms) i spróbuj przesunąć okno.

### Sprawdź

| Przycisk | Czego się spodziewać |
|---|---|
| 1 | okno zamrożone, wpisy pojawiają się na końcu |
| 2 | okno działa, wpisy na bieżąco, czas ≈ suma czasów miast |
| 3 | czas kilka razy krótszy (miasta równolegle), ale wyniki bez nazwy miasta |
| 4 | jak 3, z nazwami miast |
| 5 | wyniki w kolejności **odpowiedzi serwera**, nie listy |
| 6 | pasek postępu rośnie po każdym mieście |
| 7 | jak 6; dopisz do listy `xyzxyz` – w dzienniku pojawi się „Błąd: …”, a przycisk znów będzie aktywny |

3. Eksperyment: w wariancie 5 usuń `Dispatcher.Invoke(() => { … })`, zostawiając samo `AppendResult(...)`. Uruchom – dostaniesz `InvalidOperationException: The calling thread cannot access this object`. Przywróć kod.

### Dlaczego tak

| Sytuacja | Co robić |
|---|---|
| Biblioteka ma metodę `…Async` | `await` i nic więcej (wariant 7) – podczas czekania żaden wątek nie pracuje |
| Biblioteka ma tylko metodę synchroniczną albo liczysz coś ciężkiego (CPU) | `await Task.Run(…)` (warianty 2, 6) |
| Wiele niezależnych operacji | uruchom wszystkie, potem `await Task.WhenAll` (warianty 3, 4) |
| Kod z innego wątku ma zmienić kontrolkę | `Dispatcher.Invoke` (wariant 5); w praktyce lepiej użyć `await`, który sam wraca na wątek UI |

Wariant 7 to wzorzec na cały kurs: `try/catch/finally` wokół `await`, blokada przycisku na czas operacji, komunikat zamiast wyjątku.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `The calling thread cannot access this object` | zmieniasz kontrolkę z innego wątku – użyj `await` albo `Dispatcher.Invoke` |
| Przycisk 1 nie zamraża okna | wttr.in odpowiedziało bardzo szybko – dodaj więcej miast |
| Ostrzeżenie „async method lacks await” | w którejś metodzie `async` nie ma `await` – sprawdź, czy wkleiłeś cały plik |

---

## 7. P03WeatherForecastWPF.Client: klient Open‑Meteo

**Cel:** przeczytać dokumentację API, zamienić JSON na klasy C#, napisać serwis i okno.

### Teoria w pigułce

- **API** (interfejs programistyczny) to usługa, do której program wysyła zapytanie HTTP i dostaje dane, najczęściej w formacie **JSON**. Przeglądarka też potrafi je wyświetlić – to najprostszy sposób na poznanie API.
- **Open‑Meteo** jest darmowe i **nie wymaga klucza**. Inne usługi (AccuWeather, OpenWeatherMap) wymagają rejestracji i klucza API, który jest sekretem i nie może trafić do repozytorium (o sekretach powiemy w dniu 15).
- Pogoda wymaga **współrzędnych**, więc najpierw pytamy **geocoding** (miasto → lista pasujących miejsc ze współrzędnymi), potem **forecast** (współrzędne → pogoda).
- **Deserializacja** to zamiana JSON na obiekt C#. W .NET służy do tego wbudowany `System.Text.Json`. Nazwy pól JSON (`country_code`) mapujemy na właściwości C# (`CountryCode`) atrybutem `[JsonPropertyName]`.
- Rozróżniamy **DTO** (klasy odwzorowujące JSON dostawcy) i **model domenowy** (`Weather` – to, co pokazujemy). Dzięki temu zmiana dostawcy nie zmienia okna.

### Krok po kroku

1. Otwórz w przeglądarce i obejrzyj odpowiedzi:
   - `https://geocoding-api.open-meteo.com/v1/search?name=Warszawa&count=10&language=pl&format=json`
   - `https://api.open-meteo.com/v1/forecast?latitude=52.23&longitude=21.01&current=temperature_2m,wind_speed_10m,weather_code&timezone=auto`
   - Dokumentacja: https://open-meteo.com/en/docs (sekcja „Current Weather”) i https://open-meteo.com/en/docs/geocoding-api

2. Dodaj do solucji projekt **WPF Application** `P03WeatherForecastWPF.Client` (.NET 10) i utwórz w nim foldery `Models` i `Services`.

3. W `Models` utwórz cztery pliki:

```csharp
// P03WeatherForecastWPF.Client/Models/City.cs
using System.Text.Json.Serialization;

namespace P03WeatherForecastWPF.Client.Models;

/// <summary>
/// Jedno miasto z odpowiedzi Open-Meteo Geocoding API.
/// Nazwy w JSON są w stylu snake_case (country_code), a w C# PascalCase,
/// dlatego mapujemy je atrybutem [JsonPropertyName].
/// </summary>
public class City
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("admin1")]
    public string? Admin1 { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("population")]
    public int? Population { get; set; }

    /// <summary>Tekst do wyświetlenia na liście, np. "Warszawa, Województwo mazowieckie (Polska)".</summary>
    [JsonIgnore]
    public string FullName =>
        string.IsNullOrEmpty(Admin1) ? $"{Name} ({Country})" : $"{Name}, {Admin1} ({Country})";
}
```

```csharp
// P03WeatherForecastWPF.Client/Models/GeocodingResponse.cs
using System.Text.Json.Serialization;

namespace P03WeatherForecastWPF.Client.Models;

/// <summary>
/// Cała odpowiedź z https://geocoding-api.open-meteo.com/v1/search
/// Gdy nic nie znaleziono, pole "results" w ogóle nie występuje – stąd typ nullable.
/// </summary>
public class GeocodingResponse
{
    [JsonPropertyName("results")]
    public List<City>? Results { get; set; }

    [JsonPropertyName("generationtime_ms")]
    public double GenerationTimeMs { get; set; }
}
```

```csharp
// P03WeatherForecastWPF.Client/Models/ForecastResponse.cs
using System.Text.Json.Serialization;

namespace P03WeatherForecastWPF.Client.Models;

/// <summary>
/// Odpowiedź z https://api.open-meteo.com/v1/forecast?current=temperature_2m,...
/// Mapujemy tylko pola, których używamy – reszta JSON-a jest ignorowana.
/// </summary>
public class ForecastResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("current_units")]
    public CurrentUnits? CurrentUnits { get; set; }

    [JsonPropertyName("current")]
    public CurrentWeather? Current { get; set; }
}

public class CurrentUnits
{
    [JsonPropertyName("temperature_2m")]
    public string? Temperature2m { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public string? WindSpeed10m { get; set; }
}

public class CurrentWeather
{
    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("temperature_2m")]
    public double Temperature2m { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed10m { get; set; }

    /// <summary>Kod pogody WMO (0 = bezchmurnie, 61 = deszcz itd.).</summary>
    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}
```

```csharp
// P03WeatherForecastWPF.Client/Models/Weather.cs
namespace P03WeatherForecastWPF.Client.Models;

/// <summary>
/// Model "domenowy" pogody – to, co pokazujemy użytkownikowi.
/// Jest niezależny od formatu JSON konkretnego dostawcy (Open-Meteo),
/// dzięki czemu zmiana dostawcy nie zmienia interfejsu.
/// </summary>
public class Weather
{
    public DateTime Time { get; set; }

    public double Temperature { get; set; }

    public double WindSpeed { get; set; }

    public int WeatherCode { get; set; }

    /// <summary>Opis słowny na podstawie kodu WMO.</summary>
    public string Description => WeatherCode switch
    {
        0 => "bezchmurnie",
        1 or 2 or 3 => "częściowe zachmurzenie",
        45 or 48 => "mgła",
        51 or 53 or 55 => "mżawka",
        61 or 63 or 65 => "deszcz",
        71 or 73 or 75 => "śnieg",
        80 or 81 or 82 => "przelotne opady",
        95 or 96 or 99 => "burza",
        _ => $"kod WMO {WeatherCode}"
    };
}
```

4. W `Services` utwórz serwis:

```csharp
// P03WeatherForecastWPF.Client/Services/OpenMeteoService.cs
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using P03WeatherForecastWPF.Client.Models;

namespace P03WeatherForecastWPF.Client.Services;

/// <summary>
/// Komunikacja z Open-Meteo: wyszukiwanie miast (geocoding) i aktualna pogoda.
/// Dokumentacja: https://open-meteo.com/en/docs i https://open-meteo.com/en/docs/geocoding-api
/// </summary>
public class OpenMeteoService
{
    private const string GeocodingBaseUrl = "https://geocoding-api.open-meteo.com/v1/search";
    private const string ForecastBaseUrl = "https://api.open-meteo.com/v1/forecast";

    private readonly HttpClient _httpClient;
    private readonly string _language;

    /// <param name="httpClient">Klient HTTP – przekazujemy z zewnątrz, żeby nie tworzyć go w każdym serwisie.</param>
    /// <param name="language">Język nazw miast, np. "pl" lub "en".</param>
    public OpenMeteoService(HttpClient httpClient, string language)
    {
        _httpClient = httpClient;
        _language = language;
    }

    public async Task<City[]> GetLocationsAsync(string locationName)
    {
        string url = $"{GeocodingBaseUrl}?name={Uri.EscapeDataString(locationName)}&count=10&language={_language}&format=json";

        // GetFromJsonAsync pobiera odpowiedź i od razu deserializuje JSON do obiektu
        GeocodingResponse? response = await _httpClient.GetFromJsonAsync<GeocodingResponse>(url);

        return response?.Results?.ToArray() ?? [];
    }

    public async Task<Weather?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        // Współrzędne zawsze z kropką dziesiętną, niezależnie od polskich ustawień regionalnych
        string lat = latitude.ToString(CultureInfo.InvariantCulture);
        string lon = longitude.ToString(CultureInfo.InvariantCulture);

        string url = $"{ForecastBaseUrl}?latitude={lat}&longitude={lon}&current=temperature_2m,wind_speed_10m,weather_code&timezone=auto";

        ForecastResponse? response = await _httpClient.GetFromJsonAsync<ForecastResponse>(url);

        if (response?.Current is null)
        {
            return null;
        }

        // Mapowanie z modelu dostawcy (ForecastResponse) na nasz model (Weather)
        return new Weather
        {
            Time = DateTime.TryParse(response.Current.Time, CultureInfo.InvariantCulture, out DateTime time) ? time : DateTime.Now,
            Temperature = response.Current.Temperature2m,
            WindSpeed = response.Current.WindSpeed10m,
            WeatherCode = response.Current.WeatherCode
        };
    }
}
```

5. Zastąp `MainWindow.xaml`:

```xml
<!-- P03WeatherForecastWPF.Client/MainWindow.xaml -->
<Window x:Class="P03WeatherForecastWPF.Client.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:P03WeatherForecastWPF.Client"
        mc:Ignorable="d"
        Title="P03 – Pogoda (Open-Meteo)" Height="420" Width="640">

    <DockPanel Margin="8">

        <!-- Lewa kolumna: wyszukiwanie miasta -->
        <StackPanel DockPanel.Dock="Left" Width="260" Margin="0,0,8,0">
            <Label Content="Miasto:" />
            <TextBox x:Name="txtCity" Text="Warszawa" />
            <Button x:Name="btnSearch" Content="Szukaj" Click="btnSearch_Click" Margin="0,4" />
            <ListBox x:Name="lbCities" Height="260"
                     DisplayMemberPath="FullName"
                     SelectionChanged="lbCities_SelectionChanged" />
        </StackPanel>

        <!-- Prawa część: pogoda dla wybranego miasta -->
        <StackPanel>
            <Label Content="Wybrane miasto:" FontWeight="Bold" />
            <Label x:Name="lblCityName" Content="—" />

            <Label Content="Temperatura:" FontWeight="Bold" />
            <Label x:Name="lblTemperature" Content="—" FontSize="28" />

            <Label Content="Wiatr:" FontWeight="Bold" />
            <Label x:Name="lblWind" Content="—" />

            <Label Content="Opis:" FontWeight="Bold" />
            <Label x:Name="lblDescription" Content="—" />

            <Label Content="Czas pomiaru:" FontWeight="Bold" />
            <Label x:Name="lblTime" Content="—" />

            <TextBlock x:Name="tbStatus" Foreground="DarkRed" TextWrapping="Wrap" Margin="4" />
        </StackPanel>
    </DockPanel>
</Window>
```

6. Zastąp `MainWindow.xaml.cs` (konfigurację z sekcji 8 dodamy za chwilę – ten plik już jej używa):

```csharp
// P03WeatherForecastWPF.Client/MainWindow.xaml.cs
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
```

### Dlaczego tak

- `HttpClient` wchodzi do serwisu **konstruktorem** – to już jest wstrzykiwanie zależności (Dependency Injection), tylko ręczne. Jutro zrobi to za nas kontener.
- `?? []` zwraca pustą tablicę, gdy odpowiedź jest pusta – okno nigdy nie dostaje `null`.
- `latitude.ToString(CultureInfo.InvariantCulture)` – na polskim Windowsie `52.23.ToString()` da `52,23`, a serwer odpowie błędem 400. **Najczęstszy błąd na tym kursie.**
- `DisplayMemberPath="FullName"` – lista dostaje obiekty `City`, a wyświetla właściwość `FullName`.
- `lbCities.SelectedItem is not City selectedCity` – pattern matching: jeśli nic nie zaznaczono, wychodzimy.

---

## 8. P03: konfiguracja w `appsettings.json`

**Cel:** trzymać ustawienia poza kodem i poznać mechanizm konfiguracji, który działa tak samo w API, Blazorze i MAUI.

### Teoria w pigułce

`Microsoft.Extensions.Configuration` czyta ustawienia z plików JSON, zmiennych środowiskowych, User Secrets itd. i udostępnia je pod kluczami `Sekcja:Klucz`. Plik `appsettings.json` **nie** jest wbudowany w `.exe`, więc można go zmienić bez kompilacji.

### Krok po kroku

1. Dodaj pakiet NuGet do P03: prawy przycisk → **Manage NuGet Packages** → `Microsoft.Extensions.Configuration.Json` (10.0.x). Albo z konsoli:

```powershell
dotnet add P03WeatherForecastWPF.Client package Microsoft.Extensions.Configuration.Json
```

2. Dodaj do projektu plik `appsettings.json`, zaznacz go → **Properties → Copy to Output Directory: Copy if newer**:

```json
// P03WeatherForecastWPF.Client/appsettings.json
{
  "AppSettings": {
    "DefaultLanguage": "pl"
  }
}
```

3. Sprawdź plik projektu (powinien wyglądać tak):

```xml
<!-- P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.12" />
  </ItemGroup>

  <ItemGroup>
    <!-- Plik konfiguracyjny kopiowany obok .exe, żeby aplikacja mogła go odczytać -->
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

4. Ustaw P03 jako projekt startowy i uruchom.

### Sprawdź

- Wpisz `Warszawa` → **Szukaj** → kliknij pierwszy wynik. Widzisz temperaturę, wiatr, opis i czas.
- Wpisz `Kraków` – na liście jest też Krakow w Wisconsin (USA). Tak działa geocoding: zwraca wszystkie pasujące miejsca.
- Wpisz `xyzxyz` – komunikat „Nie znaleziono miasta”.
- Wyłącz Wi‑Fi i kliknij Szukaj – komunikat „Błąd połączenia”, aplikacja działa dalej.
- Zmień `DefaultLanguage` na `en` w pliku `bin/Debug/net10.0-windows/appsettings.json` i uruchom `.exe` – nazwy krajów będą po angielsku, bez kompilacji.

### Dlaczego tak

Sekcja `AppSettings` z kluczem `DefaultLanguage`: w kodzie `configuration["AppSettings:DefaultLanguage"]`. Jutro zbindujemy całą sekcję do klasy `AppSettings`, w dniu 13 dojdzie `ApiBaseUrl` (adres naszego API), a w dniu 15 sekrety Azure trafią do User Secrets. Ten sam mechanizm działa w każdym typie aplikacji .NET.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `FileNotFoundException: appsettings.json` | brak **Copy to Output Directory** – sprawdź `.csproj` |
| `HttpRequestException: 400` przy pogodzie | współrzędne z przecinkiem – sprawdź `CultureInfo.InvariantCulture` |
| Lista pusta po Szukaj, bez komunikatu | sprawdź `DisplayMemberPath="FullName"` i czy `Results` jest `List<City>?` |

---

## 9. Podsumowanie

Dziś nauczyłeś się:

1. tworzyć solucję i projekty z CLI `dotnet`,
2. pobierać dane przez `HttpClient` i parsować je regexem,
3. budować okna WPF z kontenerów `DockPanel`, `Grid`, `StackPanel` i stylować je przez zasoby i `BasedOn`,
4. dlaczego wątek UI nie może czekać i jak `async/await`, `Task.Run`, `Task.WhenAll` i `Dispatcher` rozwiązują problem,
5. mapować JSON na klasy przez `System.Text.Json` i oddzielać DTO od modelu domenowego,
6. trzymać ustawienia w `appsettings.json`.

Jutro (dzień 12) przeniesiemy logikę z `MainWindow.xaml.cs` do **ViewModelu** (MVVM) i oddamy tworzenie obiektów kontenerowi **Dependency Injection**. Kod serwisu i modeli zostaje bez zmian.

### Zapisz pracę

```powershell
dotnet new gitignore
git init
git add .
git commit -m "Dzien 11: konsola, WPF (kontenery, style, async), klient Open-Meteo"
```

### Zadania dodatkowe

1. P01: pobieraj temperaturę raz i przeliczaj ją metodą `Convert`.
2. P02: dodaj przycisk „Anuluj” i `CancellationToken` do wariantu 7 (podpowiedź: `CancellationTokenSource`, `GetStringAsync(url, token)`).
3. P03: dodaj przycisk „Ulubione”, który zapisuje wybrane miasto do `favorites.json` (`JsonSerializer.Serialize`) i wczytuje listę przy starcie.

### Pytania do samodzielnego sprawdzenia

1. Dlaczego `HttpClient` jest `static`, a nie tworzony w `using` przy każdym zapytaniu?
2. Co się stanie, gdy w `ContinueWith` zmienisz kontrolkę bez `Dispatcher`?
3. Czym różni się wariant 2 od wariantu 3 pod względem czasu i dlaczego?
4. Po co `CultureInfo.InvariantCulture` przy `latitude.ToString()`?
5. Czym różni się styl z `x:Key` od stylu bez klucza?
6. Dlaczego `GeocodingResponse.Results` jest typu `List<City>?`?
