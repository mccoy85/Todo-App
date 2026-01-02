# Todo App

A full-stack Todo application with a .NET 9 API backend and React + Ant Design frontend.

## Project Structure

```
TodoApp/
├── todo-api/                  # .NET 9 backend
│   ├── src/
│   │   ├── TodoApi.Api/           # ASP.NET Core Web API
│   │   ├── TodoApi.Core/          # Business logic, entities, interfaces, DTOs
│   │   └── TodoApi.Infrastructure/# Data access, repositories, EF Core
│   ├── tests/
│   │   ├── TodoApi.Api.Tests/     # Integration tests
│   │   ├── TodoApi.Core.Tests/    # Service unit tests
│   │   └── TodoApi.Infrastructure.Tests/ # Repository tests
│   └── TodoApi.sln
└── todo-client/               # React + TypeScript frontend
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 24](https://nodejs.org/)
- npm or yarn

## Quick Start

### 1. Run the API

```bash
cd todo-api/src/TodoApi.Api
dotnet run
```

The API will start at `http://localhost:5121`

### 2. Run the Frontend

```bash
cd todo-client
npm install
npm run dev
```

The frontend will start at `http://localhost:3000` (configurable via `VITE_UI_HOST`/`VITE_UI_PORT`)

### 3. View API Documentation

Open `http://localhost:5121/api/docs` to see the Swagger UI with all available endpoints.

The OpenAPI specification is available at `http://localhost:5121/api/docs/v1/swagger.json`.

## API Endpoints

| Method | Endpoint                 | Description                                         |
| ------ | ------------------------ | --------------------------------------------------- |
| GET    | `/api/todo`              | Get all todos (with filtering, sorting, pagination) |
| GET    | `/api/todo/{id}`         | Get a specific todo                                 |
| POST   | `/api/todo`              | Create a new todo                                   |
| PUT    | `/api/todo/{id}`         | Update a todo                                       |
| PATCH  | `/api/todo/{id}/toggle`  | Toggle completion status                            |
| DELETE | `/api/todo/{id}`         | Delete a todo                                       |
| GET    | `/api/todo/deleted`      | Get deleted todos                                   |
| PATCH  | `/api/todo/{id}/restore` | Restore a deleted todo                              |

### Query Parameters (GET /api/todo)

- `isCompleted` - Filter by completion status (true/false)
- `priority` - Filter by priority (0=Low, 1=Medium, 2=High)
- `sortBy` - Sort field (title, duedate, priority, iscompleted, createdat)
- `sortDescending` - Sort direction (true/false)
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 10, max: 100)

## Running Tests

### Backend Tests

```bash
cd todo-api

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TodoApi.Api.Tests
dotnet test tests/TodoApi.Core.Tests
dotnet test tests/TodoApi.Infrastructure.Tests
```

### Frontend Tests

```bash
cd todo-client

# Run tests in watch mode
npm test

# Run tests once
npm run test:run

# Run with coverage
npm run test:coverage
```

### Frontend Build

```bash
cd todo-client
npm run build
```

## Docker

Run both with Docker Compose (from the repo root):

```bash
docker compose up --build
```

Note: the Docker Compose setup configures the frontend to call the API via `/api` and proxies to the `api` container.

## Architecture

### Backend

- **Api Layer** (`TodoApi.Api`) - Controllers, middleware, configuration
- **Core Layer** (`TodoApi.Core`) - Entities, DTOs, interfaces, services
- **Infrastructure Layer** (`TodoApi.Infrastructure`) - EF Core, repositories, data access

### Key Design Decisions

