FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UniversityLostAndFound.csproj", "."]
RUN dotnet restore "UniversityLostAndFound.csproj"
COPY . .
RUN dotnet publish "UniversityLostAndFound.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "dotnet UniversityLostAndFound.dll --urls http://0.0.0.0:${PORT:-8080}"]
