# Dzień 11: scenariusz prowadzenia (krok po kroku)

> **Technologia:** .NET 10 (LTS), C# 14, Visual Studio 2026, solucja `32Dzien11.slnx`.
> **Punkt startowy:** dni 1–10 to .NET Framework i Windows Forms. Od dziś pracujemy w **.NET (Core)**: wieloplatformowym, budowanym z linii poleceń, z nowym desktopem **WPF**.
> **Cel dnia:** trzy projekty, od najprostszego do „prawdziwej” aplikacji:
> **P01Pogoda** (konsola: HttpClient, regex, CLI) → **P02WeatherForecast** (WPF: kontenery, style, 7 wariantów async) → **P03WeatherForecastWPF.Client** (WPF: Open‑Meteo, JSON, konfiguracja).
> P03 będzie rozwijany przez kolejne dni (12–18), więc decyzje z dzisiaj zostają.

## Jak czytać ten scenariusz

| Znacznik | Znaczenie |
|---|---|
| 🗣️ **Mówię** | Tekst do powiedzenia uczestnikom, prawie dosłownie |
| ⌨️ **Robię** | Dokładne kliknięcia, nazwy i polecenia do wpisania |
| 👀 **Pokazuję** | Na co zwrócić uwagę na ekranie |
| 💡 **Wyjaśniam** | Szersze omówienie, co i dlaczego się dzieje |
| 🔧 **Ulepszenie** | Tu robimy **inaczej niż w edycjach 26/31**. Tamto działało, to jest poprawka lub nowsza praktyka |
| ✅ **Efekt** | Punkt kontrolny: co ma działać, zanim pójdziesz dalej |
| 🔗 **Przejście** | Jak połączyć ten temat z następnym |
| 🟩 **Co dopisujesz** | Podgląd zmian w istniejącym pliku: linie `+` dopisujesz, `-` usuwasz, `⋯` to pominięty kod. Pod spodem zawsze jest pełny plik |

Kod w blokach jest **gotowy do skopiowania w całości**. Nad blokiem jest ścieżka pliku: zastąp całą zawartość tego pliku. Znaków `+`/`-` z bloków 🟩 nie kopiuj.

## Plan dnia (orientacyjnie)

| Blok | Temat | Czas |
|---|---|---|
| 0 | Wstęp: .NET Framework a .NET, po co nam to, mapa dni 11–18 | 20 min |
| 1 | CLI `dotnet`: solucja i projekt z konsoli, P01 „Hello” | 25 min |
| 2 | P01: HttpClient, wttr.in, regex, przeliczanie jednostek | 50 min |
| 3 | P02: nowy projekt WPF, kontenery (DockPanel, Grid, StackPanel), menu, obrazek | 45 min |
| 4 | P02: style i zasoby (Application / Window, BasedOn) | 30 min |
| 5 | P02: serwis synchroniczny i zamrożone okno | 20 min |
| 6 | P02: siedem wariantów asynchronicznych | 75 min |
| 7 | P03: Open‑Meteo w przeglądarce, modele z JSON, serwis, UI | 75 min |
| 8 | P03: konfiguracja `appsettings.json` | 20 min |
| 9 | Podsumowanie, pytania, commit | 15 min |

Przerwy po blokach 2, 4 i 6.

## Dane testowe (używaj zawsze tych samych)

| Gdzie | Wartość |
|---|---|
| P01, miasto | `Warszawa`, `New York`, `Tokio`; błąd: `xyzxyz` (wttr.in zwraca tekst bez temperatury) |
| P02, lista miast | `Warszawa`, `Kraków`, `Gdańsk`, `Berlin`, `Paryż`, `Tokio` (ustawione w kodzie) |
| P03, miasto | `Warszawa` (pierwszy wynik: Warszawa, Województwo mazowieckie), `Kraków` (pokazuje też Krakow w Wisconsin – dobra okazja do rozmowy o geocodingu), `xyzxyz` (brak wyników) |

---

## 0. Przygotowanie (u siebie, przed startem)

⌨️ **Robię:**

1. Sprawdzam SDK w **Developer PowerShell** (Visual Studio: **View → Terminal**):

```powershell
dotnet --version
dotnet --list-sdks
```

Ma być `10.0.x`. Jeśli nie ma, instaluję .NET 10 SDK z https://dotnet.microsoft.com/download/dotnet/10.0 albo przez Visual Studio Installer (workload **.NET desktop development**).

2. Sprawdzam, że działają usługi, z których dziś korzystamy (w przeglądarce):
   - `https://wttr.in/Warszawa?format=%t&m` → np. `+9°C`
   - `https://geocoding-api.open-meteo.com/v1/search?name=Warszawa&count=3&language=pl&format=json` → JSON z listą miast
   - `https://api.open-meteo.com/v1/forecast?latitude=52.23&longitude=21.01&current=temperature_2m,wind_speed_10m,weather_code&timezone=auto` → JSON z polem `current`

3. Tworzę pusty folder `32Dzien11` i otwieram go w terminalu. Repozytorium GitHub `tomasz-trainer/32Dzien11` ma gotowy kod, ale na zajęciach piszemy od zera.

4. Mam pod ręką plik `Images/Logo.png` (dowolny obrazek PNG, w repozytorium jest w `P02WeatherForecast/Images`).

### 0.1 Wstęp dla uczestników (teoria, 15–20 min)

🗣️ **Mówię:**
„Przez 10 dni pracowaliśmy w .NET Framework: działa tylko na Windows, ma Windows Forms, a nowe wersje już nie powstają (4.8 jest ostatnia). Od dziś przechodzimy na **.NET**, kiedyś nazywany .NET Core. Co się zmienia?