1. **Repository Pattern** - Generic `IRepository<T>` with specialized `ITodoRepository` for custom queries. Keeps data access isolated from services and makes testing simpler.
2. **DTOs** - Separate request/response DTOs (never expose EF entities directly). Enables validation at the boundary and stable API contracts.
3. **Validation** - FluentValidation rules with consistent error responses, letting the API reject bad input early with clear messages.
4. **Global Error Handling** - Middleware returns a uniform `ErrorResponse` shape and the API config uses the same shape for validation errors.
5. **Soft Deletes** - Todo items are marked deleted and filtered by default, with a restore endpoint to recover items without data loss.
6. **Client Data Caching** - The frontend caches full lists in SWR and performs client-side filtering/sorting/pagination for fast UI updates.
7. **Config-Driven Ports** - API/UI hosts and ports are configurable via environment settings to make local setup flexible.
8. **Dev-Only Migrations** - Database migrations auto-apply only in Development to avoid implicit changes in prod environments.
9. **Error Contract** - Validation and exception errors share a consistent response shape for simpler client handling.
10. **Refresh Cadence** - SWR refreshes cached lists every 60 seconds to balance freshness and network load.

### Frontend

- **React 19** with TypeScript
- **Ant Design** for UI components
- **SWR** for data fetching with caching and invalidation
- **Vite** for fast development and builds
- **Vitest** + **React Testing Library** for unit and component tests

### Key Features

- Loading, error, and empty states handled throughout
- Optimistic UI updates with proper error handling
- Form validation matching backend constraints
- Responsive filtering and sorting

## Configuration

### API (todo-api/src/TodoApi.Api/appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=todo.db"
  },
  "Api": {
    "Host": "localhost",
    "Port": 5121
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Frontend (todo-client/.env)

| Variable              | Description                                                    | Default                     |
| --------------------- | -------------------------------------------------------------- | --------------------------- |
| `VITE_API_HOST`       | API server hostname                                            | `localhost`                 |
| `VITE_API_PORT`       | API server port                                                | `5121`                      |
| `VITE_API_BASE_URL`   | Full API base URL (overrides host/port)                        | `http://localhost:5121/api` |
| `VITE_API_BATCH_SIZE` | Number of items fetched per API request when loading all todos | `100`                       |
| `VITE_UI_HOST`        | Frontend dev server hostname                                   | `localhost`                 |
| `VITE_UI_PORT`        | Frontend dev server port                                       | `3000`                      |

Example `.env` file:

```
VITE_API_HOST=localhost
VITE_API_PORT=5121
VITE_API_BASE_URL=http://localhost:5121/api
VITE_API_BATCH_SIZE=100
VITE_UI_HOST=localhost
VITE_UI_PORT=3000
```

## Trade-offs and Assumptions

1. **SQLite** - Chosen for simplicity and portability. For production, consider PostgreSQL or SQL Server.
2. **No Authentication** - This is a demo app. Real apps would need auth middleware.
3. **Simple Service Layer** - No CQRS/MediatR to keep complexity low for a todo app.
4. **Validation at the API Boundary** - FluentValidation runs on incoming requests; deeper business logic is kept minimal to avoid duplicating rule layers. In a more mature product, there would need to be deeper business logic implemented.
5. **Soft Deletes Lifecycle** - Items can be restored. Longer term, higher usage app would need a retention policy for this.
6. **Client-Side Full-List Caching** - The UI caches full lists for fast filtering/sorting; this trades higher memory usage for fewer API calls. Currently we limit the number of records we can pull to minimize overhead. In the long term we would want to implement a different way to determine which records to pull or we could use a hybrid pagination between the UI and API.

## Scalability

1. **UI** - Record count could become an issue with the client side caching and filtering/sorting.
2. **API** - Would need to be scaled out behind a load balancer. If the app evolves into something with shared tasks, we would need to have a mechanism for real-time updates (ws/grpc).
3. **Database** - Would need a production shared database that allows us to independently scale the API and UI and support connection pooling. Database could need to be distributed depending on scale. More complete indexes may need to be added based on the most common data queried.

## What I'd Add With More Time

1. **User authentication** with JWT
2. **Real-time updates** with SignalR
3. **CI/CD pipeline** with GitHub Actions
4. **Rate limiting** and request throttling
5. **Logging** with Serilog and structured logs
6. **bulk actions** for deleting and completing
