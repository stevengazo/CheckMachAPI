# Usar la imagen oficial de .NET 10 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar csproj y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del proyecto y compilar
COPY . ./
RUN dotnet publish -c Release -o out

# Imagen de runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Puerto que expondrá la API
EXPOSE 5000
EXPOSE 5001

# Comando para ejecutar la API
ENTRYPOINT ["dotnet", "CheckMachAPI.dll"]