1. **Wieloplatformowość**: ten sam kod działa na Windows, Linuksie, macOS, a z MAUI na Androidzie i iOS. Serwer może stać na Linuksie, bo jest tańszy.
2. **Narzędzia z linii poleceń**: `dotnet new`, `dotnet build`, `dotnet run`. Visual Studio to tylko wygodna nakładka. Dzięki temu da się budować na serwerze CI (GitHub Actions) albo w kontenerze Docker.
3. **Jedna wersja co rok**: parzyste są LTS (długie wsparcie, 3 lata). .NET 10 wyszedł w listopadzie 2025 i jest LTS, dlatego go używamy.
4. **Wydajność**: wielokrotnie szybszy od Frameworka, szczególnie w aplikacjach webowych.

Co **nie** działa tak samo? Część klas z Frameworka jest przestarzała, np. `WebClient`: istnieje, ale kompilator ostrzega, że mamy używać `HttpClient`. Windows Forms działa w .NET, ale nowym standardem desktopu jest **WPF** z językiem **XAML**, a ten sam XAML (z małymi różnicami) zobaczymy w **MAUI** na telefonie.

Plan na dni 11–18 wygląda tak:

| Dzień | Co budujemy |
|---|---|
| 11 (dziś) | konsola + WPF, pobieramy pogodę z internetu, programowanie asynchroniczne |
| 12 | MVVM i Dependency Injection w WPF |
| 13 | własne Web API (serwer) i klient WPF, który z niego korzysta |
| 14 | baza danych: Entity Framework Core, migracje, CRUD |
| 15 | chmura Azure: baza, API, rozpoznawanie mowy |
| 16 | aplikacje webowe: Razor Pages i MVC |
| 17 | Blazor WebAssembly: C# w przeglądarce |
| 18 | MAUI: aplikacja na Windows i Androida |

Od dnia 13 budujemy jeden system (sklep z produktami), więc kod z każdego dnia jest punktem startowym następnego”.

💡 **Wyjaśniam** (alternatywy dla desktopu, 3 min):

| Technologia | Zalety | Wady |
|---|---|---|
| Windows Forms | prosta, przeciągnij i upuść | tylko Windows, stary model, trudne stylowanie |
| **WPF** | XAML, style, binding, MVVM, wektorowa grafika | tylko Windows |
| WinUI 3 | nowoczesny wygląd Windows 11 | młodszy ekosystem, tylko Windows |
| Avalonia | WPF‑podobny, wieloplatformowy | firma trzecia (choć stabilna) |
| **MAUI** | jeden kod na Windows, Android, iOS, macOS | mniej kontrolek desktopowych niż WPF |

„Uczymy się WPF, bo XAML i MVVM z WPF przenoszą się 1:1 na MAUI (dzień 18) i są zrozumiałe w Avalonii czy WinUI”.

🔗 **Przejście:** „Zaczynamy od konsoli, bo chcę pokazać, że w .NET nie potrzebujemy Visual Studio, żeby stworzyć i uruchomić projekt”.

---

## 1. CLI `dotnet`: solucja i projekt z konsoli

**Cel:** uczestnicy potrafią stworzyć solucję i projekt bez kreatora i wiedzą, co robi każde polecenie.

⌨️ **Robię** w terminalu, w folderze `32Dzien11`:

```powershell
dotnet new sln -n 32Dzien11 --format slnx
dotnet new console -n P01Pogoda -o P01Pogoda -f net10.0
dotnet sln 32Dzien11.slnx add P01Pogoda/P01Pogoda.csproj
dotnet build
dotnet run --project P01Pogoda
```

👀 **Pokazuję** po kolei:

| Polecenie | Co mówię |
|---|---|
| `dotnet new sln --format slnx` | „Solucja to spis projektów. Format `.slnx` to nowy, czytelny XML zamiast starego `.sln`; Visual Studio 2026 obsługuje go domyślnie” |
| `dotnet new console -n … -o … -f net10.0` | „Szablon `console`, nazwa, folder, wersja frameworka. Lista szablonów: `dotnet new list`” |
| `dotnet sln add` | „Dopisuje projekt do solucji. Bez tego Visual Studio nie pokaże projektu” |
| `dotnet build` / `dotnet run` | „Kompilacja i uruchomienie. `run` sam robi `build`” |

⌨️ **Robię:** otwieram `32Dzien11.slnx` w Visual Studio (dwuklik) i pokazuję, że projekt tam jest. Otwieram `P01Pogoda/Program.cs`.

💡 **Wyjaśniam:** „Zobaczcie, że nie ma klasy `Program` ani metody `Main`. To **top‑level statements**: kompilator sam dokłada klasę i `Main`. Działa od C# 9 i jest domyślne w szablonach. Możemy pisać `await` na najwyższym poziomie, bo kompilator robi z tego `static async Task Main`”.

👀 **Pokazuję** plik `P01Pogoda.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

🗣️ **Mówię:** „Plik projektu to kilka linii. `ImplicitUsings` dokłada automatycznie `using System;`, `System.Linq`, `System.Net.Http` i kilka innych. `Nullable` włącza ostrzeżenia, gdy coś może być `null`; to nie błąd, ale kompilator podpowiada, gdzie sprawdzić wartość. W .NET Framework ten plik miał 100 linii i listę każdego pliku .cs”.

✅ **Efekt:** `dotnet run --project P01Pogoda` wypisuje `Hello, World!`.

🔗 **Przejście:** „Zamiast Hello World pobierzemy temperaturę z internetu. Najpierw bez żadnego JSON‑a: usługa `wttr.in` zwraca zwykły tekst”.

---

## 2. P01: HttpClient, wttr.in, regex, jednostki

**Cel:** pobranie tekstu z HTTP, wyciągnięcie liczby wyrażeniem regularnym, enum zamiast „magicznych” napisów, obsługa wyjątków.

⌨️ **Robię:** w przeglądarce otwieram `https://wttr.in/Warszawa?format=%t&m`.

