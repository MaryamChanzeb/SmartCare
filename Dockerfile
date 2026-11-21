FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SmartClinicManagement.API/SmartClinicManagement.API.csproj SmartClinicManagement.API/
COPY SmartClinicManagement.Application/SmartClinicManagement.Application.csproj SmartClinicManagement.Application/
COPY SmartClinicManagement.Domain/SmartClinicManagement.Domain.csproj SmartClinicManagement.Domain/
COPY SmartClinicManagement.Infrastructure/SmartClinicManagement.Infrastructure.csproj SmartClinicManagement.Infrastructure/
COPY SmartClinicManagement.Persistence/SmartClinicManagement.Persistence.csproj SmartClinicManagement.Persistence/
COPY SmartClinicManagement.UI/SmartClinicManagement.UI.csproj SmartClinicManagement.UI/

RUN dotnet restore SmartClinicManagement.API/SmartClinicManagement.API.csproj

COPY . .
WORKDIR /src/SmartClinicManagement.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SmartClinicManagement.API.dll"]
