FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /App

COPY . ./

RUN dotnet restore

RUN dotnet publish -c Release -o /out

RUN ls -la /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /App

COPY --from=build /out ./

ENTRYPOINT ["dotnet", "PocMsGateway.dll"]