👀 **Pokazuję:** odpowiedź to np. `+9°C`. Zmieniam na `https://wttr.in/Warszawa` (bez `format`), żeby pokazać pełną „ASCII‑artową” prognozę. „`format=%t` to tylko temperatura, `&m` wymusza stopnie Celsjusza (bez tego usługa zgaduje po adresie IP i potrafi zwrócić Fahrenheity)”.

### 2.1 Klasa `WeatherManager`

⌨️ **Robię:** prawy przycisk na projekcie P01Pogoda → **Add → Class…** → `WeatherManager.cs`.

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

💡 **Wyjaśniam** po kolei, pokazując kursorem:

1. **`HttpClient` jako `static readonly`**: „W .NET Framework używaliśmy `WebClient`. W .NET jest oznaczony jako przestarzały. `HttpClient` jest szybszy i asynchroniczny, ale ma pułapkę: jeśli tworzymy go w `using` przy każdym zapytaniu, system zostaje z setkami otwartych gniazd. Dlatego jeden na aplikację (jutro zrobi to za nas kontener DI)”.
2. **Regex**: `[+-]?` opcjonalny znak, `\d+` co najmniej jedna cyfra, `(\.\d+)?` opcjonalna część dziesiętna, `\s*°C` końcówka. `(?<value>…)` to **grupa nazwana**; zamiast `Groups[1]` piszemy `Groups["value"]`. `RegexOptions.Compiled` kompiluje wzorzec raz; opłaca się, bo używamy go wiele razy.
3. **`CultureInfo.InvariantCulture`**: „Na polskim Windowsie `double.Parse("12.5")` rzuci wyjątek, bo oczekuje przecinka. Dane z internetu zawsze parsujemy kulturą niezmienną”.
4. **Enum i `switch` jako wyrażenie**: „Zamiast `if (unit == "C")` mamy typ, w którym nie da się zrobić literówki, i jedno wyrażenie zamiast drabinki `if/else`”.

⌨️ **Robię:** otwieram https://regex101.com, wklejam wzorzec `(?<value>[+-]?\d+(\.\d+)?)\s*°C` i teksty `+12°C`, `-3°C`, `Unknown location`. 👀 **Pokazuję**, co się dopasowuje, a co nie.

🔧 **Ulepszenie:** w edycjach 26/31 używaliśmy `WebClient` i scrapowania strony Google po klasie CSS (`BNeawe iBp4i AP7Wnd`), co przestawało działać po każdej zmianie HTML Google. `wttr.in` ma stabilny format tekstowy, a `HttpClient` to obecny standard.

### 2.2 `Program.cs`

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

💡 **Wyjaśniam:** „`Console.OutputEncoding = UTF8` jest po to, żeby znak `°` i polskie litery wyświetliły się poprawnie. Dwa bloki `catch`: inny komunikat, gdy nie ma internetu, inny, gdy usługa odpowiedziała, ale nie tym, czego oczekujemy. Trzy wywołania pod rząd są celowo rozrzutne (trzy zapytania HTTP o to samo): zapytaj grupę, jak to poprawić. Odpowiedź: pobrać raz w Celsjuszach i przeliczyć `WeatherManager.Convert`”.

⌨️ **Robię:** `dotnet run --project P01Pogoda`, wpisuję `Warszawa`, `New York`, `xyzxyz`.

✅ **Efekt:** dla miast widać trzy wartości, dla `xyzxyz` komunikat „Nie udało się odczytać temperatury” (wttr.in zwraca tekst bez `°C`) albo błąd HTTP 404.

🔗 **Przejście:** „W konsoli nie widać problemu, który jest najważniejszy w aplikacjach okienkowych: co się dzieje z oknem, gdy czekamy na internet. Przechodzimy do WPF”.

---

## 3. P02: nowy projekt WPF, kontenery, menu, obrazek

**Cel:** uczestnicy rozumieją, jak XAML buduje drzewo kontrolek i jak działają trzy podstawowe kontenery.

⌨️ **Robię:**

