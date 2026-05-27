# Build context must be the solution root: docker build -f EMP.Web/Dockerfile .
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EMP.Web/EMP.Web.csproj", "EMP.Web/"]
COPY ["Emp.Api/Emp.Api.csproj", "Emp.Api/"]
COPY ["Emp.Models/Emp.Models.csproj", "Emp.Models/"]
RUN dotnet restore "EMP.Web/EMP.Web.csproj"
COPY . .
WORKDIR "/src/EMP.Web"
RUN dotnet publish "EMP.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EMP.Web.dll"]
