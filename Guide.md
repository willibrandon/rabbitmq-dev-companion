Below is a step-by-step implementation guide broken into logical, bite-sized chunks, designed to help you (and any AI collaborator) implement the entire RabbitMQ Developer’s Companion in a structured manner. Each chunk concludes with instructions for verification, testing, and a Git commit step before moving on. This approach ensures that each portion of the system is properly built, tested, and version-controlled, minimizing the risk of “wandering” into incomplete or unverified territory.

⸻

Implementation Guide

Chunk 1: Project Initialization & Environment Setup
	1.	Install Prerequisites
	•	.NET 8 SDK
	•	Docker (Desktop or Engine)
	•	RabbitMQ (can be run via Docker or installed locally)
	•	Node.js (if using React) or ensure .NET Blazor workloads are installed.
	2.	Create a New Git Repository
	•	Initialize a new repository, for example:

git init rabbitmq-dev-companion
cd rabbitmq-dev-companion


	3.	Create a .NET Solution Structure
	•	For instance:

dotnet new sln --name RabbitMQDevCompanion


	•	Add subprojects as placeholders:
	1.	Companion.Api (ASP.NET Core project for REST/SignalR endpoints)
	2.	Companion.Core (Class library for domain models and shared logic)
	3.	Companion.Simulator (Class library for message simulation logic)
	4.	Companion.Debug (Class library for debugging/tracing features)
	5.	Companion.Learning (Class library for tutorial modules)
	6.	Companion.Patterns (Class library for pattern analysis)
	•	(Optional) If you decide on a microservices approach, each library may become a separate ASP.NET service; for the MVP, libraries inside one solution is often simpler.

	4.	Add Each Project to the Solution

cd rabbitmq-dev-companion
dotnet new classlib -o Companion.Core
dotnet new webapi -o Companion.Api
dotnet new classlib -o Companion.Simulator
dotnet new classlib -o Companion.Debug
dotnet new classlib -o Companion.Learning
dotnet new classlib -o Companion.Patterns

dotnet sln add Companion.Core/Companion.Core.csproj
dotnet sln add Companion.Api/Companion.Api.csproj
dotnet sln add Companion.Simulator/Companion.Simulator.csproj
dotnet sln add Companion.Debug/Companion.Debug.csproj
dotnet sln add Companion.Learning/Companion.Learning.csproj
dotnet sln add Companion.Patterns/Companion.Patterns.csproj


	5.	Set Up Basic Project References
	•	For example, Companion.Api references Companion.Core (and so on):

dotnet add Companion.Api reference Companion.Core
dotnet add Companion.Api reference Companion.Simulator
dotnet add Companion.Api reference Companion.Debug
dotnet add Companion.Api reference Companion.Learning
dotnet add Companion.Api reference Companion.Patterns


	•	You may also need references among the libraries themselves, depending on how you split logic.

	6.	Build & Confirm Initial Compilation

dotnet build



Verification, Testing & Commit
	•	Verification: Ensure the solution builds with zero errors.
	•	Testing: No tests yet, but confirm you can run dotnet run from Companion.Api and see a basic ASP.NET Core app responding on localhost.
	•	Git Commit:

git add .
git commit -m "Initial solution structure with projects and references"



⸻

Chunk 2: Core Domain Models & Shared Utilities
	1.	Define Core Domain Models in Companion.Core
	•	Example:
	•	Topology (with fields for exchanges, queues, bindings)
	•	Exchange, Queue, Binding classes
	•	Common enumerations (ExchangeType, etc.)
	•	Keep them serializable (e.g., [Serializable] or [DataContract]) if you plan to store or transmit them via API.
	2.	Add Shared Utilities
	•	Logging helpers
	•	Exceptions (e.g., InvalidTopologyException)
	•	Common result wrappers (e.g., OperationResult<T>) for consistent response handling
	3.	Create a Basic Unit Test Project (optional but recommended)
	•	dotnet new xunit -o Companion.Tests
	•	Add references to Companion.Core in Companion.Tests.
	•	Write minimal tests (e.g., to ensure domain objects are instantiated correctly).
	4.	Check Build & Run Tests

dotnet build
dotnet test



Verification, Testing & Commit
	•	Verification: Models compile, basic tests pass.
	•	Testing: Confirm domain objects can be serialized/deserialized if needed.
	•	Git Commit:

