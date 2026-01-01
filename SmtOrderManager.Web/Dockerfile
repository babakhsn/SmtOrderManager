# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln ./
COPY SmtOrderManager.Domain/*.csproj SmtOrderManager.Domain/
COPY SmtOrderManager.Application/*.csproj SmtOrderManager.Application/
COPY SmtOrderManager.Infrastructure/*.csproj SmtOrderManager.Infrastructure/
COPY SmtOrderManager.Api/*.csproj SmtOrderManager.Api/
COPY SmtOrderManager.Web/*.csproj SmtOrderManager.Web/

RUN dotnet restore SmtOrderManager.Web/SmtOrderManager.Web.csproj

COPY . .
RUN dotnet publish SmtOrderManager.Web/SmtOrderManager.Web.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmtOrderManager.Web.dll"]
