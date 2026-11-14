# RaceDay Event Management System

## Project Overview

RaceDay is an event management system designed for South African road-running, walking and cycling events. The system provides a structured way for organisers to manage events and for participants to enrol in events and view their results.

## Project Purpose

The purpose of RaceDay is to provide a centralised system for managing road events, participants, event categories, enrolments and race results.

---

## User Roles

### Organiser

Organisers can:

- Register and log in to the system.
- View and update their own profile.
- Create, update and delete their own events.
- Specify the event type, date, location and distance.
- Create and manage categories for their events.
- View participant enrolments for their events.
- Capture, update and delete race results.

### Participant

Participants can:

- Register and log in to the system.
- View and update their own profile.
- View available events.
- View available event categories.
- Enrol in an event by selecting a category.
- View their own enrolments.
- View their own race results.
- Cancel their own enrolments.

---

## Part 1 - Planning and Database Design

Part 1 focused on planning the RaceDay system, designing the database and defining the RESTful API endpoints.

The following Part 1 files are available in the `docs` folder:

- `RaceDay_ERD.pdf`
- `RaceDay_API_Endpoint_Plan.pdf`
- `RaceDay_Database.sql`

### Database

The RaceDay database is implemented using Microsoft SQL Server.

The database contains the following main tables:

- Users
- EventTypes
- Events
- Categories
- Enrolments
- Results

The SQL script is available in the `docs` folder.

### Entity Relationship Diagram

The Entity Relationship Diagram (ERD) shows the RaceDay database entities, primary keys, foreign keys and relationships.

The ERD is available at:

`docs/RaceDay_ERD.pdf`

### API Endpoint Plan

The RESTful API endpoint plan describes the planned API operations for the RaceDay system.

The endpoint plan is available at:

`docs/RaceDay_API_Endpoint_Plan.pdf`

---

# Part 2 - RESTful API Development

Part 2 implements the RaceDay RESTful API using ASP.NET Core Web API, C#, Entity Framework Core and Microsoft SQL Server.

The API provides authentication, role-based access, profile management, event management, categories, event enrolments and race results.

## Authentication and Security

RaceDay provides registration and login functionality for both Organisers and Participants.

Security features include:

- Passwords are hashed before being stored.
- Passwords are never returned by API responses.
- Server-side session management stores the authenticated User ID and Role.
- Protected endpoints require an authenticated session.
- Role-based access prevents Participants from accessing Organiser functionality.
- Organisers can only manage resources associated with their own events.
- Participants can only access their own protected information.
- Logout clears the current user session.

---

## API Functionality

### Authentication

The authentication endpoints allow users to:

- Register as an Organiser or Participant.
- Log in using their email address and password.
- Log out and clear their session.

### User Profile

Both roles can:

- View their own profile.
- Update their own profile information.

### Events

Both roles can:

- View available events.
- View an individual event.

Organisers can:

- Create events.
- Update their own events.
- Delete their own events.

Each event captures:

- Name
- Description
- Date
- Location
- Distance
- Event type

Supported event types include running, walking and cycling events.

### Categories

Both roles can view available event categories.

Organisers can:

- Create categories for their own events.
- Update categories.
- Delete categories.

Categories can represent age or distance groups such as:

- Under 20
- Senior
- 10km
- 21km

### Event Enrolments

Participants can enrol in events by selecting a valid category.

The system records the relationship between:

- Participant
- Event
- Category

Participants cannot enrol in the same event more than once.

Organisers can view enrolments associated with their own events.

### Results

Organisers can capture race results for Participants enrolled in their events.

Results include:

- Finish time
- Finishing position

Participants can view their own race results.

---

## Swagger API Documentation

Swagger UI is integrated into the RaceDay API.

Swagger provides browser-based access to the API endpoints and displays:

- Endpoint purpose
- HTTP method
- Expected input
- Possible response codes
- Request body structure
- Response information

When the application is running in the Development environment, open the Swagger page from the URL launched by Visual Studio and navigate to `/swagger`.

The following API areas are available through Swagger:

- Authentication
- Profile
- Events
- Categories
- Enrolments
- Results

---

