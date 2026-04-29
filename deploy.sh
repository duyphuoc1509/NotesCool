#!/bin/bash

# NotesCool Deployment Script
# Managed by Systemd or manual execution

set -e

echo ">>> Starting deployment for NotesCool..."

# 1. Pull latest changes (optional, if in a git repo)
if [ -d ".git" ]; then
    echo ">>> Pulling latest changes from Git..."
    git pull origin dev
fi

# Ensure /etc/sysconfig/notescool exists if running directly
if [ ! -f "/etc/sysconfig/notescool" ] && [ -z "$NOTESCOOL_ENV_FILE" ]; then
    echo ">>> WARNING: /etc/sysconfig/notescool not found. Fallback to default env vars."
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
echo ">>> Frontend is available on port 10001"
echo ">>> Backend API is available on port 10002"
