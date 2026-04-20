FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/GoodHamburger.Domain/GoodHamburger.Domain.csproj", "src/GoodHamburger.Domain/"]
COPY ["src/GoodHamburger.Application/GoodHamburger.Application.csproj", "src/GoodHamburger.Application/"]
COPY ["src/GoodHamburger.Infrastructure/GoodHamburger.Infrastructure.csproj", "src/GoodHamburger.Infrastructure/"]
COPY ["src/GoodHamburger.API/GoodHamburger.API.csproj", "src/GoodHamburger.API/"]
COPY ["NuGet.Config", "."]
RUN dotnet restore "src/GoodHamburger.API/GoodHamburger.API.csproj"
COPY . .
RUN dotnet publish "src/GoodHamburger.API/GoodHamburger.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "GoodHamburger.API.dll"]