## Unit Testing

The RaceDay solution contains a separate xUnit test project:

`RaceDayAPI.Tests`

The unit tests use Entity Framework Core InMemory so that tests do not modify the production SQL Server database.

Tests cover required success and failure scenarios, including:

- Successful user registration.
- Invalid role registration.
- Duplicate email registration.
- Successful login.
- Invalid password login.
- Unknown email login.
- Session storage of User ID and Role.
- Organiser event creation.
- Participant rejection from Organiser functionality.
- Unauthenticated event-management requests.
- Successful Participant event enrolment.
- Organiser rejection from Participant enrolment functionality.
- Unauthenticated enrolment attempts.
- Duplicate event enrolment.
- Invalid event/category combinations.

### Running the Tests

Tests can be run in Visual Studio using:

`Test > Test Explorer > Run All Tests`

They can also be run from the command line from the solution directory:

```bash
dotnet test
```

---

## CI/CD

RaceDay uses GitHub Actions for Continuous Integration.

The workflow runs automatically when code is pushed to the `main` branch or when a pull request targets `main`.

The CI workflow:

1. Checks out the repository.
2. Validates the required Part 1 documentation.
3. Validates the RaceDay SQL script.
4. Sets up .NET 10.
5. Restores project dependencies.
6. Builds the RaceDay solution in Release mode.
7. Runs the automated unit tests.

A successful workflow confirms that the application builds and the automated tests pass.

### <img width="1018" height="77" alt="ci-part2-success" src="https://github.com/user-attachments/assets/49f7166a-4e48-4353-b4af-c7c645060718" />




---

## Technologies

RaceDay uses:

- C#
- ASP.NET Core Web API
- ASP.NET Core MVC (Part 3)
- .NET 10
- Entity Framework Core
- Entity Framework Core InMemory
- Microsoft SQL Server
- SQL Server Management Studio
- Swagger / OpenAPI
- Bootstrap 5 (Part 3)
- Azure Blob Storage (Part 3)
- Docker (Part 3)
- xUnit
- Git
- GitHub
- GitHub Actions

---

## Setup and Run Instructions

### Prerequisites

Install the following before running the project:

- Visual Studio
- .NET 10 SDK
- Microsoft SQL Server
- SQL Server Management Studio

### 1. Clone the Repository

Clone the RaceDay repository from GitHub:

```bash
git clone https://github.com/DomboMM/RaceDay.git
```

### 2. Open the Solution

Open the RaceDay solution in Visual Studio.

The solution contains:

- `RaceDayAPI` - ASP.NET Core Web API
- `RaceDayMVC` - ASP.NET Core MVC web application (Part 3)
- `RaceDayAPI.Tests` - xUnit test project

### 3. Configure SQL Server

Open SQL Server Management Studio and ensure the SQL Server instance used by RaceDay is available.

The Part 1 database script can be found at:

`docs/RaceDay_Database.sql`

### 4. Configure the Connection String

Open:

`RaceDayAPI/appsettings.json`

Ensure that the `RaceDayConnection` connection string points to the SQL Server instance being used on the local computer.

### 5. Restore Dependencies

Visual Studio should restore the NuGet packages automatically.

Alternatively, run:

```bash
dotnet restore
```

### 6. Build the Solution

In Visual Studio select:

`Build > Build Solution`

or press:

`Ctrl + Shift + B`

### 7. Run the API

Run the `RaceDayAPI` project from Visual Studio.

Swagger UI should open in the browser.

### 8. Test the API

Use Swagger UI to test the RaceDay endpoints.

For protected functionality:

1. Register a user.
2. Log in using the registered account.
3. Continue testing endpoints using the same browser session.
4. Test functionality according to the logged-in role.
5. Use the logout endpoint when finished.

### 9. Run Unit Tests

Open:

`Test > Test Explorer`

Select:

`Run All Tests`

All tests should pass successfully.

---

# Part 3 - Automation of Web Application

Part 3 adds the **ASP.NET Core MVC web application** (the front end) to RaceDay. The MVC application consumes the Part 2 API for every data operation and provides a role-aware interface for Organisers and Participants. **Azure Blob Storage** is added to the API for event banner images, and the MVC application is containerised with **Docker**.

