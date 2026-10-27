# SkillHub Backend API

SkillHub is a professional RESTful backend solution built with **.NET 10** and **Clean Architecture** for managing in-person training workshops, instructors, participants, enrollments, bulk Excel imports, PDF enrollment certificates, and analytical reports.

---

## 1. Project Overview & Architecture

The solution follows **Clean Architecture** principles to ensure a strict separation of concerns, high maintainability, and testability. Dependencies flow inward toward the core domain.

```text
SkillHub/
├── SkillHub.slnx
├── README.md
├── skillhub_db_backup.sql
└── src/
    ├── SkillHub.Domain/          # Enterprise business entities, enums, and repository interfaces
    ├── SkillHub.Application/     # Business logic services, DTOs, and application abstractions
    ├── SkillHub.Infrastructure/  # EF Core PostgreSQL persistence, Identity, JWT, ClosedXML, QuestPDF, Seeder
    └── SkillHub.Api/             # ASP.NET Core Web API controllers, DI setup, middleware, Swagger UI
```

### Architectural Layers

- **`SkillHub.Domain`**:
  - Contains core domain entities (`Workshop`, `Enrollment`), enumerations (`EnrollmentStatus`), and repository contracts (`IWorkshopRepository`, `IEnrollmentRepository`).
  - Has **zero dependencies** on external frameworks or outer layers.
- **`SkillHub.Application`**:
  - Implements core business rules and use cases through application services (`WorkshopService`, `EnrollmentService`) and contracts (`IAuthService`, `IWorkshopService`, `IEnrollmentService`).
  - Defines Data Transfer Objects (DTOs) and infrastructure abstractions (`IJwtService`, `IUserService`, `ICertificateGenerator`).
- **`SkillHub.Infrastructure`**:
  - Implements data persistence using **Entity Framework Core** with **PostgreSQL** (`ApplicationDbContext`).
  - Configures **ASP.NET Core Identity** (`ApplicationUser`, roles `Employee` and `Participant`).
  - Implements repositories (`WorkshopRepository`, `EnrollmentRepository`), JWT token generation (`JwtService`), authentication (`AuthService`), user lookup (`UserService`), PDF generation with **QuestPDF** (`CertificateGenerator`), and automated initial seeding from `SkillHub_Data.xlsx` (`DataSeeder`).
- **`SkillHub.Api`**:
  - Exposes RESTful controllers (`AuthController`, `WorkshopsController`, `EnrollmentsController`, `ReportsController`).
  - Configures Dependency Injection, JWT Bearer authentication, global error-handling middleware (`ExceptionHandlingMiddleware`), and interactive **Swagger UI** documentation.

---

## 2. Technologies, Libraries & Design Patterns

### Technologies & Libraries
- **Framework**: .NET 10 (`net10.0`) / ASP.NET Core Web API
- **Database**: PostgreSQL with `Npgsql.EntityFrameworkCore.PostgreSQL`
- **ORM**: Entity Framework Core 10 (Code-First Migrations)
- **Authentication & Authorization**: ASP.NET Core Identity + JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`)
- **Excel Processing**: `ClosedXML` (automated database seeding and bulk enrollment import)
- **PDF Generation**: `QuestPDF` (official enrollment certificate generation)
- **API Documentation**: `Swashbuckle.AspNetCore` (Swagger OpenAPI v1 with Bearer security scheme)

### Design Patterns Used
- **Clean Architecture (Onion/Layered Architecture)**
- **Repository Pattern** (`IWorkshopRepository`, `IEnrollmentRepository`)
- **Dependency Injection (IoC)**
- **DTO (Data Transfer Object) Pattern** with C# immutable `record` types
- **Global Exception Handling Middleware Pattern**
- **Automated Database Seeder Pattern**

---

## 3. Mandatory Business Rules Implemented

### Workshops
- **Instructor Validation**: Workshops can only be assigned to existing users with the `Employee` role.
- **Future Start Date**: A workshop must be created with a start date and time strictly in the future (`StartDateTime > DateTime.UtcNow`).
- **Valid Schedule Window**: The workshop end time must be strictly after the start time (`EndDateTime > StartDateTime`).
- **Unique Code**: Each workshop must have a unique `Code` (e.g., `FOT-001`).
- **Public Listing**: Displays only upcoming workshops (`StartDateTime > DateTime.UtcNow`) and dynamically computes `AvailableSpots = Capacity - ActiveEnrollmentsCount`.

### Enrollments
- **Capacity Control**: Enrollments cannot exceed the workshop's `Capacity` (counting only `Active` enrollments).
- **Duplicate Prevention**: A participant cannot hold two active enrollments in the same workshop simultaneously.
- **Schedule Conflict Prevention**: A participant cannot enroll in a workshop whose schedule overlaps with another active workshop enrollment (`w1.Start < w2.End && w2.Start < w1.End`).
- **Future Workshops Only**: Participants cannot enroll in a workshop that has already started.
- **Cancellation Rules**:
  - **Participants** can only cancel their own enrollments and only up to **24 hours prior** to the workshop start time.
  - **Employees** can cancel any enrollment without the 24-hour restriction, provided the workshop has not yet started.
  - Cancellations record `CancelledAt` (UTC timestamp) and `CancelledByUserId`.

### Bulk Excel Import (`POST /api/v1/enrollments/import`)
- Reads `.xlsx` files containing `ParticipantEmail` and `WorkshopCode` columns.
- Validates every enrollment business rule row-by-row.
- Persists valid enrollments while rejecting invalid rows and returning a detailed summary (`TotalProcessed`, `TotalSuccess`, `TotalFailed`, and row-level error descriptions).

### PDF Certificate Generation (`GET /api/v1/enrollments/{id}/certificate`)
- Restricted to `Active` enrollments.
- Generates an official A4 PDF document containing SkillHub branding, **"Constancia de inscripción"**, participant full name and email, workshop code and title, instructor name, workshop schedule, enrollment date, and certificate generation date.

### Analytical Report (`GET /api/v1/reports/top-workshops`)
- Returns workshops ordered descending by their active enrollment count (`ActiveEnrollmentsCount`).

---

## 4. Getting Started & Launch Instructions

### Prerequisites
- **.NET 10 SDK** installed
- **PostgreSQL** running on `localhost:5432`

### 1. Configure Database Connection
Verify the connection string in `src/SkillHub.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=skillhub_db;Username=postgres;Password=postgres"
}
```

### 2. Apply Entity Framework Core Migrations
From the repository root, run:

```bash
dotnet ef database update --project src/SkillHub.Infrastructure --startup-project src/SkillHub.Api
```

*(Note: The application also applies pending migrations and runs `DataSeeder` automatically on startup if the database tables are empty.)*

### 3. Build and Run the Application
```bash
dotnet build
dotnet run --project src/SkillHub.Api
```

- **Base URL**: `http://localhost:5241`
- **Swagger UI**: `http://localhost:5241/swagger` (also automatically redirected from `http://localhost:5241/`)

