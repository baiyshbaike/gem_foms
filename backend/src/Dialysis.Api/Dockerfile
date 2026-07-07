FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["backend/src/Dialysis.Api/Dialysis.Api.csproj", "backend/src/Dialysis.Api/"]
COPY ["backend/src/Dialysis.Application/Dialysis.Application.csproj", "backend/src/Dialysis.Application/"]
COPY ["backend/src/Dialysis.Contracts/Dialysis.Contracts.csproj", "backend/src/Dialysis.Contracts/"]
COPY ["backend/src/Dialysis.Domain/Dialysis.Domain.csproj", "backend/src/Dialysis.Domain/"]
COPY ["backend/src/Dialysis.Infrastructure/Dialysis.Infrastructure.csproj", "backend/src/Dialysis.Infrastructure/"]

RUN dotnet restore "backend/src/Dialysis.Api/Dialysis.Api.csproj"

COPY . .

RUN dotnet publish "backend/src/Dialysis.Api/Dialysis.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dialysis.Api.dll"]