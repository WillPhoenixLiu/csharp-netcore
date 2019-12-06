FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["samples/CustomerApi/CustomerApi.csproj", "samples/CustomerApi/"]
COPY ["src/OpenTracing.Contrib.NetCore/OpenTracing.Contrib.NetCore.csproj", "src/OpenTracing.Contrib.NetCore/"]
COPY ["samples/Shared/Shared.csproj", "samples/Shared/"]
RUN dotnet restore "samples/CustomerApi/CustomerApi.csproj"
COPY . .
WORKDIR "/src/samples/CustomerApi"
RUN dotnet build "CustomerApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomerApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerApi.dll"]