FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["samples/FrontendWeb/FrontendWeb.csproj", "samples/FrontendWeb/"]
COPY ["samples/Shared/Shared.csproj", "samples/Shared/"]
COPY ["src/OpenTracing.Contrib.NetCore/OpenTracing.Contrib.NetCore.csproj", "src/OpenTracing.Contrib.NetCore/"]

RUN dotnet restore "samples/FrontendWeb/FrontendWeb.csproj"
COPY . .
WORKDIR "/src/samples/FrontendWeb"
RUN dotnet build "FrontendWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FrontendWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FrontendWeb.dll"]