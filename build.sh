#!/usr/bin/env bash
set -e

CONFIGURATION="Release"

echo -e "\033[1;36m🚀 Starting Build Process ($CONFIGURATION)\033[0m"

echo -e "\n\033[1;33m📦 Restoring dependencies...\033[0m"
dotnet restore

echo -e "\n\033[1;33m🧹 Verifying code format...\033[0m"
dotnet format --verify-no-changes || { echo -e "\033[1;31mFormat check failed. Run 'dotnet format' to fix.\033[0m"; exit 1; }

echo -e "\n\033[1;33m🔨 Building solution...\033[0m"
dotnet build --configuration $CONFIGURATION --no-restore

echo -e "\n\033[1;33m🧪 Running unit tests...\033[0m"
dotnet test --configuration $CONFIGURATION --no-build

echo -e "\n\033[1;32m✅ Build completed successfully!\033[0m"