git add .
git commit -m "Add core domain models and initial unit tests"



⸻

Chunk 3: Topology & Validation Services
	1.	Implement TopologyService in Companion.Core (or separate library if you prefer):
	•	Methods:
	•	ValidateTopology(Topology topology) -> ValidationResult
	•	NormalizeTopology(Topology topology) -> Topology (optional: ensures consistent naming/format)
	•	Possibly a method to apply a topology to a real RabbitMQ environment (will rely on the Management API in a future chunk).
	2.	Define ValidationResult structure:

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; }
}

	•	This object will be used to display warnings or errors in the frontend.

	3.	Unit Tests for Validation
	•	Test direct exchange route correctness, fanout exchange requiring no routing key, etc.
	4.	Add Endpoints in Companion.Api
	•	POST /api/designer/validate that accepts a Topology and returns ValidationResult.
	•	Keep it simple, e.g.:

[ApiController]
[Route("api/designer")]
public class DesignerController : ControllerBase
{
    private readonly ITopologyService _topologyService;

    public DesignerController(ITopologyService topologyService)
    {
        _topologyService = topologyService;
    }

    [HttpPost("validate")]
    public ActionResult<ValidationResult> ValidateTopology([FromBody] Topology topology)
    {
        var result = _topologyService.ValidateTopology(topology);
        return Ok(result);
    }
}


	5.	Integration Test (optional):
	•	Use WebApplicationFactory<Program> or similar approach to call POST /api/designer/validate and verify responses.

Verification, Testing & Commit
	•	Verification:
	•	Local build is successful.
	•	Unit tests for topology validation pass.
	•	Test the API endpoint with a sample JSON topology payload.
	•	Testing:
	•	Confirm you get correct validation responses (valid topologies vs. ones with errors).
	•	Git Commit:

git add .
git commit -m "Implement topology service, validation logic, and designer endpoints"



⸻

Chunk 4: RabbitMQ Management API Integration
	1.	Add a RabbitMqManagementClient in Companion.Core or a new project (e.g., Companion.Infrastructure if you prefer layering):
	•	Use HttpClient to call the official Management HTTP endpoints:
	•	GET /api/exchanges
	•	GET /api/queues
	•	GET /api/bindings
	•	Provide methods like GetTopologyFromBroker() -> Topology.
	2.	Configuration for Management API
	•	In appsettings.json, store RabbitMqManagement:BaseUrl and credentials.
	•	Create a strongly typed RabbitMqSettings class:

public class RabbitMqSettings
{
    public string BaseUrl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}


	3.	Dependency Injection
	•	In Program.cs or Startup.cs:

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqManagement"));
builder.Services.AddHttpClient<IRabbitMqManagementClient, RabbitMqManagementClient>();


	4.	Implement Endpoints
	•	GET /api/topologies/from-broker that uses RabbitMqManagementClient.GetTopologyFromBroker() and returns a Topology.
	5.	Test Locally
	•	Spin up a local RabbitMQ container with the management plugin:

docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.11-management


	•	Confirm your credentials and base URL (e.g., http://localhost:15672/api).
	•	Call your endpoint; it should return the real broker’s topology in your domain model format.

Verification, Testing & Commit
	•	Verification:
	•	Make sure you can retrieve a real topology from RabbitMQ management.
	•	Confirm correct domain mapping (exchanges, queues, bindings).
	•	Testing:
	•	Integration tests or Postman requests verifying the JSON response shape.
	•	Git Commit:

git add .
git commit -m "Integrate RabbitMQ Management API for topology retrieval"



⸻

Chunk 5: Message Flow Simulation (Core)
	1.	Create MessageFlowService in Companion.Simulator:
	•	Public methods, e.g.:
	•	StartSimulation(SimulationConfig config)
	•	StopSimulation(string simulationId)
	•	Use the official .NET RabbitMQ.Client library to publish messages according to the config (routing keys, exchange, etc.).
	2.	Define SimulationConfig

public class SimulationConfig
{
    public string TopologyId { get; set; }
    public int MessageCount { get; set; }
    public int MessageSizeBytes { get; set; }
    public string RoutingKeyPattern { get; set; }
    // Possibly more, e.g. concurrency, consumer failures, etc.
}


	3.	In Companion.Api:
	•	Create a SimulationsController with:
	•	POST /api/simulations/start (returns a simulationId)
	•	GET /api/simulations/{simulationId}/status
	4.	Real-Time Updates
	•	Add a SignalR hub (SimulationHub) to push progress to the frontend.
	•	Each time a message is published or consumed, broadcast metrics (e.g., how many messages remain).
	5.	Basic Testing
	•	Might do local integration tests that start a simulation, then read queue lengths from RabbitMQ to confirm messages arrived.

Verification, Testing & Commit
	•	Verification:
	•	Build and run a simple simulation with your local RabbitMQ.
	•	Check that the published message count matches the queue(s) message count.
	•	Optionally, consume them with a test consumer to see they truly flow end-to-end.
	•	Testing:
	•	Confirm StartSimulation triggers the correct publishing logic.
	•	Check logs or real queue sizes in the RabbitMQ management console.
	•	Git Commit:

git add .
git commit -m "Implement basic message flow simulation with SignalR updates"



⸻

Chunk 6: Debugging Tools (Dead Letter / Tracing)
	1.	Implement DebugService in Companion.Debug:
	•	Methods to:
	•	Fetch dead-lettered messages from known dead-letter queues.
	•	Trace a message by ID or correlation ID (if you embed a custom header or property).
	•	For advanced tracing, consider hooking into RabbitMQ Tracing plugin or a custom approach with a “firehose” exchange.
	2.	Add Endpoints in Companion.Api:
	•	GET /api/debug/deadLetters
	•	GET /api/debug/trace/{messageId}
	3.	Optionally:
	•	Extend your simulation or consumer code to set a unique MessageId in the AMQP properties.
	•	Capture these in a log or a small database table.
	4.	Unit / Integration Tests
	•	Manually produce a message that fails in a consumer so it lands in DLQ.
	•	Confirm DebugService can retrieve and show it.

Verification, Testing & Commit
	•	Verification:
	•	Ensure you can cause a message to dead-letter, retrieve it from the debug endpoint, and see meaningful data.
	•	Testing:
	•	Start a simulation with a forced consumer error rate. Check that some messages are dead-lettered, and confirm the debug endpoint surfaces them.
	•	Git Commit:

git add .
git commit -m "Add debugging tools for dead-letter and message tracing"



⸻

Chunk 7: Pattern Analysis & Recommendations
	1.	Create PatternAnalysisService in Companion.Patterns:
	•	Example:
	•	AnalyzeTopology(Topology topology) -> AnalysisResult.
	•	Implement rules (e.g., check if an exchange has zero bindings, if a queue is unbounded, etc.).
	2.	Define AnalysisResult

public class AnalysisResult
{
    public List<AnalysisFinding> Findings { get; set; }
}

public class AnalysisFinding
{
    public string Type { get; set; } // warning, info, error
    public string Message { get; set; }
}


	3.	Add Endpoint in Companion.Api:
	•	POST /api/analysis/run accepting a Topology body or referencing a stored topology.
	•	Returns the AnalysisResult.
	4.	Test
	•	Provide known topologies with mistakes (unbound exchange, missing DLQ, etc.).
	•	Check that the system yields correct warnings or suggestions.

Verification, Testing & Commit
	•	Verification:
	•	Confirm you receive correct findings for known topologies.
	•	Testing:
	•	Add a unit test suite for each pattern check.
	•	Possibly a real integration test referencing a real broker (optional).
	•	Git Commit:

git add .
git commit -m "Implement pattern analysis service and API endpoint"



⸻

Chunk 8: Configuration Generator
	1.	Create ConfigGeneratorService in a new or existing library:
	•	Methods to:
	•	Generate Docker Compose or Kubernetes YAML for a RabbitMQ instance.
	•	Generate minimal .NET code for producers/consumers.
	•	Consider using a template engine like Scriban or Razor templates for generating text files.
	2.	Add Endpoint
	•	POST /api/config-generator/{topologyId} returning a zipped archive or JSON structure with the final config text.
	3.	Verify Basic Outputs
	•	For instance, the Docker Compose snippet referencing environment variables, volumes, etc.
	•	.NET code snippet with the correct exchange/queue definitions.

Verification, Testing & Commit
	•	Verification:
	•	Call your endpoint with a sample topology; confirm you get back valid YAML or code that references your queue names, etc.
	•	Testing:
	•	Manually run the generated Docker Compose to see if it starts RabbitMQ as intended.
	•	Git Commit:

git add .
git commit -m "Add configuration generator for Docker/Kubernetes and .NET code snippets"



⸻

Chunk 9: Learning Modules
	1.	Learning Module Content in Companion.Learning:
	•	Define a structure like:

public class LearningModule
{
    public string ModuleId { get; set; }
    public string Title { get; set; }
    public List<LearningStep> Steps { get; set; }
}

public class LearningStep
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string ValidationCriteria { get; set; }
}


	2.	Module Orchestration
	•	A service that loads module JSON from disk or DB, checks user’s actions (e.g., “did the user create a queue named X?”) to unlock next step.
	3.	API Endpoints
	•	GET /api/modules to list available modules.
	•	GET /api/modules/{moduleId} to retrieve a specific module’s data.
	•	POST /api/modules/{moduleId}/progress to update user’s progress.
	4.	Testing
	•	Create one or two sample modules for “Pub/Sub Basics” and “Topic Exchange Basics.”
	•	Use unit tests or manual checks to confirm the user flow.

