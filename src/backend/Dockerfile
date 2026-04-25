# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY NotesCool.sln ./
COPY src/backend/NotesCool.Api/NotesCool.Api.csproj src/backend/NotesCool.Api/
COPY src/backend/NotesCool.Shared/NotesCool.Shared.csproj src/backend/NotesCool.Shared/
COPY src/backend/NotesCool.Notes/NotesCool.Notes.csproj src/backend/NotesCool.Notes/
COPY src/backend/NotesCool.Tasks/NotesCool.Tasks.csproj src/backend/NotesCool.Tasks/

# Restore dependencies
RUN dotnet restore NotesCool.sln

# Copy the rest of the source code
COPY src/backend/ src/backend/

# Build and publish
RUN dotnet publish src/backend/NotesCool.Api/NotesCool.Api.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NotesCool.Api.dll"]
