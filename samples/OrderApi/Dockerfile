FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["samples/OrderApi/OrderApi.csproj", "samples/OrderApi/"]
COPY ["src/OpenTracing.Contrib.NetCore/OpenTracing.Contrib.NetCore.csproj", "src/OpenTracing.Contrib.NetCore/"]
COPY ["samples/Shared/Shared.csproj", "samples/Shared/"]
RUN dotnet restore "samples/OrderApi/OrderApi.csproj"
COPY . .
WORKDIR "/src/samples/OrderApi"
RUN dotnet build "OrderApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderApi.dll"]