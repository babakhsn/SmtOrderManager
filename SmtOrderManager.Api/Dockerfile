# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + projects
COPY *.sln ./
COPY SmtOrderManager.Domain/*.csproj SmtOrderManager.Domain/
COPY SmtOrderManager.Application/*.csproj SmtOrderManager.Application/
COPY SmtOrderManager.Infrastructure/*.csproj SmtOrderManager.Infrastructure/
COPY SmtOrderManager.Api/*.csproj SmtOrderManager.Api/

RUN dotnet restore SmtOrderManager.Api/SmtOrderManager.Api.csproj

# Copy everything and publish
COPY . .
RUN dotnet publish SmtOrderManager.Api/SmtOrderManager.Api.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# SQLite file will be stored under /data
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__AppDb=Data\ Source=/data/smt_orders.db
ENV RunMigrationsOnStartup=true


COPY --from=build /app/publish ./
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmtOrderManager.Api.dll"]
