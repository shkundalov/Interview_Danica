# Setup
Dependencies:
- .NET SDK 10.0+
NuGet packages (run dotnet restore)
- Microsoft.AspNetCore.Authentication.JwtBearer 10.0.5
- Microsoft.EntityFrameworkCore.Sqlite 10.0
- Swashbuckle.AspNetCore 10.1.7

# Run the project
Run the batch file Run_Tests.bat

It will:
- Build the solution
- Start the Server at http://localhost:5254
- Run 18 unit tests and 2 integration tests
- Keep the server running so Swagger UI is available at: http://localhost:5254/swagger

# Design Overview
Idempotancy
- Customer POST: Idempotency based on email, where same email returns existing record with state 200
- Template POST: Idempotency based on name, where same name returns existing record with state 200
- POST returns 200 (not 201 Created) for both new and existing resources - implemented for idempotency reasons
- DELETE: Always returns 200 based on model state in database, same as Amazon

Validation
- Stored in Validators/EmailValidator.cs
- Email regex: ^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$
- Invalid format returns 400

Error statuses
- 400 BadRequest: Validation failures (invalid email, duplicate name in PUT)
- 404 NotFound: Resource doesn't exist
- 200 OK: Successful DELETE / never existed
- 204 NoContent: Successful PUT

Architecture
3-Layer Design:
- Controllers (HTTP endpoints) - CustomersController, TemplatesController, CommunicationController
- Services (Business logic) - CustomerService, TemplateService, CommunicationService
- Data (EF Core SQLite database) - DbContext

Dependency Injection
- Services registered in Program.cs using AddScoped

Database
- SQLite with auto-created via EnsureCreated()
Files in use:
- interview_danica.db
- interview_danica.db-shm
- interview_danica.db-wal

# Testing
18 unit tests:
- Test business logic
- Use in-memory database
- Cover CRUD operations, email validation, idempotency logic

2 integration tests:
- Test end-to-end API workflow
- Use HttpClient and running Server API
- Clear database before each run
- Validate 15 steps for Customers workflow + 11 steps for Templates

Template Placeholders:
Support {{Name}} and {{Email}} placeholders.
Example: "String {{Name}} string {{Email}} string"

SQLite operates:
- interview_danica.db (main database)
- interview_danica.db-shm (shared memory file)
- interview_danica.db-wal (write-ahead log)

# Project structure
InterviewDanica.Api
- Program.cs
- Controllers/
- Services/
- Data/
- Models/
- DTOs/
- Validators/

InterviewDanica.Api.Tests/
- CustomerServiceTests
- TemplateServiceTests
- IdempotencyServiceTests
- CommunicationServiceTests
- UnitTest1.cs
- Utils

# Notes
- Email Validation uses a simplified regex for interview clarity. In real worl scenario a System.Net.Mail.MailAddress, fluentvalidation or RFC 5322 would be used
- [Authorize] attributes are commented out because Swashbuckle 10 does not fully support .NET 10, requried for Swagger tests since
- Authentication is hardcoded due to time limits
- Communication API logs output to console instead of sending real emails