## Part 3 Architecture

```text
Browser  -->  RaceDayMVC (MVC front end)  -->  RaceDayAPI (REST API)  -->  SQL Server
                                                      |
                                                      `-->  Azure Blob Storage
```

- **RaceDayMVC** contains controllers, views and view models only. It has no database access and no duplicated business logic.
- All API calls go through one service class, `RaceDayApiService`, which forwards the user's session cookie to the API.
- **The MVC application never communicates with Azure directly.** Files are sent to the API, and the API stores them in Blob Storage.

## Part 3 Features

### Organiser Interface

- **Dashboard:** summary of the Organiser's events, total enrolments, enrolments per event and upcoming dates.
- **Events:** create, edit and delete events (name, description, date, location, distance, event type) with an optional banner image.
- **Categories:** add age or distance categories to each event.
- **Enrolments:** view every participant enrolled in an event with their category and status, and confirm or cancel an enrolment.
- **Results:** capture finish times and finishing positions after an event.

### Participant Interface

- **Home / Browse Events:** upcoming events.
- **Event details:** full event information, banner image and available categories, with an **Enter Event** button.
- **My Events:** all enrolments with colour-coded status (green Confirmed, amber Pending, red Cancelled).
- **My Results:** personal race history with finish time and position.
- **Profile:** view and update personal information.

### General

- Register, log in and log out using session-based authentication.
- Role-specific navigation menus.
- Unauthenticated users are redirected to the Login page.
- Invalid input is handled with friendly messages instead of raw errors.
- Responsive Bootstrap layout with a consistent RaceDay brand (navy and orange).

## Part 3 Authentication and Roles

- Users register and log in through the MVC application, which calls the API authentication endpoints.
- The MVC application keeps the user's ID and role in its own session and forwards the API session cookie on every request.
- The navigation menu differs for Organisers and Participants.
- Every protected action checks the session and redirects unauthenticated users to Login.
- The API also checks ownership, so an Organiser can only manage their own events and enrolments.
- Logging out clears the session.

## Part 3 Database Changes

Run these statements on the `RaceDayDB` database after the Part 1 script:

```sql
-- Enrolment status (Pending / Confirmed / Cancelled)
IF COL_LENGTH('dbo.Enrolments', 'Status') IS NULL
    ALTER TABLE dbo.Enrolments
    ADD Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Enrolments_Status DEFAULT 'Pending';

-- Event banner image URL
IF COL_LENGTH('dbo.Events', 'BannerUrl') IS NULL
    ALTER TABLE dbo.Events ADD BannerUrl NVARCHAR(500) NULL;
```

## Part 3 Configuration and Secrets

**No secrets are stored in this repository.**

| Setting | Location | Value |
|---|---|---|
| `ConnectionStrings:RaceDayConnection` | `RaceDayAPI/appsettings.json` | SQL Server connection (Windows authentication) |
| `AzureStorage:AccountName` and `ContainerName` | `RaceDayAPI/appsettings.json` | Storage account and container names (not secret) |
| `AzureStorage:ConnectionString` | .NET User Secrets | Storage account key, kept outside the repository |
| `RaceDayApi:BaseUrl` | `RaceDayMVC/appsettings.json` | Address of the API that the MVC application calls |

Store the Azure key locally with User Secrets:

```bash
cd RaceDayAPI
dotnet user-secrets set "AzureStorage:ConnectionString" "<your-storage-connection-string>"
```

## Part 3 Running Locally

Start the API first, then the MVC application.

```bash
# Terminal 1 - API (Swagger: https://localhost:7136/swagger)
dotnet run --project RaceDayAPI --launch-profile https

