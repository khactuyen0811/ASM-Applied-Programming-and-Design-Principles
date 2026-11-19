# SIMS - Student Information Management System

SIMS is an ASP.NET Core MVC application (net8.0) for managing students, courses, teachers and enrollments. It uses CSV files for persistence instead of a database and cookie authentication with Admin, Lecturer and Student roles. The solution is organized in layers so that the web, application, domain and infrastructure concerns can be developed and tested independently.

## Tech Stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 8.0 (`net8.0`) |
| Web framework | ASP.NET Core MVC |
| CSV persistence | CsvHelper 31.0.3 |
| Unit testing | xUnit 2.5.3 |
| Mocking | Moq 4.20.72 |
| Integration testing | Microsoft.AspNetCore.Mvc.Testing 8.0.11 |
| IDE | Visual Studio 2022 (solution format version 12.00, Visual Studio 17) |

## Solution Structure

| Project | Type | Responsibility |
| --- | --- | --- |
| `SIMS` | ASP.NET Core MVC web app | Controllers, views, cookie authentication, role policies, application startup and composition |
| `SIMS.Domain` | Class library | Entities, enums, domain common types and repository contracts |
| `SIMS.Application` | Class library | DTOs, service contracts, application services and result types |
| `SIMS.Infrastructure` | Class library | CSV repositories, PBKDF2 password hashing, user factory, data seeding and DI registration |
| `SIMS.Tests` | xUnit test project | Unit tests for application and infrastructure, plus MVC integration tests |

## Architecture

The intended dependency direction is:

```text
Web -> Application -> Domain
Web -> Infrastructure -> Domain
```

Notes:

- Domain is the core and contains entities, enums, paging types and abstractions.
- Application depends on Domain abstractions and contains business services and DTO mapping.
- Infrastructure implements persistence, hashing, factories and seeding. It depends on Domain contracts rather than on the web layer.
- The web project references Domain, Application and Infrastructure and acts as the composition root.
- Interfaces are defined in Domain or Application and implemented in Infrastructure, following the Dependency Inversion Principle.
- Composition is centralized in `AddSimsApplication()` (`SIMS.Application/DependencyInjection.cs`) and `AddSimsInfrastructure(dataDirectory)` (`SIMS.Infrastructure/DependencyInjection.cs`). `SIMS/Program.cs` reads `Sims:DataDirectory` and wires both registrations.
- Layered separation keeps controllers thin, business rules in services and persistence details behind contracts, which also makes services and repositories testable with mocks and temporary CSV directories.

## SOLID Mapping

| Principle | Concrete Example | File |
| --- | --- | --- |
| S - Single Responsibility | Controllers, services and repositories each have one focused responsibility; for example, `StudentService` handles student paging, validation and enrollment mapping only. | `SIMS\Application\Services\StudentService.cs` |
| O - Open/Closed | New repository or service implementations can be registered in composition code without changing existing consumers. | `SIMS.Infrastructure/DependencyInjection.cs` |
| L - Liskov Substitution | `CsvStudentRepository` and `CsvCourseRepository` can replace the generic repository contract wherever `IRepository<T>` is expected. | `SIMS.Domain/Abstractions/IRepository.cs` |
| I - Interface Segregation | Small, focused contracts such as `IStudentRepository`, `IEnrollmentRepository` and `IPasswordHasher` expose only the operations their consumers need. | `SIMS.Domain/Abstractions/` |
| D - Dependency Inversion | Application services depend on abstractions rather than CSV implementations; for example, `EnrollmentService` depends on `IEnrollmentRepository`, `IStudentRepository` and `ICourseRepository`. | `SIMS.Application/Services/EnrollmentService.cs` |

## Design Patterns

| Pattern | Where It Appears | File |
| --- | --- | --- |
| Repository | CSV-backed repositories hide file access and provide CRUD/count operations over entities. | `SIMS.Infrastructure/Csv/CsvRepository.cs` |
| Dependency Injection | Application services and infrastructure implementations are registered in container composition methods. | `SIMS.Application/DependencyInjection.cs`, `SIMS.Infrastructure/DependencyInjection.cs` |
| Factory | `AppUserFactory` creates `AppUser` instances with hashed credentials and role links. | `SIMS.Infrastructure/Factories/AppUserFactory.cs` |
| Facade / Service Layer | Application services expose use-case operations to controllers and hide repository coordination. | `SIMS.Application/Services/` |
| DTO Mapping | Entities are mapped to list/detail DTOs for views and JSON responses. | `SIMS.Application/Dtos/`, `SIMS.Application/Services/` |
| Result Object | `ServiceResult` and `ServiceResult<T>` return success, errors and optional values without throwing for expected validation failures. | `SIMS.Application/Dtos/ServiceResult.cs` |
| Generic Base Repository | `CsvRepository<T>` provides shared load, cache, ID assignment and atomic file persistence logic. | `SIMS.Infrastructure/Csv/CsvRepository.cs` |

