# Game Player Services Platform

A backend-focused multiplayer game services platform built with **ASP.NET Core, Entity Framework Core, and PostgreSQL**.

The project provides REST APIs for managing player profiles, recording multiplayer matches, retrieving player match history, and calculating ranked leaderboards. It is designed as a portfolio project to demonstrate backend API development, relational data modeling, service-layer architecture, LINQ aggregation, dependency injection, structured business errors, and automated testing.

> **Current status:** Backend MVP under active development.

---

## Features

### Player Profiles

- Create player profiles
- Retrieve player profiles by ID
- Update player information
- Search players by display name
- Sort player results
- Paginate player lists
- Prevent duplicate display names
- Retrieve a player's paginated match history

### Match Management

- Create multiplayer match records
- Retrieve matches by ID
- Validate participating players
- Detect duplicate player participation in the same match
- Prevent duplicate external match IDs
- Validate game modes
- Validate participant results
- Return missing player IDs when referenced players do not exist
- Persist player-specific match statistics:
  - Score
  - Kills
  - Deaths
  - Win / Loss / Draw result

### Ranked Leaderboard

- Include Ranked matches only
- Aggregate player statistics with EF Core LINQ
- Calculate:
  - Matches played
  - Wins
  - Total score
  - Total kills
  - Total deaths
- Sort by:
  1. Total score descending
  2. Wins descending
  3. Display name ascending
- Support pagination

### Validation and Error Handling

The API currently uses multiple validation layers:

- ASP.NET Core model binding
- Data Annotation validation
- Business-rule validation
- Database constraints
- Typed service results for business failures

Examples include:

- `400 Bad Request` for invalid input or missing players
- `404 Not Found` for resources that do not exist
- `409 Conflict` for duplicate match identifiers

---

## Tech Stack

| Area | Technology |
| --- | --- |
| Language | C# |
| Backend | ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| PostgreSQL Provider | Npgsql |
| Testing | xUnit |
| Service Database Tests | SQLite In-Memory |
| API Style | REST |
| Source Control | Git / GitHub |

---

## Architecture

The backend is structured using separation of concerns between HTTP handling, business logic, and data access.

```text
HTTP Request
     |
     v
Controller
     |
     v
Service Interface
     |
     v
Service
     |
     v
AppDbContext
     |
     v
PostgreSQL
```

For example:

```text
MatchesController
      |
      v
IMatchService
      |
      v
MatchService
      |
      v
AppDbContext
      |
      v
PostgreSQL
```

Controllers are responsible for HTTP concerns such as request handling and status codes, while services contain business logic and database operations.

The project also uses a typed **Result Pattern** for match creation so that the service layer can report business failures without depending directly on HTTP concepts.

Example flow:

```text
MatchService
    |
    |-- Success
    |-- DuplicateExternalMatchId
    |-- PlayerNotFound
    |-- InvalidGameMode
    |-- InvalidResult
    v
MatchesController
    |
    |-- 201 Created
    |-- 409 Conflict
    |-- 400 Bad Request
```

---

## Data Model

The core match model uses `MatchParticipant` as the relationship between players and matches.

```text
PlayerProfile
     |
     | 1
     |
     | *
MatchParticipant
     *
     |
     | 1
     |
   Match
```

A `MatchParticipant` stores statistics that belong to a specific player in a specific match.

### PlayerProfile

```text
Id
DisplayName
Region
Bio
CreatedAtUtc
UpdatedAtUtc
```

### Match

```text
Id
ExternalMatchId
GameMode
PlayedAtUtc
CreatedAtUtc
```

### MatchParticipant

```text
Id
MatchId
PlayerProfileId
Score
Kills
Deaths
Result
```

The database configuration includes:

- Foreign-key relationships
- Unique external match IDs
- Unique `(MatchId, PlayerProfileId)` combinations
- Cascade deletion for match participants when a match is removed
- Restricted player deletion to preserve match history
- EF Core migrations

---

## API Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/health` | Check API health |
| `POST` | `/api/players` | Create a player profile |
| `GET` | `/api/players/{id}` | Get a player by ID |
| `PUT` | `/api/players/{id}` | Update a player profile |
| `GET` | `/api/players` | Search, sort, and paginate players |
| `GET` | `/api/players/{id}/matches` | Get paginated player match history |
| `POST` | `/api/matches` | Create a match |
| `GET` | `/api/matches/{id}` | Get a match by ID |
| `GET` | `/api/leaderboard` | Get the ranked leaderboard |

Example player list request:

```http
GET /api/players?page=1&pageSize=10&search=snow&sortBy=displayName&sortDirection=asc
```

