#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}Setting up Docker containers for RabbitMQ Developer's Companion...${NC}\n"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}Error: Docker is not running. Please start Docker first.${NC}"
    exit 1
fi

# Function to check if container exists (running or not)
container_exists() {
    docker ps -a --format '{{.Names}}' | grep -q "^$1$"
}

# Setup RabbitMQ
if container_exists "rabbitmq"; then
    echo -e "${YELLOW}RabbitMQ container already exists.${NC}"
    
    # Check if container is running
    if ! docker ps --format '{{.Names}}' | grep -q "^rabbitmq$"; then
        echo -e "${YELLOW}Starting existing RabbitMQ container...${NC}"
        docker start rabbitmq
    else
        echo -e "${GREEN}RabbitMQ container is already running.${NC}"
    fi
else
    echo -e "${YELLOW}Creating and starting RabbitMQ container...${NC}"
    docker run -d \
        --name rabbitmq \
        -p 5672:5672 \
        -p 15672:15672 \
        rabbitmq:3.11-management
fi

# Setup Postgres
if container_exists "rabbitmq-dev-companion-postgres-1"; then
    echo -e "${YELLOW}Postgres container already exists.${NC}"
    
    # Check if container is running
    if ! docker ps --format '{{.Names}}' | grep -q "^rabbitmq-dev-companion-postgres-1$"; then
        echo -e "${YELLOW}Starting existing Postgres container...${NC}"
        docker start rabbitmq-dev-companion-postgres-1
    else
        echo -e "${GREEN}Postgres container is already running.${NC}"
    fi
else
    echo -e "${YELLOW}Setting up Postgres with docker-compose...${NC}"
    # Using docker-compose to set up Postgres with the right configuration
    docker-compose up -d postgres
fi

# Wait for services to be ready
echo -e "\n${YELLOW}Waiting for services to be ready...${NC}"
sleep 5

# Check if containers are running
echo -e "\n${GREEN}Checking container status:${NC}"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep -E "rabbitmq|postgres"

echo -e "\n${GREEN}Setup complete!${NC}"
echo -e "${YELLOW}RabbitMQ Management UI: http://localhost:15672 (guest/guest)${NC}"
echo -e "${YELLOW}Postgres: localhost:5432 (postgres/postgres)${NC}" 