Verification, Testing & Commit
	•	Verification:
	•	Confirm the modules can be listed, retrieved, and steps are validated properly.
	•	Testing:
	•	Manually step through a sample module in a test environment (UI next chunk).
	•	Git Commit:

git add .
git commit -m "Implement learning modules with sample content and progress API"



⸻

Chunk 10: Frontend Application (React or Blazor)

The frontend can be large. Consider doing it in smaller sub-chunks if needed.

	1.	Initialize Frontend
	•	For React (TypeScript):

npx create-react-app companion-frontend --template typescript


	•	Or for Blazor:

dotnet new blazorwasm -o Companion.Frontend


	2.	Structure
	•	Pages:
	•	Designer/Topology Page
	•	Simulations Page
	•	Debug/Analysis Page
	•	Learning Modules Page
	•	Services:
	•	A typed client for calling your .NET APIs (axios in React or HttpClient in Blazor).
	3.	Topology Visualization
	•	If React, use D3.js, react-flow, or another library to render nodes and edges.
	•	If Blazor, consider a library like BlazorSvgHelper or manually manage <svg> elements.
	4.	Real-Time Communications
	•	Use SignalR client library:
	•	In React: @microsoft/signalr
	•	In Blazor: built-in SignalR client package.
	5.	Testing the Frontend
	•	Start the backend (dotnet run in Companion.Api) and the frontend in another terminal (npm start for React or dotnet run for Blazor).
	•	Ensure you can visualize and manipulate a simple topology, run simulations, etc.

Verification, Testing & Commit
	•	Verification:
	•	Confirm basic UI flows:
	•	Designer page loads, user can create a queue or exchange visually.
	•	Simulation triggers and displays some form of real-time metrics or log.
	•	Learning module steps show a tutorial.
	•	Testing:
	•	Use manual UI tests or automated browser tests with tools like Cypress (for React) or Playwright.
	•	Git Commit:

git add .
git commit -m "Implement initial frontend for topology designer, simulator UI, and learning modules"



⸻

Chunk 11: Security & Authentication
	1.	Add Authentication
	•	For a production-ready solution, consider JWT or OAuth:
	•	Use AddAuthentication(JwtBearerDefaults.AuthenticationScheme) in your ASP.NET Core Program.cs.
	•	If it’s simpler, you might just do API keys for now.
	2.	Configure Authorization
	•	Add roles or policy-based checks for certain endpoints (e.g., designing topologies might require an “Editor” role).
	3.	Frontend Integration
	•	If using JWT:
	•	Login Page (collect credentials, request a token).
	•	Store the token in memory or local storage, attach to requests.
	4.	Test
	•	Attempt to call a restricted endpoint with no token — should fail with 401.
	•	Call with the correct role — should succeed.

Verification, Testing & Commit
	•	Verification:
	•	Ensure unauthorized requests are blocked.
	•	Testing:
	•	Postman or automated integration tests for endpoints requiring authentication.
	•	Git Commit:

git add .
git commit -m "Add basic JWT/role-based authentication and secure endpoints"



⸻

Chunk 12: Persistence & Database Setup
	1.	Choose a Database
	•	For storing topologies, user data, learning progress, etc. (e.g., PostgreSQL).
	2.	Add EF Core or Another ORM
	•	dotnet add Companion.Api package Microsoft.EntityFrameworkCore
	•	dotnet add Companion.Api package Npgsql.EntityFrameworkCore.PostgreSQL (for Postgres)
	3.	Create DbContext in Companion.Core or a dedicated Companion.Data:

