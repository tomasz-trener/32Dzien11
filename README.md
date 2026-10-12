# 32Dzien11 – .NET 10: konsola, WPF, async, Open‑Meteo

Edycja 32 kursu C#/.NET, dzień 11. Kod końcowy dnia oraz dwie instrukcje krok po kroku.

| Plik | Dla kogo |
|---|---|
| [INSTRUKCJA_PROWADZACY.md](INSTRUKCJA_PROWADZACY.md) | trener: co mówić, co klikać, co pokazywać (🗣️ ⌨️ 👀 💡) |
| [INSTRUKCJA_STUDENT.md](INSTRUKCJA_STUDENT.md) | uczestnik: teoria w pigułce, kroki, punkty kontrolne, zadania |

## Projekty

| Projekt | Typ | Co pokazuje |
|---|---|---|
| `P01Pogoda` | konsola | CLI `dotnet`, `HttpClient`, wttr.in, regex, `enum`, obsługa błędów |
| `P02WeatherForecast` | WPF | kontenery (`DockPanel`, `Grid`, `StackPanel`), menu, obrazek, style i zasoby, 7 wariantów sync/async |
| `P03WeatherForecastWPF.Client` | WPF | klient Open‑Meteo (geocoding + pogoda), `System.Text.Json`, `appsettings.json`. **Rozwijany w dniach 12–18** |

## Wymagania

- .NET 10 SDK (`dotnet --version` → `10.0.x`)
- Visual Studio 2026 z workloadem **.NET desktop development** (WPF działa tylko na Windows)
- Internet: `wttr.in`, `open-meteo.com` (bez kluczy API)

## Uruchomienie

```powershell
dotnet build 32Dzien11.slnx
dotnet run --project P01Pogoda
dotnet run --project P02WeatherForecast
dotnet run --project P03WeatherForecastWPF.Client
```

Albo otwórz `32Dzien11.slnx` w Visual Studio i ustaw projekt startowy.

## Co dalej

Dzień 12 (`32Dzien12`): MVVM (CommunityToolkit.Mvvm), Dependency Injection (Generic Host), delegaty. Punktem startowym jest `P03WeatherForecastWPF.Client` z tego repozytorium.
