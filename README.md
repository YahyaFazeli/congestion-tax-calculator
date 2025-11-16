# 🚦 Congestion Tax Calculator

A modern **.NET 9** application for calculating congestion taxes in Swedish cities — built with **Clean Architecture** and **Domain-Driven Design (DDD)**.

---

## 🧭 Overview

This system computes congestion taxes using rules stored in PostgreSQL. It handles complex logic such as:

* **Single Charge Rule**: One charge per 60-minute window (highest fee applies)
* **Daily Maximum Cap**: Max 60 SEK per day (Gothenburg)
* **Time-Based Pricing**
* **Toll-Free Conditions**: Weekends, holidays, July, pre-holiday days
* **Vehicle Exemptions**: Emergency, military, diplomats, buses, motorcycles, foreign vehicles

---

## ✨ Features

* Clean Architecture + DDD
* CQRS with MediatR
* PostgreSQL + EF Core
* REST API with OpenAPI
* Docker + docker-compose
* Configurable tax rules per city/year
* Unit & Integration test coverage

---

## 🛠️ Tech Stack

* **.NET 9**
* **ASP.NET Core**
* **Entity Framework Core 9**
* **PostgreSQL 16**
* **MediatR**
* **Scalar** (API UI)
* **xUnit**
* **Docker**

---

## 📁 Project Structure

```
congestion-tax-calculator/
├── src/
│   ├── Domain/              # Business rules
│   ├── Application/         # CQRS use cases
│   ├── Infrastructure/      # EF Core, PostgreSQL, services
│   └── API/                 # HTTP endpoints
└── tests/
    ├── Tests.Unit/          
    └── Tests.Integration/
```

---

## 🚀 Getting Started

### Requirements

* Docker + Docker Compose
* (Optional) .NET 9 SDK

### Clone

```bash
git clone https://github.com/yahyafazeli/congestion-tax-calculator.git
cd congestion-tax-calculator
```

### Run with Docker

```bash
docker-compose up
```

This will:

1. Start PostgreSQL
2. Run migrations
3. Seed Gothenburg 2013 rules
4. Start API at **[http://localhost:8080](http://localhost:8080)**

### Run Locally (No Docker)

Start PostgreSQL:

```bash
docker run -d -p 5432:5432 \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=YourStrong@Passw0rd \
  -e POSTGRES_DB=CongestionTaxDb \
  postgres:16-alpine
```

Run migrations:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

Start API:

```bash
dotnet run --project src/API
```

---

## 📚 API Documentation

* Scalar UI → **[http://localhost:8080/scalar/v1](http://localhost:8080/scalar/v1)**
* OpenAPI JSON → **[http://localhost:8080/openapi/v1.json](http://localhost:8080/openapi/v1.json)**

---

## 📡 API Endpoints

### 🔢 Calculate Tax

POST `/api/tax/calculate`

Request:

```json
{
  "cityId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "year": 2013,
  "vehicleRegistration": "ABC123",
  "vehicleType": 1,
  "timestamps": [
    "2013-02-08T06:27:00",
    "2013-02-08T15:29:00"
  ]
}
```

Vehicle types:
1=Car, 2=Motorbike, 3=Emergency, 4=Bus, 5=Diplomat, 6=Military, 7=Foreign

Response:

```json
{
  "totalTax": 21.0,
  "currency": "SEK"
}
```

---

### 🌆 Get All Cities

GET `/api/cities`

```json
[
  { "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "Gothenburg" }
]
```

---

### 📜 Get City Tax Rules

GET `/api/cities/{cityId}/rules`

```json
{
  "cityId": "...",
  "cityName": "Gothenburg",
  "rules": [
    {
      "id": "...",
      "year": 2013,
      "dailyMax": 60.0,
      "singleChargeMinutes": 60
    }
  ]
}
```

---

### 📘 Get Detailed Tax Rule

GET `/api/cities/{cityId}/rules/{ruleId}`

```json
{
  "id": "...",
  "cityId": "...",
  "cityName": "Gothenburg",
  "year": 2013,
  "dailyMax": 60.0,
  "singleChargeMinutes": 60,
  "intervals": [
    { "startTime": "06:00:00", "endTime": "06:29:59", "amount": 8.0 }
  ],
  "tollFreeDates": ["2013-01-01"],
  "tollFreeMonths": [7],
  "tollFreeWeekdays": [6,0],
  "exemptVehicles": [2,3,4,5,6,7]
}
```

---

### ➕ Create City

POST `/api/cities`

Request:

```json
{ "name": "Stockholm" }
```

---

### ✏️ Update City

PUT `/api/cities/{cityId}`

```json
{ "name": "Stockholm Updated" }
```

---

### ➕ Create City Tax Rule

POST `/api/cities/{cityId}/rules`

```json
{
  "year": 2024,
  "dailyMax": 60.0,
  "singleChargeMinutes": 60,
  "intervals": [
    { "startTime": "06:00:00", "endTime": "06:29:59", "amount": 8.0 }
  ],
  "tollFreeDates": ["2024-01-01"],
  "tollFreeMonths": [7],
  "tollFreeWeekdays": [6,0],
  "exemptVehicles": [2,3,4,5,6,7]
}
```

---

### ✏️ Update City Tax Rule

PUT `/api/cities/{cityId}/rules/{ruleId}`

```json
{
  "year": 2024,
  "dailyMax": 75.0,
  "singleChargeMinutes": 90,
  "intervals": [
    { "startTime": "06:00:00", "endTime": "06:29:59", "amount": 10.0 }
  ]
}
```

---

## 🏙️ Gothenburg 2013 Tax Rules

### Time-Based Fees

| Time        | Amount (SEK) |
| ----------- | ------------ |
| 06:00–06:29 | 8            |
| 06:30–06:59 | 13           |
| 07:00–07:59 | 18           |
| 08:00–08:29 | 13           |
| 08:30–14:59 | 8            |
| 15:00–15:29 | 13           |
| 15:30–16:59 | 18           |
| 17:00–17:59 | 13           |
| 18:00–18:29 | 8            |
| 18:30–05:59 | 0            |

### Rules

* Daily max: **60 SEK**
* Single-charge window: **60 min**
* Toll-free: Weekends, holidays, pre-holidays, **July**
* Exempt: Motorcycles, buses, emergency, diplomats, military, foreign vehicles

---

## 🧮 Example Calculations

### 1) Simple

08:23 → 8 SEK
15:27 → 13 SEK
**Total = 21 SEK**

### 2) Single Charge Window

Two timestamps within 60 minutes → **Highest fee only**

### 3) Daily Maximum

If total > 60 SEK → **Capped at 60**

### 4) Exempt Vehicle

Motorbike → **0 SEK**

### 5) Toll-Free Date

Pre-holiday → **0 SEK**

---

## 🧪 Running Tests

Run all:

```bash
dotnet test
```

Unit only:

```bash
dotnet test tests/Tests.Unit
```

Integration only:

```bash
dotnet test tests/Tests.Integration
```

Coverage:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 🧱 Development

### Migrations

Create:

```bash
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/API
```

Update:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

---

## 🏗️ Architecture

The solution follows **Clean Architecture**:

* **Domain** — Pure business logic
* **Application** — CQRS (MediatR)
* **Infrastructure** — EF Core, PostgreSQL
* **API** — Minimal API endpoints

### Patterns

* DDD — Entities, Value Objects, Aggregates
* CQRS — Commands & Queries
* Repository Pattern
* Specification Pattern

---
