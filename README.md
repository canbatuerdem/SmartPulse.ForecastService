## Setup & Execution Instructions

### Prerequisites

-   Docker Desktop (with Docker Compose support)
-   No local database or message broker installation is required

------------------------------------------------------------------------

### Running the System

From the solution root directory, execute:

``` bash
docker compose up --build
```

This command will: - Build the API container - Start SQL Server and
RabbitMQ containers - Automatically apply database migrations - Seed the
required reference data - Bring the system to a runnable state

------------------------------------------------------------------------

### Service Endpoints

#### API (Swagger UI)

-   URL:\
    http://localhost:8080/swagger

All available endpoints can be tested directly through the Swagger UI.

------------------------------------------------------------------------

#### RabbitMQ (Management UI)

-   URL:\
    http://localhost:15672
-   Username: `guest`
-   Password: `guest`

This interface can be used to monitor exchanges, queues, and published
events.

------------------------------------------------------------------------

#### SQL Server

-   Host: `localhost,1433`
-   Username: `sa`
-   Password: `YourStrong!Passw0rd`
-   Database: `ForecastDb`

The database is created automatically on first startup via Entity
Framework Core migrations.

------------------------------------------------------------------------

### Notes on Production Readiness

This solution is a greenfield, assignment-focused implementation
intended for demonstration and evaluation purposes. Production
deployment concerns such as CI/CD pipelines, infrastructure
provisioning, secret management, scaling strategies, and security
hardening are intentionally out of scope and would normally be handled
by dedicated DevOps processes.
