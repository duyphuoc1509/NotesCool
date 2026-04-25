#!/bin/bash

# NotesCool Deployment Script

set -e

echo ">>> Starting deployment for NotesCool..."

# 1. Pull latest changes (optional, if in a git repo)
if [ -d ".git" ]; then
    echo ">>> Pulling latest changes from Git..."
    git pull origin dev
fi

# 2. Build and restart containers
echo ">>> Building and starting containers..."
docker compose down
docker compose up -d --build

# 3. Clean up unused images
echo ">>> Cleaning up old images..."
docker image prune -f

# 4. Verification
echo ">>> Checking service status..."
docker compose ps

echo ">>> Deployment completed successfully!"
echo ">>> Frontend is available on port 80"
echo ">>> Backend API is available on port 8080"