# Terminal 2 - MVC (https://localhost:7149)
dotnet run --project RaceDayMVC --launch-profile https
```

In Visual Studio, set both projects as startup projects (`Solution > Configure Startup Projects > Multiple startup projects`) with `RaceDayAPI` above `RaceDayMVC`.

## Docker

The MVC application runs inside a Docker container and calls the API running on the host computer.

**1. Start the API on the host (HTTP profile):**

```bash
dotnet run --project RaceDayAPI --launch-profile http
```

**2. Build the image:**

```bash
docker build -t raceday-mvc ./RaceDayMVC
```

**3. Run the container (single command):**

```bash
docker run --rm -p 8080:8080 -e RaceDayApi__BaseUrl=http://host.docker.internal:5223/ raceday-mvc
```

**4. Open the application:** http://localhost:8080

How it works:

- The `Dockerfile` uses a multi-stage build. The .NET 10 SDK image restores and publishes the app, and the smaller ASP.NET runtime image runs it.
- `RaceDayApi__BaseUrl` overrides `RaceDayApi:BaseUrl` from `appsettings.json`, so the container behaves the same as the local application but reaches the API through `host.docker.internal`.
- The container exposes port 8080.

## Azure Blob Storage

Event banner images are stored in the `event-images` container, and all file handling goes through the API:

1. The Organiser chooses an image on the Create or Edit Event form in the MVC application.
2. The MVC application sends the file to the API using `POST /api/Events/{id}/banner` (multipart form data).
3. The API checks that the user is a logged-in Organiser who owns the event, and that the file is a JPG, PNG or WEBP image of 2 MB or less.
4. The API uploads the file to Azure Blob Storage and saves the blob URL in `Events.BannerUrl`.
5. The MVC application displays that URL on the event cards and the event detail page.

The container uses blob-level anonymous read access. An image can be displayed only by someone who has its exact link, and the container contents cannot be listed.

## Part 3 CI/CD

The GitHub Actions workflow in `.github/workflows/ci.yml` was updated so that it also builds the `RaceDayMVC` project.

**Latest successful build:**

> [PASTE THE PART 3 GREEN GITHUB ACTIONS SCREENSHOT HERE, for example `![ci-part3-success](docs/ci-part3-success.png)`]

## Part 3 Test Accounts

| Role | Email | Password |
|---|---|---|
| Organiser | [email] | [password] |
| Participant | [email] | [password] |

## Part 3 Screenshots

> [ADD 3 TO 5 SCREENSHOTS HERE: Home page, Organiser dashboard, Event details, My Events with status badges, My Results]

---

## Repository Structure

```text
RaceDay
|
|-- .github
|   `-- workflows
|       `-- ci.yml
|
|-- docs
|   |-- RaceDay_ERD.pdf
|   |-- RaceDay_API_Endpoint_Plan.pdf
|   |-- RaceDay_Database.sql
|   |-- ci-part2-success.png
|   `-- ci-part3-success.png
|
|-- RaceDayAPI
|   |-- Controllers
|   |-- Data
|   |-- DTOs
|   |-- Models
|   |-- Program.cs
|   `-- RaceDayAPI.csproj
|
|-- RaceDayMVC
|   |-- Controllers
|   |-- Models
|   |-- Services
|   |-- Views
|   |-- wwwroot
|   |-- Program.cs
|   |-- RaceDayMVC.csproj
|   `-- Dockerfile
|
|-- RaceDayAPI.Tests
|
|-- README.md
`-- RaceDayAPI.slnx
```

---

## Video Presentation

### Part 1 Video

The Part 1 video presentation is available here:

https://youtu.be/B5_M79is2sk

### Part 2 Video

The Part 2 video will demonstrate:

- Running the RaceDay API.
- Swagger UI.
- Registration and login.
- Session-based authentication.
- Organiser functionality.
- Participant functionality.
- Event management.
- Categories.
- Event enrolments.
- Results.
- Unit tests.
- GitHub Actions CI/CD.
- Code structure and explanation.

Part 2 YouTube Video:https://youtu.be/GBfjOYkFZMs.

### Part 3 Video

The Part 3 video demonstrates the full running application, including:

- The MVC pages for Organisers and Participants.
- Session-based login and role-specific navigation.
- Azure Blob Storage image upload through the API.
- The Docker setup and run command.
- GitHub Actions CI/CD.
- MVC structure and code explanation.

Part 3 YouTube Video (unlisted): []

---

## Author

Mpho Dombo

RaceDay Event Management System
