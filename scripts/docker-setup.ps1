# PowerShell script to set up Docker containers for RabbitMQ Developer's Companion

# Function to check if Docker is running
function Test-DockerRunning {
    try {
        $null = docker info
        return $true
    } catch {
        return $false
    }
}

# Function to check if container exists (running or not)
function Test-ContainerExists {
    param([string]$ContainerName)
    $exists = docker ps -a --format "{{.Names}}" | Select-String -Pattern "^$ContainerName$"
    return $null -ne $exists
}

# Function to check if container is running
function Test-ContainerRunning {
    param([string]$ContainerName)
    $running = docker ps --format "{{.Names}}" | Select-String -Pattern "^$ContainerName$"
    return $null -ne $running
}

# Setup color output
$Green = [System.ConsoleColor]::Green
$Yellow = [System.ConsoleColor]::Yellow
$Red = [System.ConsoleColor]::Red

Write-Host "Setting up Docker containers for RabbitMQ Developer's Companion..." -ForegroundColor $Green

# Check if Docker is running
if (-not (Test-DockerRunning)) {
    Write-Host "Error: Docker is not running. Please start Docker first." -ForegroundColor $Red
    exit 1
}

# Setup RabbitMQ
if (Test-ContainerExists -ContainerName "rabbitmq") {
    Write-Host "RabbitMQ container already exists." -ForegroundColor $Yellow
    
    # Check if container is running
    if (-not (Test-ContainerRunning -ContainerName "rabbitmq")) {
        Write-Host "Starting existing RabbitMQ container..." -ForegroundColor $Yellow
        docker start rabbitmq
    } else {
        Write-Host "RabbitMQ container is already running." -ForegroundColor $Green
    }
} else {
    Write-Host "Creating and starting RabbitMQ container..." -ForegroundColor $Yellow
    docker run -d `
        --name rabbitmq `
        -p 5672:5672 `
        -p 15672:15672 `
        rabbitmq:3.11-management
}

# Setup Postgres
if (Test-ContainerExists -ContainerName "rabbitmq-dev-companion-postgres-1") {
    Write-Host "Postgres container already exists." -ForegroundColor $Yellow
    
    # Check if container is running
    if (-not (Test-ContainerRunning -ContainerName "rabbitmq-dev-companion-postgres-1")) {
        Write-Host "Starting existing Postgres container..." -ForegroundColor $Yellow
        docker start rabbitmq-dev-companion-postgres-1
    } else {
        Write-Host "Postgres container is already running." -ForegroundColor $Green
    }
} else {
    Write-Host "Setting up Postgres with docker-compose..." -ForegroundColor $Yellow
    # Using docker-compose to set up Postgres with the right configuration
    docker-compose up -d postgres
}

# Wait for services to be ready
Write-Host "`nWaiting for services to be ready..." -ForegroundColor $Yellow
Start-Sleep -Seconds 5

# Check if containers are running
Write-Host "`nChecking container status:" -ForegroundColor $Green
docker ps --format "table {{.Names}}`t{{.Status}}`t{{.Ports}}" | Where-Object { $_ -match "rabbitmq|postgres" }

Write-Host "`nSetup complete!" -ForegroundColor $Green
Write-Host "RabbitMQ Management UI: http://localhost:15672 (guest/guest)" -ForegroundColor $Yellow
Write-Host "Postgres: localhost:5432 (postgres/postgres)" -ForegroundColor $Yellow 