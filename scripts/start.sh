#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting RabbitMQ Developer's Companion...${NC}\n"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}Error: Docker is not running. Please start Docker first.${NC}"
    exit 1
fi

# Check if RabbitMQ container is running
if ! docker ps | grep -q "rabbitmq"; then
    echo -e "${YELLOW}RabbitMQ container not found. Starting RabbitMQ...${NC}"
    docker run -d \
        --name rabbitmq-dev-companion \
        -p 5672:5672 \
        -p 15672:15672 \
        rabbitmq:3.11-management

    # Wait for RabbitMQ to be ready
    echo -e "${YELLOW}Waiting for RabbitMQ to be ready...${NC}"
    sleep 10
else
    echo -e "${GREEN}RabbitMQ is already running.${NC}"
fi

# Build the .NET solution
echo -e "\n${YELLOW}Building .NET solution...${NC}"
dotnet build

if [ $? -ne 0 ]; then
    echo -e "${RED}Build failed. Please check the errors above.${NC}"
    exit 1
fi

# Run the API
echo -e "\n${GREEN}Starting the API...${NC}"
dotnet run --project Companion.Api

# Cleanup function
cleanup() {
    echo -e "\n${YELLOW}Shutting down...${NC}"
    # Add any cleanup tasks here if needed
}

# Set up trap for cleanup on script exit
trap cleanup EXIT

# Keep the script running
wait 