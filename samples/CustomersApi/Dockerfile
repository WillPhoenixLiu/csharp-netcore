FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["samples/CustomersApi/CustomersApi.csproj", "samples/CustomersApi/"]
COPY ["samples/Shared/Shared.csproj", "samples/Shared/"]
COPY ["src/OpenTracing.Contrib.NetCore/OpenTracing.Contrib.NetCore.csproj", "src/OpenTracing.Contrib.NetCore/"]

RUN dotnet restore "samples/CustomersApi/CustomersApi.csproj"
COPY . .
WORKDIR "/src/samples/CustomersApi"
RUN dotnet build "CustomersApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomersApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomersApi.dll"]