1. Prawy przycisk na solucji → **Add → New Project…** → **WPF Application** (C#), nazwa `P02WeatherForecast`, framework **.NET 10.0** → **Create**. (Z konsoli: `dotnet new wpf -n P02WeatherForecast -o P02WeatherForecast -f net10.0` i `dotnet sln 32Dzien11.slnx add P02WeatherForecast/P02WeatherForecast.csproj`.)
2. F5 (ustawiam P02 jako **Set as Startup Project**). Pokazuję puste okno.
3. Tworzę folder `Images`, kopiuję do niego `Logo.png`. Zaznaczam plik w Solution Explorer → **Properties** → **Build Action: Resource**.

👀 **Pokazuję** w `P02WeatherForecast.csproj` (po ustawieniu Build Action):

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

💡 **Wyjaśniam:** „`net10.0-windows`, bo WPF działa tylko na Windows. `Resource` wbudowuje obrazek do pliku `.exe`; alternatywa `Content` + `Copy to Output Directory` kopiuje go obok `.exe`, co pozwala podmienić obrazek bez kompilacji. W edycji 26 używaliśmy `Content`, dziś `Resource`: jeden plik do dystrybucji”.

### 3.1 Układ okna

⌨️ **Robię:** zastępuję `MainWindow.xaml` (na razie bez stylów; wklejam całość i omawiam):

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

> ⚠️ Po wklejeniu Visual Studio podkreśli `Click="…"` i `StaticResource BaseControlStyle` na czerwono: metod i stylów jeszcze nie ma. Dopiszemy je w 3.2 i 4. Jeśli wolisz budować stopniowo, wklej najpierw sam `DockPanel` z `Menu`, `Image` i `Grid` z pustymi `StackPanel`, a przyciski dodawaj w bloku 6.

💡 **Wyjaśniam** trzy kontenery (rysuję na tablicy):

| Kontener | Zasada | Kiedy |
|---|---|---|
| `DockPanel` | „magnes”: `Dock=Top/Left/…`, ostatnie dziecko wypełnia resztę | szkielet okna: menu u góry, panel z boku |
| `Grid` | wiersze i kolumny; `Auto` = tyle, ile treść, `*` = reszta, `3*` = trzy części | formularze, układy proporcjonalne |
| `StackPanel` | jeden pod drugim (lub obok siebie: `Orientation="Horizontal"`) | listy przycisków, pola formularza |

„Kontenery można zagnieżdżać bez ograniczeń. `Grid.ColumnSpan="2"` rozciąga element na dwie kolumny. `x:Name` daje nam pole w C#, przez które dostajemy się do kontrolki z kodu. `_` w `Header="_Plik"` to skrót klawiszowy Alt+P”.

👀 **Pokazuję:** okno **Document Outline** (View → Other Windows) z drzewem kontrolek. Zmieniam `Width="3*"` na `Width="*"` i pokazuję, jak przesuwa się granica.

### 3.2 Code‑behind z pustymi metodami

⌨️ **Robię:** na razie tylko menu, żeby projekt się kompilował:

```csharp
// P02WeatherForecast/MainWindow.xaml.cs  (wersja tymczasowa – pełna w bloku 6)
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

💡 **Wyjaśniam:** „`partial class`: druga połowa klasy jest generowana z XAML (`InitializeComponent`, pola `txtCities`, `lvLog`). Dlatego nazwy z `x:Name` działają w C#. `namespace P02WeatherForecast;` ze średnikiem to **file‑scoped namespace** (C# 10): jedno wcięcie mniej”.

🔗 **Przejście:** „Zanim dodamy style, projekt się nie zbuduje, bo XAML odwołuje się do `BaseControlStyle`. Style to następny temat”.

---

## 4. P02: style i zasoby

**Cel:** zasoby aplikacji a zasoby okna, styl bez klucza (implicit), styl z kluczem, dziedziczenie `BasedOn`.

⌨️ **Robię:** zastępuję `App.xaml`:

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

💡 **Wyjaśniam:**

| Element | Co mówię |
|---|---|
| `Application.Resources` | „Słownik zasobów na poziomie aplikacji: dostępny w każdym oknie. `Window.Resources` działa tylko w jednym oknie i ma pierwszeństwo przy tym samym kluczu” |
| `Style TargetType="Button"` **bez** `x:Key` | „Styl niejawny: WPF stosuje go do każdego `Button`, chyba że przycisk dostanie własny `Style=`. Wszystkie nasze przyciski są jasnoniebieskie bez jednej linii w oknie” |
| `x:Key="BaseControlStyle"` z `TargetType="Control"` | „Styl z kluczem trzeba wskazać ręcznie: `Style="{StaticResource BaseControlStyle}"`. `Control` to przodek większości kontrolek, więc jeden styl pasuje do TextBox, ListView i Button” |
| `BasedOn` | „Dziedziczenie jak w klasach: `AccentButtonStyle` bierze czcionkę i margines z bazowego i dokłada kolory. Zmiana w bazowym zmienia wszystkie pochodne” |
| `StaticResource` | „Odczyt zasobu raz, przy ładowaniu. Jest jeszcze `DynamicResource`, który śledzi zmiany (np. motyw ciemny/jasny)” |

👀 **Pokazuję:** uruchamiam (F5). Wszystkie przyciski jasnoniebieskie, przycisk 7 ciemnoniebieski z białym tekstem. Zmieniam `PrimaryBrush` na `#C0392B` i pokazuję, że zmieniły się wszystkie elementy, które go używają.

✅ **Efekt:** okno z menu, logo z lewej, polem miast, siedmioma przyciskami i dwiema ramkami. Menu **Pomoc → O programie** pokazuje komunikat.

🔗 **Przejście:** „Mamy wygląd, pora na logikę. Najpierw napiszemy ją tak, jak robiliśmy to w Windows Forms: synchronicznie. I zobaczymy, dlaczego to zły pomysł”.

---

## 5. P02: serwis synchroniczny i zamrożone okno

**Cel:** uczestnicy *widzą* problem blokowania wątku interfejsu.

⌨️ **Robię:** folder `Services` → klasa `WeatherForecastService.cs`:

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

💡 **Wyjaśniam:** „Trzy metody, ten sam wynik. Pierwsza blokuje wątek (`Send` bez `Async`). Druga używa gotowej metody asynchronicznej z biblioteki: podczas czekania na serwer **żaden** wątek nie jest zajęty. Trzecia to wyjście awaryjne, gdy biblioteka jest stara i ma tylko wersję synchroniczną: `Task.Run` bierze wątek z puli i tam blokuje. Na razie używamy tylko pierwszej”.

⌨️ **Robię:** w `MainWindow.xaml.cs` dopisuję pole i metodę `btnSync_Click` (w bloku 6 wkleimy pełny plik, tu pokazuję sam fragment):

```csharp
private readonly WeatherForecastService _service = new();

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

⌨️ **Robię:** F5, klikam **1. Synchronicznie**, a w trakcie próbuję przesunąć okno i kliknąć menu.

👀 **Pokazuję:** okno nie reaguje, tytuł dostaje „(Brak odpowiedzi)”, a w dzienniku **nie** pojawiają się kolejne „Pobieram…”; wszystko wyskakuje naraz na końcu.

💡 **Wyjaśniam** (rysuję na tablicy): „WPF ma **jeden wątek interfejsu** (UI thread). On rysuje okno, obsługuje kliknięcia i wykonuje nasz `btnSync_Click`. Gdy w środku czekamy 2 sekundy na serwer, przez 2 sekundy nikt nie rysuje okna. `lvLog.Items.Add` dodał element, ale wątek nie miał czasu go narysować. Rozwiązanie: nie czekać w wątku UI”.

🔗 **Przejście:** „Teraz siedem… a właściwie sześć wariantów asynchronicznych. Pokażę je od najbardziej ręcznego do zalecanego, żebyście rozumieli, co `async/await` robi pod spodem”.

---

## 6. P02: warianty asynchroniczne

**Cel:** `async/await`, `Task.Run`, `Task.WhenAll`, krotki, `Dispatcher`, obsługa postępu i wyjątków.

⌨️ **Robię:** zastępuję cały `MainWindow.xaml.cs`:

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

Omawiam każdy wariant **po uruchomieniu**, klikając przycisk i patrząc na dziennik (czasy w ms są ważne):

### 6.1 Wariant 2: `Task.Run` + `await`

🗣️ **Mówię:** „`async void` w nazwie obsługi zdarzenia: jedyny przypadek, gdy `async void` jest dozwolone (bo WPF wymaga `void`). W środku `await Task.Run(…)`. `Task.Run` wysyła pracę do wątku z puli. `await` mówi: »zwróć wątek UI, a gdy zadanie się skończy, wróć do tej linii«. Dlatego okno reaguje, a w dzienniku widzimy wpisy na bieżąco. Ale czas łączny jest podobny do synchronicznego, bo miasta idą jedno po drugim”.

👀 **Pokazuję:** w trakcie przesuwam okno. Działa. Czas w dzienniku ≈ suma czasów.

### 6.2 Wariant 3: `Task.WhenAll`

🗣️ **Mówię:** „Tworzymy wszystkie zadania w pętli **bez** `await`: startują równolegle. Dopiero `await Task.WhenAll(tasks)` czeka na komplet. Czas ≈ najwolniejsze miasto. Problem: `Task<double>` zwraca tylko liczbę; nie wiemy, które miasto. Stąd `(miasto?)` w wynikach”.

👀 **Pokazuję:** czas w dzienniku jest kilka razy krótszy niż w wariancie 2.

### 6.3 Wariant 4: krotka

🗣️ **Mówię:** „Zadanie może zwrócić krotkę `(City, Temperature)`. `Task.WhenAll` zwraca tablicę krotek, a w pętli rozpakowujemy ją `foreach (var (city, temperature) in results)`. To ten sam wariant, co 3, ale z pełną informacją”.

### 6.4 Wariant 5: `ContinueWith` i `Dispatcher`

🗣️ **Mówię:** „Tu nie czekamy na nikogo. `ContinueWith` rejestruje kod, który wykona się po zakończeniu zadania, ale w **wątku z puli**. Gdybyśmy tam napisali `tbResults.Text += …`, dostaniemy `InvalidOperationException: The calling thread cannot access this object`. Dlatego `Dispatcher.Invoke` przekazuje ten kawałek do wątku UI. Zwróćcie uwagę na kolejność: wyniki pojawiają się w kolejności **odpowiedzi serwera**, nie w kolejności listy”.

⌨️ **Robię:** na chwilę usuwam `Dispatcher.Invoke(() => { … })` zostawiając samo `AppendResult`, uruchamiam, pokazuję wyjątek, przywracam.

💡 **Wyjaśniam:** „W praktyce `ContinueWith` prawie nie piszemy: `await` robi dokładnie to samo i sam wraca na wątek UI. Pokazuję go, żebyście wiedzieli, co `await` robi za was”.

### 6.5 Wariant 6: pasek postępu

🗣️ **Mówię:** „Sekwencyjnie jak w wariancie 2, ale z `ProgressBar`. Tutaj używamy `GetTemperatureWrappedAsync`: serwis sam opakowuje kod synchroniczny w `Task.Run`, więc okno nie musi wiedzieć, jak to zrobiono”.

### 6.6 Wariant 7: gotowa metoda async (zalecane)

🗣️ **Mówię:** „To jest wzorzec, którego używamy od jutra do końca kursu. `GetTemperatureAsync` korzysta z `GetStringAsync` z `HttpClient`. Podczas czekania na serwer **żaden wątek nie jest zajęty** (operacja I/O, system operacyjny powiadomi nas, gdy dane przyjdą). Nie ma `Task.Run`, nie ma `Dispatcher`. Jest `try/catch/finally`: wyjątek z metody asynchronicznej wraca do linii z `await`, więc łapiemy go zwykłym `catch`. `finally` odblokowuje przycisk, nawet gdy był błąd”.

⌨️ **Robię:** dopisuję do listy miast `xyzxyz` i klikam 7. 👀 **Pokazuję:** w dzienniku „Błąd: …”, przycisk znów aktywny.

💡 **Wyjaśniam** (podsumowanie na tablicy):

| Sytuacja | Co robimy |
|---|---|
| Biblioteka ma metodę `…Async` | `await` i nic więcej (wariant 7) |
| Biblioteka ma tylko metodę synchroniczną albo liczymy coś ciężkiego (CPU) | `await Task.Run(…)` (warianty 2, 6) |
| Wiele niezależnych operacji | uruchom wszystkie, potem `await Task.WhenAll` (warianty 3, 4) |
| Kod z innego wątku ma dotknąć kontrolki | `Dispatcher.Invoke` (wariant 5), ale lepiej `await` |

🔧 **Ulepszenie:** względem edycji 26/31: zamiast `t.GetAwaiter().OnCompleted` jest czytelniejsze `ContinueWith` z `IsCompletedSuccessfully`; każdy wariant mierzy czas (`Stopwatch`), żeby różnica była widoczna w liczbach; wariant zalecany ma `try/catch/finally` i blokadę przycisku; `Split` z `TrimEntries` usuwa puste linie.

✅ **Efekt:** wszystkie siedem przycisków działa. W wariantach 3 i 4 czas jest wyraźnie krótszy niż w 1 i 2.

🔗 **Przejście:** „Usługa, która zwraca tekst `+9°C`, jest dobra do nauki, ale prawdziwe API zwracają **JSON**. W P03 zbudujemy klienta Open‑Meteo, którego będziemy rozwijać przez następne dni”.

---

## 7. P03: klient Open‑Meteo

**Cel:** czytanie dokumentacji API, modele z JSON, `System.Text.Json`, serwis z `HttpClient`, dwuetapowe wyszukiwanie (miasto → współrzędne → pogoda).

### 7.1 API w przeglądarce

⌨️ **Robię:** otwieram po kolei:

1. `https://geocoding-api.open-meteo.com/v1/search?name=Warszawa&count=10&language=pl&format=json`
2. `https://api.open-meteo.com/v1/forecast?latitude=52.23&longitude=21.01&current=temperature_2m,wind_speed_10m,weather_code&timezone=auto`
3. Dokumentację: https://open-meteo.com/en/docs (sekcja „Current Weather”) i https://open-meteo.com/en/docs/geocoding-api

🗣️ **Mówię:** „Open‑Meteo jest darmowe do użytku niekomercyjnego i **nie wymaga klucza**, dlatego każdy z was może go używać od razu. Pogoda wymaga współrzędnych, więc najpierw pytamy geocoding o miasto, a potem prognozę o współrzędne. Zobaczcie nazwy pól: `country_code`, `temperature_2m`. W C# nazwy właściwości piszemy `CountryCode`, więc trzeba je zmapować”.

💡 **Wyjaśniam** (alternatywy): „AccuWeather i OpenWeatherMap wymagają rejestracji i klucza API. Klucz to sekret: nie może trafić do repozytorium. W dniu 15 pokażę User Secrets przy kluczach Azure. Dziś celowo API bez klucza”.

### 7.2 Nowy projekt

⌨️ **Robię:** **Add → New Project → WPF Application** → `P03WeatherForecastWPF.Client`, .NET 10. Tworzę foldery `Models` i `Services`.

🗣️ **Mówię:** „Nazwa z `.Client` nie jest przypadkowa: w dniu 13 obok pojawi się `P05Shop.API`, czyli serwer, a ten projekt będzie jego klientem. Projekt P03 zostaje z nami do dnia 18”.

### 7.3 Modele z JSON

⌨️ **Robię:** pokazuję narzędzie **Edit → Paste Special → Paste JSON As Classes** w Visual Studio (wklejam JSON z geocodingu). 👀 **Pokazuję**, że generuje klasy `Rootobject`, `Result` z polami `country_code` jako nazwami właściwości.

🗣️ **Mówię:** „To dobry start, ale nazwy w stylu `country_code` łamią konwencje C#. Zrobimy to porządnie: PascalCase plus atrybut `[JsonPropertyName]`. Mapujemy tylko pola, których używamy; reszta JSON‑a jest po prostu ignorowana”.

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

💡 **Wyjaśniam:**

- „Dwa rodzaje modeli: `GeocodingResponse`/`ForecastResponse` to **kształt JSON‑a dostawcy** (DTO). `Weather` to **nasz model**: to, co aplikacja chce pokazać. Dzięki temu, gdy zmienimy dostawcę pogody, zmienimy tylko serwis, a okno i (od jutra) ViewModel zostają”.
- „`string?` i `List<City>?`: pytajnik mówi »może być null«. `results` nie ma w JSON‑ie, gdy nic nie znaleziono, więc `Results` musi być nullable. `Name` bez pytajnika ma wartość domyślną `string.Empty`”.
- „`[JsonIgnore]` na `FullName`: to właściwość tylko do wyświetlania, nie chcemy, żeby serializator jej szukał w JSON‑ie”.
- „`switch` z `or` (C# 9 pattern matching) zamiast dziesięciu `case`”.

🔧 **Ulepszenie:** edycje 26/31 używały Newtonsoft.Json. W .NET wbudowany `System.Text.Json` jest szybszy i nie wymaga pakietu; `GetFromJsonAsync` z `System.Net.Http.Json` robi pobranie i deserializację w jednej linii.

### 7.4 Serwis

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

💡 **Wyjaśniam:**

- „`HttpClient` wchodzi **konstruktorem**. Serwis nie tworzy go sam; dostaje gotowy. To już jest wstrzykiwanie zależności, tylko ręczne. Jutro zrobi to za nas kontener DI”.
- „`?? []` (C# 12 collection expression): gdy odpowiedź albo lista jest `null`, zwracamy pustą tablicę. Okno nigdy nie dostanie `null`”.
- „`latitude.ToString(CultureInfo.InvariantCulture)`: na polskim Windowsie `52.23.ToString()` da `52,23` i serwer odpowie błędem 400. To najczęstszy błąd na tym kursie”.
- „Serwis zwraca `Weather?`: `null` znaczy »brak danych«. Okno decyduje, co z tym zrobić”.

### 7.5 Okno

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

💡 **Wyjaśniam:** „`DisplayMemberPath="FullName"`: lista dostanie obiekty `City`, a wyświetli właściwość `FullName`. Bez tego zobaczylibyśmy nazwę klasy. `SelectionChanged` odpala się po kliknięciu elementu”.

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

> Ten plik korzysta z `ConfigurationBuilder`, którego dodamy w bloku 8. Jeśli chcesz uruchomić P03 przed blokiem 8, zastąp na chwilę trzy linie konfiguracji przez `string language = "pl";`.

💡 **Wyjaśniam:** „`lbCities.SelectedItem is not City selectedCity`: pattern matching z deklaracją zmiennej; gdy nic nie zaznaczono (np. po odświeżeniu listy), wychodzimy. `ItemsSource` zamiast `Items.Add` w pętli: podajemy całą kolekcję; jutro to samo zrobimy bindingiem bez jednej linii C#”.

🔗 **Przejście:** „Język `pl` jest wpisany… a właściwie nie jest, bo czytamy go z pliku. Zobaczmy skąd”.

---

## 8. P03: konfiguracja `appsettings.json`

**Cel:** ustawienia poza kodem, pakiet NuGet z konsoli, kopiowanie pliku do katalogu wyjściowego.

⌨️ **Robię:**

1. Pakiet: prawy przycisk na P03 → **Manage NuGet Packages** → `Microsoft.Extensions.Configuration.Json` (wersja 10.0.x). Z konsoli: `dotnet add P03WeatherForecastWPF.Client package Microsoft.Extensions.Configuration.Json`.
2. Nowy plik `appsettings.json` w projekcie P03 → **Properties → Copy to Output Directory: Copy if newer**.

```json
// P03WeatherForecastWPF.Client/appsettings.json
{
  "AppSettings": {
    "DefaultLanguage": "pl"
  }
}
```

👀 **Pokazuję** `P03WeatherForecastWPF.Client.csproj`:

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

💡 **Wyjaśniam:** „Sekcja `AppSettings` z kluczem `DefaultLanguage`; w kodzie czytamy `configuration["AppSettings:DefaultLanguage"]`, dwukropek to separator poziomów. Jutro zamiast tekstowego klucza zbindujemy całą sekcję do klasy `AppSettings`, a w dniu 13 dojdzie `ApiBaseUrl`. Ten sam system konfiguracji działa w API, Blazorze i MAUI, więc uczymy się go raz. Plik **nie** jest wbudowany w exe: po zmianie języka na `en` w folderze `bin` nie trzeba kompilować”.

🔧 **Ulepszenie:** edycja 31 miała `appsettings.json` podpięty pod generator `SettingsSingleFileGenerator` (relikt z .NET Framework, tworzył `appsettings.Designer.cs`) i czytała go `AddUserSecrets<App>()` bez zarejestrowanego `UserSecretsId`. Tu jest czysto: tylko `AddJsonFile`. User Secrets dołożymy, gdy będzie sekret (dzień 15).

⌨️ **Robię:** F5 na P03. Wpisuję `Warszawa` → **Szukaj** → klikam pierwszy wynik. Potem `Kraków` (zwracam uwagę na Krakow w Wisconsin). Potem `xyzxyz`. Na koniec wyłączam Wi‑Fi (albo zmieniam adres w serwisie na `https://nie-ma-takiej-domeny.invalid`) i klikam Szukaj: komunikat „Błąd połączenia”.

✅ **Efekt:** po wyborze miasta widać temperaturę, wiatr, opis i czas. Dla `xyzxyz` komunikat „Nie znaleziono”. Bez sieci komunikat o błędzie zamiast wyjątku.

---

## 9. Podsumowanie dnia, pytania i commit

🗣️ **Mówię** (podsumowanie, 5 min):
„Dziś: (1) .NET zamiast .NET Framework i CLI `dotnet`, (2) `HttpClient` i regex, (3) WPF: `DockPanel`, `Grid`, `StackPanel`, style w `App.xaml` i `BasedOn`, (4) jeden wątek UI i dlaczego `await` ratuje okno, (5) JSON do klas z `System.Text.Json`, (6) konfiguracja w `appsettings.json`. Jutro odseparujemy logikę od okna: MVVM, binding i Dependency Injection. Kod P03 zostaje, zmieni się tylko miejsce, gdzie leży logika”.

**Pytania kontrolne** (zadaj grupie):

1. Dlaczego `HttpClient` jest `static`, a nie tworzony w `using` przy każdym zapytaniu?
2. Co się stanie, gdy w `ContinueWith` zapiszemy do `tbResults.Text` bez `Dispatcher`?
3. Czym różni się wariant 2 (`Task.Run` w pętli z `await`) od wariantu 3 (`Task.WhenAll`) pod względem czasu?
4. Po co `CultureInfo.InvariantCulture` przy `latitude.ToString()`?
5. Czym różni się styl z `x:Key` od stylu bez klucza?
6. Dlaczego `GeocodingResponse.Results` jest `List<City>?`, a nie `List<City>`?

⌨️ **Robię:** commit i push:

```powershell
git init
git add .
git commit -m "Dzien 11: konsola, WPF (kontenery, style, async), klient Open-Meteo"
git branch -M master
git remote add origin https://github.com/tomasz-trainer/32Dzien11.git
git push -u origin master
```

Plik `.gitignore` dla Visual Studio: `dotnet new gitignore` (ignoruje `bin/`, `obj/`, `.vs/`).

**Zadanie domowe** (opcjonalne): w P03 dodaj przycisk „Ulubione”, który zapisuje wybrane miasto do pliku `favorites.json` (`System.Text.Json.JsonSerializer.Serialize`) i wczytuje listę przy starcie.

---

## Dodatek A: ulepszenia względem edycji 26 i 31

| Gdzie | Edycje 26/31 | Edycja 32 | Dlaczego |
|---|---|---|---|
| Platforma | 26: .NET 8, `.sln`; 31: .NET 10, `.slnx` | .NET 10 LTS, `.slnx`, pakiety 10.0.x | aktualne LTS, format solucji VS 2026 |
| Pobieranie HTTP | `WebClient` (przestarzały, ostrzeżenie SYSLIB0014) | `HttpClient`, jeden na aplikację | zalecenie Microsoft, wydajność |
| Źródło temperatury w P01 | 26: scrapowanie Google po klasie CSS; 31: wttr.in bez `&m` | wttr.in z `?format=%t&m` | stabilne, zawsze Celsjusze |
| Parsowanie | `Replace("+","").Replace("°C","")` | regex z grupą nazwaną, `InvariantCulture`, `FormatException` | czytelny błąd zamiast wyjątku konwersji |
| Jednostki | napisy `"C"`, `"F"`, `"K"` | `enum TemperatureUnit` + `switch` wyrażenie | bezpieczeństwo typów |
| P01 | klasa `Program` + `Main` lub `ConsoleApp1` bez treści | top‑level statements, `await` na najwyższym poziomie, dwa `catch` | nowoczesny szablon |
| P02 obrazek | `Content` + `Copy always` | `Resource` | jeden plik exe |
| P02 style | tylko `Window.Resources` | `Application.Resources`, styl niejawny, `BasedOn`, pędzle jako zasoby | pełen obraz systemu stylów |
| P02 wariant „od razu gdy gotowy” | `GetAwaiter().OnCompleted` | `ContinueWith` + `IsCompletedSuccessfully` + `Dispatcher.Invoke` | widać obsługę błędu |
| P02 wariant zalecany | brak obsługi błędów | `try/catch/finally`, blokada przycisku, `Stopwatch` w każdym wariancie | różnice czasowe widać w liczbach |
| P02 metoda sync | `WebClient.DownloadString` | `HttpClient.Send` | ten sam klient w obu wariantach |
| P03 JSON | Newtonsoft.Json, 18 pól w `City` | `System.Text.Json`, `GetFromJsonAsync`, tylko używane pola | bez dodatkowego pakietu |
| P03 model | `Weather` z `Time` jako `string` | `Weather` z `DateTime`, wiatr, kod WMO, opis | model domenowy oddzielony od DTO |
| P03 współrzędne | `latitude.ToString(InvariantCulture)` (31) | to samo + komentarz dlaczego | najczęstszy błąd kursantów |
| P03 serwis | tworzy `HttpClient` w `using` przy każdym zapytaniu, sam czyta konfigurację | `HttpClient` i język przez konstruktor | gotowe pod DI w dniu 12 |
| P03 konfiguracja | 31: `appsettings.json` z generatorem `SettingsSingleFileGenerator`, `AddUserSecrets` bez `UserSecretsId` | `Microsoft.Extensions.Configuration.Json`, sekcja `AppSettings`, `PreserveNewest` | ten sam mechanizm w API/Blazor/MAUI |
| P03 błędy | wyjątek `HttpRequestException` zamykał aplikację | komunikaty w `tbStatus`, blokada przycisku | aplikacja nie wywraca się bez sieci |
| P04 konsola (31) | kopia serwisu w osobnym projekcie | usunięta: serwis jest w P03, test robimy w oknie | mniej duplikatów |

## Dodatek B: jak zweryfikowano scenariusz

- Wszystkie trzy projekty zbudowano z .NET SDK 10.0.401 (`dotnet build 32Dzien11.slnx`), bez błędów i ostrzeżeń.
- P01 uruchomiono z konsoli: `Warszawa` → `9.0 °C | 48.2 °F | 282.1 K`, `New York` → `14.0 °C`.
- Logikę `OpenMeteoService` (geocoding, pogoda, brak wyników) przetestowano w osobnym programie konsolowym na tych samych plikach `Models/` i `Services/`: `Kraków` zwraca Kraków (Polska), Krakow (Wisconsin), Krakow am See (Niemcy); `xyzxyzxyz` zwraca pustą tablicę; pogoda mapuje się na `Weather` z opisem.
- Okien WPF nie uruchamiano (środowisko bez Windows). Przejdź bloki 3–8 przed zajęciami: w szczególności sprawdź, że `Logo.png` wyświetla się po ustawieniu **Build Action: Resource**, a wariant 1 faktycznie zamraża okno (gdy wttr.in odpowiada szybko, dodaj więcej miast).
