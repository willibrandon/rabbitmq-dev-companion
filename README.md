# RabbitMQ Developer's Companion

A visual tool for learning, designing, and debugging RabbitMQ topologies. Built with .NET 9 and React.

## Features

- **Topology Designer**: Drag-and-drop interface for RabbitMQ exchanges and queues
- **Message Simulator**: Test message flows and failure scenarios
- **Pattern Analysis**: Find common messaging patterns and anti-patterns
- **Learning Modules**: Interactive tutorials for RabbitMQ concepts
- **Code Generation**: Export configurations and .NET code snippets
- **Live Monitoring**: Track queue depths and message flow

## Setup

### Requirements

- .NET 9 SDK
- Node.js 18+
- Docker Desktop
- PostgreSQL 15+
- RabbitMQ 3.11+

### Quick Start

1. Start services:
```bash
docker-compose up -d
```

2. Run API:
```bash
dotnet run --project Companion.Api
```

3. Run frontend:
```bash
cd companion-frontend
npm install
npm start
```

Visit http://localhost:3000 and log in with:
- Username: `admin`
- Password: `admin123`

## Projects

- `Companion.Api`: Web API with JWT auth
- `Companion.Core`: Domain models and interfaces
- `Companion.Infrastructure`: Data access and external services
- `Companion.Simulator`: Message flow simulation
- `Companion.Debug`: Debugging tools
- `Companion.Learning`: Tutorial modules
- `Companion.Patterns`: Pattern analysis
- `companion-frontend`: React application

## Development

Build:
```bash
dotnet build
```

Test:
```bash
dotnet test
```

Update database:
```bash
cd Companion.Api
dotnet ef database update
```

## Docs

- [API](http://localhost:5052/swagger)

## Security

JWT authentication with roles:
- Admin: Full access
- Editor: Create/modify topologies
- Viewer: Read-only

## License

MIT License - see [LICENSE](LICENSE)
