# Congestion Tax Calculator

A modern .NET 9.0 application for calculating congestion tax fees for vehicles passing through toll stations in Swedish cities. Built with Clean Architecture principles and Domain-Driven Design (DDD).

## Overview

This system calculates congestion taxes based on configurable rules stored in a PostgreSQL database. It implements complex business logic including:

- **Single Charge Rule**: Vehicles passing multiple toll stations within 60 minutes are charged only once (highest fee)
- **Daily Maximum Cap**: Maximum charge per vehicle per day (60 SEK for Gothenburg)
- **Time-based Pricing**: Different fees based on time of day
- **Toll-Free Conditions**: Weekends, holidays, July, and days before holidays
- **Vehicle Exemptions**: Emergency vehicles, buses, motorcycles, diplomats, military, and foreign vehicles

## Features

- ✅ Clean Architecture with DDD
- ✅ CQRS pattern using MediatR
- ✅ PostgreSQL database with Entity Framework Core
- ✅ RESTful API with OpenAPI documentation
- ✅ Docker support with docker-compose
- ✅ Configurable tax rules per city and year
- ✅ Comprehensive test coverage (unit & integration tests)

## Technology Stack

- **.NET 9.0** - Target framework
- **ASP.NET Core** - Web API
- **Entity Framework Core 9.0** - ORM
- **PostgreSQL 16** - Database
- **MediatR** - CQRS implementation
- **Scalar** - API documentation UI
- **xUnit** - Testing framework
- **Docker** - Containerization

## Project Structure

```
congestion-tax-calculator/
├── src/
│   ├── Domain/              # Core business logic (no dependencies)
│   ├── Application/         # Use cases (commands/queries)
│   ├── Infrastructure/      # Data access & external concerns
│   └── API/                 # HTTP endpoints
└── tests/
    ├── Tests.Unit/          # Domain & application unit tests
    └── Tests.Integration/   # Infrastructure & API integration tests
```

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/get-started) and Docker Compose
- (Optional) [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) for local development

### Clone the Repository

```bash
git clone https://github.com/yahyafazeli/congestion-tax-calculator.git
cd congestion-tax-calculator
```

### Run with Docker Compose

The easiest way to run the application is using Docker Compose:

```bash
docker-compose up
```

This will:
1. Start a PostgreSQL 16 database container
2. Build and start the API container
3. Run database migrations automatically
4. Seed the database with Gothenburg 2013 tax rules

The API will be available at: **http://localhost:8080**

### Run Locally (Without Docker)

If you prefer to run without Docker:

1. **Start PostgreSQL:**
   ```bash
   docker run -d -p 5432:5432 \
     -e POSTGRES_USER=postgres \
     -e POSTGRES_PASSWORD=YourStrong@Passw0rd \
     -e POSTGRES_DB=CongestionTaxDb \
     postgres:16-alpine
   ```

2. **Run migrations:**
   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/API
   ```

3. **Start the API:**
   ```bash
   dotnet run --project src/API
   ```

## API Documentation

Once the application is running, you can access:

- **Scalar UI**: http://localhost:8080/scalar/v1
- **OpenAPI Spec**: http://localhost:8080/openapi/v1.json

## API Endpoints

### Calculate Tax

Calculate congestion tax for a vehicle based on timestamps.

**Endpoint:** `POST /api/tax/calculate`

**Request:**
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

**Vehicle Types:**
- `1` - Car (taxable)
- `2` - Motorbike (exempt)
- `3` - Emergency (exempt)
- `4` - Bus (exempt)
- `5` - Diplomat (exempt)
- `6` - Military (exempt)
- `7` - Foreign (exempt)

**Response:**
```json
{
  "totalTax": 21.0,
  "currency": "SEK"
}
```

**Example using curl:**
```bash
curl -X POST http://localhost:8080/api/tax/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "cityId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "year": 2013,
    "vehicleRegistration": "ABC123",
    "vehicleType": 1,
    "timestamps": [
      "2013-02-08T06:27:00",
      "2013-02-08T15:29:00"
    ]
  }'
```

### Get All Cities

Retrieve all cities with tax rules.

**Endpoint:** `GET /api/cities`

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Gothenburg"
  }
]
```

### Get City Tax Rules

Get all tax rules for a specific city.

**Endpoint:** `GET /api/cities/{cityId}/rules`

**Response:**
```json
{
  "cityId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "cityName": "Gothenburg",
  "rules": [
    {
      "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "year": 2013,
      "dailyMax": 60.0,
      "singleChargeMinutes": 60
    }
  ]
}
```

### Get Detailed Tax Rule

Get detailed configuration for a specific tax rule.

**Endpoint:** `GET /api/cities/{cityId}/rules/{ruleId}`

**Response:** Includes all intervals, toll-free dates, months, weekdays, and exempt vehicles.

## Gothenburg 2013 Tax Rules

The system comes pre-configured with Gothenburg's 2013 congestion tax rules:

### Time-based Fees

| Time        | Amount (SEK) |
|-------------|--------------|
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

- **Daily Maximum**: 60 SEK per vehicle
- **Single Charge Window**: 60 minutes (highest fee applies)
- **Toll-Free Days**: Weekends, public holidays, days before holidays, entire month of July
- **Exempt Vehicles**: Motorcycles, buses, emergency vehicles, diplomats, military, foreign vehicles

## Example Calculations

### Example 1: Simple Day

**Input:**
- Vehicle: Car
- Timestamps: 
  - 2013-02-07 06:23:27
  - 2013-02-07 15:27:00

**Calculation:**
- 06:23 → 8 SEK
- 15:27 → 13 SEK
- **Total: 21 SEK**

### Example 2: Single Charge Rule

**Input:**
- Vehicle: Car
- Timestamps:
  - 2013-02-08 06:20:27
  - 2013-02-08 06:27:00

**Calculation:**
- Both within 60-minute window
- Fees: 8 SEK, 8 SEK
- **Charged: 8 SEK (highest in window)**

### Example 3: Daily Maximum Cap

**Input:**
- Vehicle: Car
- Multiple passes throughout the day totaling 73 SEK

**Calculation:**
- Sum exceeds daily maximum
- **Capped at: 60 SEK**

### Example 4: Exempt Vehicle

**Input:**
- Vehicle: Motorbike
- Any timestamps

**Calculation:**
- **Total: 0 SEK (exempt vehicle)**

### Example 5: Toll-Free Date

**Input:**
- Vehicle: Car
- Date: 2013-03-28 (day before Good Friday)

**Calculation:**
- **Total: 0 SEK (toll-free date)**

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Unit Tests Only

```bash
dotnet test tests/Tests.Unit
```

### Run Integration Tests Only

```bash
dotnet test tests/Tests.Integration
```

### Run with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Development

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/API
```

Update database:
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

## Architecture

This project follows **Clean Architecture** principles:

- **Domain Layer**: Pure business logic, no external dependencies
- **Application Layer**: Use cases orchestrated via MediatR (CQRS)
- **Infrastructure Layer**: Database access, EF Core, repositories
- **API Layer**: HTTP endpoints, minimal API style

### Key Design Patterns

- **Domain-Driven Design (DDD)**: Entities, value objects, aggregates
- **CQRS**: Commands for writes, queries for reads
- **Repository Pattern**: Abstraction over data access
- **Specification Pattern**: Single charge rule grouping logic