Example leaderboard request:

```http
GET /api/leaderboard?page=1&pageSize=10
```

---

## Leaderboard Query

Leaderboard statistics are calculated from match participant records using EF Core LINQ.

Conceptually:

```text
MatchParticipants
       |
       v
Filter Ranked matches
       |
       v
Group by player
       |
       v
Count matches and wins
       |
       v
Sum score / kills / deaths
       |
       v
Sort leaderboard
       |
       v
Apply pagination
```

The LINQ query is translated by Entity Framework Core into relational database operations such as joins, grouping, counting, summation, ordering, and pagination.

---

## Automated Testing

The project uses different testing approaches depending on the layer being tested.

### Controller Unit Tests

Controllers are tested using fake service implementations.

```text
Controller
    |
    v
Service Interface
    |
    v
Fake Service
```

This allows controller behavior to be tested without connecting to a database.

Examples include verifying that:

```text
Service Success
→ 201 Created

DuplicateExternalMatchId
→ 409 Conflict

PlayerNotFound
→ 400 Bad Request
```

### Service Tests

Database-dependent service logic is tested using:

```text
Real Service
    |
    v
Real AppDbContext
    |
    v
SQLite In-Memory Database
```

Current service tests cover behavior such as:

- Ranked-only leaderboard filtering
- Aggregate score calculations
- Win counts
- Kill and death totals
- Leaderboard ordering
- Tie-breaking rules
- Pagination
- Empty leaderboard scenarios
- Successful match persistence
- Duplicate match detection
- Missing-player validation

Each service test creates an isolated in-memory database that is disposed after the test completes.

---

## Project Structure

```text
game-player-services-platform/
|
|-- docs/
|
|-- server/
|   `-- GamePlayer.Api/
|       |
|       |-- Controllers/
|       |-- Data/
|       |-- Dtos/
|       |-- Migrations/
|       |-- Models/
|       |-- Services/
|       |   `-- Results/
|       |
|       `-- Program.cs
|
|-- tests/
|   `-- GamePlayer.Api.Tests/
|       |
|       |-- Infrastructure/
|       |-- LeaderboardControllerTests.cs
|       |-- LeaderboardServiceTests.cs
|       |-- MatchServiceTests.cs
|       `-- MatchesControllerTests.cs
|
|-- .gitignore
`-- README.md
```

---

## Running the Project Locally

### Prerequisites

Install:

- .NET SDK
- PostgreSQL
- Git

Clone the repository:

```bash
git clone https://github.com/EmmaDz/game-player-services-platform.git
cd game-player-services-platform
```

Move into the API project:

```bash
cd server/GamePlayer.Api
```

Configure the PostgreSQL connection string using .NET User Secrets:

```bash
dotnet user-secrets set \
  "ConnectionStrings:GamePlayerDatabase" \
  "Host=localhost;Database=game_player_services;Username=game_player_app;Password=YOUR_PASSWORD"
```

Apply EF Core migrations:

```bash
dotnet ef database update
```

Run the API:

```bash
dotnet run
```

Then test the health endpoint:

```http
GET http://localhost:5204/api/health
```

> The local port may differ depending on the development environment.

---

## Running Tests

From the repository root:

```bash
dotnet test tests/GamePlayer.Api.Tests/GamePlayer.Api.Tests.csproj
```

The test suite currently includes controller unit tests and EF Core service tests using SQLite in-memory databases.

---

## Engineering Concepts Demonstrated

This project currently demonstrates practical experience with:

- REST API design
- ASP.NET Core controllers
- Entity Framework Core
- PostgreSQL relational modeling
- DTO and entity separation
- One-to-many and many-to-many-style relationships
- LINQ filtering, grouping, aggregation, sorting, and pagination
- Dependency injection
- Service-layer architecture
- Interfaces and test doubles
- Result Pattern for business errors
- Asynchronous programming with `async` / `await`
- Input validation and business-rule validation
- HTTP status-code handling
- Database migrations and constraints
- Controller unit testing
- Service-level database testing

---

## Planned Improvements

The project is still under active development.

Planned work includes:

- Authentication and authorization
- Additional match business rules
- Further service-layer refactoring
- React + TypeScript frontend
- Docker support
- CI/CD with GitHub Actions
- Cloud deployment

These items are planned and are **not yet implemented in the current version**.

---

## Author

**Zida Chen**

- GitHub: [EmmaDz](https://github.com/EmmaDz)
- LinkedIn: [linkedin.com/in/dzzcemma](https://www.linkedin.com/in/dzzcemma)