## Features

- Cookie-based login and logout with claims for user id, username, role, display name, and optional student or teacher id.
- Role policies: `AdminOnly` requires the `Admin` role; `StaffOnly` allows `Admin` and `Lecturer`.
- Dashboard statistics returned as JSON from `/Dashboard/Stats`, including totals, active students, enrollment averages, chart data and recent students.
- Student paging, search and filtering by program and status over the 10,000 seeded students.
- Course and teacher management services with paging, search, validation, duplicate checks and lecturer/course relationships.
- Enrollment with capacity and duplicate validation, plus unenroll and enrollment listing by student or course.
- CSV persistence with cached reads and atomic writes through a temporary file followed by `File.Move`.
- Minimal MVC views for login, dashboard and a placeholder student list page; course and teacher screens and the full student management UI are not implemented yet.

## Accounts

All seeded accounts use the password `12345`.

| Username | Role | Notes |
| --- | --- | --- |
| `trieunt` | Admin | Primary administrator account |
| `lecturer` | Lecturer | Linked to teacher 1 |
| `student` | Student | Linked to student 1 |
| `lecturer2` | Lecturer | Linked to teacher 2 |
| `lecturer3` | Lecturer | Linked to teacher 3 |
| `lecturer4` | Lecturer | Linked to teacher 4 |
| `student2` | Student | Linked to student 2 |
| `student3` | Student | Linked to student 3 |
| `student4` | Student | Linked to student 4 |

The login form posts `Username` and `Password` to `/Login`. Invalid credentials show `Your account invalid`.

## Data Storage

Data is stored under `App_Data` by default. The location is configurable through the `Sims:DataDirectory` key in `SIMS/appsettings.json`; relative paths are resolved against the web content root.

CSV files created and used by the application:

| File | Contents |
| --- | --- |
| `students.csv` | Student records |
| `courses.csv` | Course records |
| `enrollments.csv` | Student-course enrollments and optional grades |
| `users.csv` | Login accounts, password hashes, salts and role links |
| `teachers.csv` | Teacher records |

The application seeds each empty dataset once on startup: 10,000 students, 120 teachers, 60 courses and approximately 35,000 enrollments. Password hashes use salted PBKDF2-SHA256 with 100,000 iterations.

## Getting Started

Prerequisites: .NET 8 SDK. The solution opens in Visual Studio 2022.

```powershell
dotnet restore
dotnet build SIMS.sln
dotnet run --project SIMS
```

Then open `http://localhost:5000` or the URL printed by the application and log in with `trieunt` / `12345`.

Notes:

- The app uses HTTPS redirection, so the browser may switch to the HTTPS URL shown in the console.
- The first run seeds the CSV data and takes a few seconds. Subsequent runs reuse the existing `App_Data` files.

## Testing

Run the test project with:

```powershell
dotnet test SIMS.Tests\SIMS.Tests.csproj
```

The suite contains 58 test cases derived from 48 `[Fact]`/`[Theory]` methods; theory data rows are counted individually.

| Area | Test cases | Coverage |
| --- | --- | --- |
| Infrastructure | 16 | CSV persistence round-trip, PBKDF2 hashing and verification, CSV seeder behavior |
| Application | 35 | Auth, student, course, teacher, enrollment and dashboard services using Moq |
| MVC integration | 7 | Login success/failure, redirects, role-based denial, dashboard statistics with `WebApplicationFactory` |

Per-file test case counts:

| File | Test cases |
| --- | --- |
| `SIMS.Tests/Unit/Infrastructure/CsvStudentRepositoryTests.cs` | 6 |
| `SIMS.Tests/Unit/Infrastructure/Pbkdf2PasswordHasherTests.cs` | 8 |
| `SIMS.Tests/Unit/Infrastructure/CsvDataSeederTests.cs` | 2 |
| `SIMS.Tests/Unit/Application/AuthServiceTests.cs` | 5 |
| `SIMS.Tests/Unit/Application/StudentServiceTests.cs` | 15 |
| `SIMS.Tests/Unit/Application/CourseServiceTests.cs` | 3 |
| `SIMS.Tests/Unit/Application/TeacherServiceTests.cs` | 3 |
| `SIMS.Tests/Unit/Application/EnrollmentServiceTests.cs` | 7 |
| `SIMS.Tests/Unit/Application/DashboardServiceTests.cs` | 2 |
| `SIMS.Tests/Integration/SimsIntegrationTests.cs` | 7 |

## Repository Layout

```text
.
|-- .github/
|-- SIMS/
|-- SIMS.Application/
|-- SIMS.Domain/
|-- SIMS.Infrastructure/
|-- SIMS.Tests/
|-- SIMS.sln
`-- README.md
```

UI views are intentionally minimal at this stage; the backend, data layer and automated tests are complete.
