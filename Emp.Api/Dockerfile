# Build context must be the solution root: docker build -f Emp.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Emp.Api/Emp.Api.csproj", "Emp.Api/"]
COPY ["Emp.Models/Emp.Models.csproj", "Emp.Models/"]
RUN dotnet restore "Emp.Api/Emp.Api.csproj"
COPY . .
WORKDIR "/src/Emp.Api"
RUN dotnet publish "Emp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Emp.Api.dll"]
