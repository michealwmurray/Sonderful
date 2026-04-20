# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Sonderful/Sonderful.API/Sonderful.API.csproj Sonderful.API/
RUN dotnet restore Sonderful.API/Sonderful.API.csproj
COPY Sonderful/Sonderful.API/ Sonderful.API/
RUN dotnet publish Sonderful.API/Sonderful.API.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "Sonderful.API.dll"]