---

## 5. Default Credentials for Testing

On first startup, `DataSeeder` automatically populates the `Employee` and `Participant` roles along with instructors, participants, and workshops from `src/SkillHub.Api/SkillHub_Data.xlsx`.

### Default Employee (Instructor) Account
- **Email**: `andres.mejia@correo.com`
- **Password**: `Instructor2026*`
- **Role**: `Employee`

---

## 6. Complete API Endpoint Documentation

| HTTP Method | Endpoint | Authorization | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Public (`AllowAnonymous`) | Registers a new user with the `Participant` role and returns a JWT token. |
| `POST` | `/api/v1/auth/login` | Public (`AllowAnonymous`) | Authenticates an existing user (`Employee` or `Participant`) and returns a JWT token. |
| `GET` | `/api/v1/workshops` | Public (`AllowAnonymous`) | Lists all upcoming future workshops with calculated `availableSpots`. |
| `GET` | `/api/v1/workshops/{id}` | Public (`AllowAnonymous`) | Retrieves detailed information for a specific workshop by its GUID. |
| `POST` | `/api/v1/workshops` | `Bearer` (`Role: Employee`) | Creates a new workshop after validating future dates, schedule, instructor role, and unique code. |
| `GET` | `/api/v1/workshops/{id}/enrollments` | `Bearer` (`Role: Employee`) | Lists all enrollments (participants and statuses) for a specific workshop. |
| `POST` | `/api/v1/enrollments` | `Bearer` (`Role: Participant`) | Enrolls the authenticated participant into a future workshop. |
| `GET` | `/api/v1/enrollments/mine` | `Bearer` (`Role: Participant`) | Lists all enrollments belonging to the authenticated participant. |
| `PATCH` | `/api/v1/enrollments/{id}/cancel` | `Bearer` (`Participant` / `Employee`) | Cancels an active enrollment enforcing role-specific time rules and audit tracking. |
| `POST` | `/api/v1/enrollments/import` | `Bearer` (`Role: Employee`) | Bulk-imports enrollments from an uploaded `.xlsx` file with row-by-row validation. |
| `GET` | `/api/v1/enrollments/{id}/certificate` | `Bearer` (`Participant` / `Employee`) | Generates and downloads the PDF enrollment certificate (`Constancia de inscripción`) for an active enrollment. |
| `GET` | `/api/v1/reports/top-workshops` | `Bearer` (`Role: Employee`) | Returns workshops ranked descending by active enrollment count. |

---

## 7. Database Backup Deliverable

To export the full PostgreSQL database backup (schema + seeded data) to `skillhub_db_backup.sql`:

```bash
pg_dump -U postgres -h localhost -d skillhub_db --inserts > skillhub_db_backup.sql
```

The generated `skillhub_db_backup.sql` file is included at the root of this repository.

---

## 8. Author

- **Name**: Fernando Alonso Santos Puentes
- **Clan**: Clan Esthercita
- **Email**: fsantosp29@gmail.com
- **Program**: Riwi Software Development
