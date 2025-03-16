FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -p:AssemblyName=app -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "app.dll"]