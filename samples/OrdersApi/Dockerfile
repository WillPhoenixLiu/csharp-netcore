FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["samples/OrdersApi/OrdersApi.csproj", "samples/OrdersApi/"]
COPY ["samples/Shared/Shared.csproj", "samples/Shared/"]
COPY ["src/OpenTracing.Contrib.NetCore/OpenTracing.Contrib.NetCore.csproj", "src/OpenTracing.Contrib.NetCore/"]

RUN dotnet restore "samples/OrdersApi/OrdersApi.csproj"
COPY . .
WORKDIR "/src/samples/OrdersApi"
RUN dotnet build "OrdersApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrdersApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrdersApi.dll"]