public class CompanionDbContext : DbContext
{
    public DbSet<TopologyEntity> Topologies { get; set; }
    // ...
}


	4.	Migrations
	•	dotnet ef migrations add InitialCreate -p Companion.Api -s Companion.Api
	•	dotnet ef database update -p Companion.Api -s Companion.Api
	5.	Test with Basic CRUD
	•	Save a TopologyEntity to the DB, read it back, ensure everything works.

Verification, Testing & Commit
	•	Verification:
	•	Confirm that the database is created, tables exist, migrations run.
	•	Testing:
	•	Automated test verifying a round trip (create, retrieve, update, delete).
	•	Git Commit:

git add .
git commit -m "Implement database persistence using EF Core with initial migrations"



⸻

Chunk 13: Dockerization
	1.	Create a Dockerfile for the .NET API:

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore RabbitMQDevCompanion.sln
RUN dotnet build RabbitMQDevCompanion.sln -c Release -o /app/build

FROM build AS publish
RUN dotnet publish RabbitMQDevCompanion.sln -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Companion.Api.dll"]


	2.	Create a docker-compose.yml for Dev/Local:

version: '3.8'
services:
  companion-api:
    build: .
    ports:
      - "5000:80"
    depends_on:
      - rabbitmq
      - db
  rabbitmq:
    image: rabbitmq:3.11-management
    ports:
      - "5672:5672"
      - "15672:15672"
  db:
    image: postgres:14
    environment:
      POSTGRES_USER: dev
      POSTGRES_PASSWORD: dev
      POSTGRES_DB: companiondb
    ports:
      - "5432:5432"


	3.	Run Compose

docker-compose up --build


	4.	Validate
	•	Check http://localhost:5000/swagger or any endpoint to confirm the container is working.

Verification, Testing & Commit
	•	Verification:
	•	The system fully runs inside Docker containers.
	•	Testing:
	•	Confirm you can run simulations, load topologies from the broker container, etc.
	•	Git Commit:

git add .
git commit -m "Add Dockerfiles and docker-compose for local deployment"



⸻

Chunk 14: Performance & Load Testing
	1.	Add a Load Testing Script
	•	Could be a simple Locust, k6, or JMeter scenario that hits POST /api/simulations/start repeatedly with varying message loads.
	2.	Optimize
	•	Check logs for throughput, memory usage.
	•	Scale out containers if needed.
	•	Add caching or concurrency improvements in the .NET code.
	3.	Document
	•	Summarize recommended hardware, scaling strategies.

Verification, Testing & Commit
	•	Verification:
	•	Confirm the companion tool can handle enough load for typical usage scenarios.
	•	Testing:
	•	Generate a performance test report for your code to ensure stable performance.
	•	Git Commit:

git add .
git commit -m "Add performance testing scripts and initial optimizations"



⸻

Chunk 15: Final QA & Production Deployment
	1.	CI/CD Pipeline
	•	Use GitHub Actions, Azure DevOps, or GitLab CI to automate:
	•	Build
	•	Test
	•	Docker build & push to registry
	•	Possibly a staging environment for final validations.
	2.	Production Deployment
	•	If you’re using Kubernetes, create a Helm chart.
	•	If you prefer a simpler approach, push Docker images to a host (e.g., ECS, Docker Swarm, or a VM).
	3.	Final Documentation
	•	README with usage instructions.
	•	Postman collection or reference docs for APIs.
	•	Quick start guides for new developers.

Verification, Testing & Commit
	•	Verification:
	•	Ensure the pipeline runs successfully (build, test, container push).
	•	Deploy to a real environment and confirm everything is operational (topology design, simulation, debugging, learning modules).
	•	Testing:
	•	Run end-to-end acceptance tests.
	•	Confirm no major security or performance issues remain.
	•	Git Commit:

git add .
git commit -m "Finalize QA, add CI/CD pipeline, and prepare for production deployment"



⸻

Conclusion

By following these 15 logical chunks and completing each step with verification, testing, and a Git commit, you will methodically build a full production-ready RabbitMQ Developer’s Companion. This process ensures clarity, maintains momentum, and helps both human and AI collaborators stay on track.

Good luck with your development journey!