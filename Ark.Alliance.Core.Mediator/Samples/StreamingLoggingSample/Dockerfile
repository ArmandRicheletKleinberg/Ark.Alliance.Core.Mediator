FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["@Core/Ark.Alliance.Core.Mediator/Samples/StreamingLoggingSample/", "Sample/"]
COPY ["@Core/Ark.Alliance.Core.Mediator/Ark.Alliance.Core.Mediator.Messaging/", "Ark.Alliance.Core.Mediator.Messaging/"]
COPY ["@Core/Ark.Alliance.Core.Mediator/Ark.Alliance.Core.Mediator.IoC/", "Ark.Alliance.Core.Mediator.IoC/"]
COPY ["@Core/Ark/", "Ark/"]
RUN dotnet restore "Sample/StreamingLoggingSample.csproj"
RUN dotnet publish "Sample/StreamingLoggingSample.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "StreamingLoggingSample.dll"]
