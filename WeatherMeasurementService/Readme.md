# WeatherMeasurementService

## 📝 Projektbeschreibung

Der `WeatherMeasurementService` ist ein Microservice zur Speicherung und Analyse von Wetterdaten der Wasserschutzpolizei Zürich.  
Die Anwendung bezieht externe Wetterdaten, speichert diese in einer lokalen Datenbank und stellt sie via REST-API zur Verfügung.

---

## ✅ Testaufgabe

Dieses Projekt wurde im Rahmen der **Testaufgabe für .NET Softwareentwickler (LS35.3)** der **Post CH AG** erstellt.  
Ziel war es, einen .NET REST-Service zu entwerfen, welcher Wetterdaten verarbeitet.

### Anforderungen laut Aufgabenstellung

- Daten aus externer API lesen
- Validierung, Filterung und Speicherung
- REST-Schnittstelle bereitstellen
- Unit-Tests
- Optional: Docker-Unterstützung

Die Aufgabenstellung ist in der Datei `Testaufgabe_Bewerber.pdf` enthalten.

---

## ▶️ Build & Start

### 🔧 Debug-Modus (lokale Entwicklung)

```bash
dotnet build WeatherMeasurementService -c Debug
dotnet run --project WeatherMeasurementService
```

### 🚀 Release-Modus

```bash
dotnet build WeatherMeasurementService -c Release
dotnet run --project WeatherMeasurementService -c Release
```

## 🧪 Build und Testen

```bash
dotnet build .\WeatherMeasurementService -c Release
dotnet test .\WeatherMeasurementService.Test\WeatherMeasurementService.Tests.csproj
```

---

## 🔌 REST API (Swagger)

### 🌐 Swagger URL

Die REST API ist mit Swagger UI dokumentiert und erreichbar unter:  
➡️ **[https://localhost:7017/swagger](https://localhost:7017/swagger/index.html)**

### 📦 Verfügbare API-Endpunkte

Die REST-API stellt folgende Endpunkte zur Verfügung:

| HTTP-Methode | Endpoint                            | Beschreibung                                            |
|--------------|-------------------------------------|---------------------------------------------------------|
| `GET`        | `/api/weatherdata/highest`          | Höchster Messwert eines Typs im Zeitraum               |
| `GET`        | `/api/weatherdata/lowest`           | Tiefster Messwert eines Typs im Zeitraum               |
| `GET`        | `/api/weatherdata/average`          | Durchschnittswert eines Typs im Zeitraum               |
| `GET`        | `/api/weatherdata/count`            | Anzahl Messungen eines Typs im Zeitraum                |
| `GET`        | `/api/weatherdata`                  | Alle Messwerte mit vollständigen Attributen            |
| `GET`        | `/api/stations`                     | Liste aller gespeicherten Wetterstationen              |
| `GET`        | `/api/measurementtype/measurement-types` | Liste aller verfügbaren Messwerttypen            |

#### 🔍 Abfrageparameter für `weatherdata`-Endpunkte

| Parameter     | Beschreibung                          | Pflicht |
|---------------|----------------------------------------|---------|
| `start`       | Startdatum des gewünschten Zeitraums   | ✅      |
| `end`         | Enddatum des gewünschten Zeitraums     | ✅      |
| `station`     | Name der Wetterstation (z. B. "tiefenbrunnen") | ❌   |
| `measurementType` | Typ der Messung (z. B. "air_temperature") | ✅ |

Beispiel-URL:

```
GET https://localhost:7017/api/weatherdata/average?start=2025-04-01&end=2025-04-10&station=tiefenbrunnen&measurementType=air_temperature
```

---

## 📡 Externe Datenquelle

Die Wetterdaten werden über folgende öffentliche Open-Data-API bezogen:

🔗 **[https://tecdottir.metaodi.ch/docs/](https://tecdottir.metaodi.ch/docs/)**

### Eigenschaften der bezogenen Messwerte

- **Stationen**: "tiefenbrunnen", "mythenquai"
- **Zeitraum**: Messwerte jeweils bis zum Vortag
- **Limitierung**: max. 100 Einträge je Abfrage
- **Sortierung**: timestamp_cet desc
- **Datenformat**: JSON

---

## 🧪 Tests

Die Anwendung wurde mit **xUnit** und **Moq** umfassend getestet.  
Der Fokus der Tests liegt auf der Absicherung der zentralen Logik, Validierungen und der REST-Schnittstelle.

### ✅ Abgedeckte Testbereiche

- Unit-Tests für alle Service-Methoden (z. B. Durchschnitt, Minimum, Maximum, Anzahl, Gesamtabfrage)
- Validierungsszenarien (z. B. ungültige Eingaben, nicht vorhandene Ressourcen)
- Tests für Randfälle (z. B. keine Daten vorhanden)
- Abdeckung des gesamten Kontrollflusses inklusive Fehlerbehandlung
- Integrationstests für Controller und Datenzugriffsschicht

### 🔍 Beispiele getesteter Szenarien

- Fehler bei inkonsistentem Datumsbereich (z. B. Start > Ende)
- Prüfung auf Existenz von Stationen und Messwerttypen
- Rückgabe von `null` oder leeren Mengen bei fehlenden Daten
- Korrekte Berechnung statistischer Kennzahlen
- API-Fehlermeldungen und Statuscodes (400, 404, 500)

> 🧪 Ziel der Tests: **Robustheit, Nachvollziehbarkeit und Wartbarkeit der Applikation sicherstellen**

---

## 🛠 Datenbank

Das Projekt verwendet eine relationale **SQLite**-Datenbank zur persistenten Speicherung von Wettermessdaten.

### 📊 Datenmodell

Die Datenbank besteht aus drei Haupttabellen:

#### `WeatherData` (Messwerte)
Speichert die eigentlichen Wetterdaten.

| Spalte           | Beschreibung                          | Beispiel              |
|------------------|----------------------------------------|------------------------|
| `WeatherDataId`  | Primärschlüssel                        | 1                      |
| `StationId`      | Fremdschlüssel zu `Station`            | 2                      |
| `MeasurementTypeId` | Fremdschlüssel zu `MeasurementType` | 3                      |
| `TimestampUtc`   | Zeitpunkt der Messung (UTC)            | 2025-04-05T21:50:00Z   |
| `Value`          | Gemessener Wert                        | 13.2                   |
| `Unit`           | Einheit der Messung                    | °C, hPa                |

#### `Station` (Stammdaten Wetterstation)
Beinhaltet den Namen der Station.

| Spalte     | Beschreibung             | Beispiel         |
|------------|---------------------------|------------------|
| `StationId`| Primärschlüssel           | 1                |
| `Name`     | Name der Wetterstation    | Tiefenbrunnen    |

#### `MeasurementType` (Typ der Messung)
Definiert Art und Standard-Einheit der Messung.

| Spalte           | Beschreibung                          | Beispiel              |
|------------------|----------------------------------------|------------------------|
| `MeasurementTypeId` | Primärschlüssel                    | 2                      |
| `TypeName`       | Typenbezeichnung                     | air_temperature        |
| `DefaultUnit`    | Standard-Einheit                      | °C                     |

### 🔄 Beziehungen

- Eine `Station` kann viele `WeatherData`-Einträge haben
- Ein `MeasurementType` kann vielen `WeatherData`-Einträgen zugeordnet sein
- Jeder `WeatherData`-Eintrag ist einer Station **und** einem Messungstyp zugewiesen

> Die Struktur ist optimiert für einfache Aggregationen (Max, Min, Avg) und historische Abfragen.

