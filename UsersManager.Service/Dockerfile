FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["UsersManager.Service/UsersManager.Service.csproj", "UsersManager.Service/"]
RUN dotnet restore "UsersManager.Service/UsersManager.Service.csproj"
COPY . .
WORKDIR "/src/UsersManager.Service"
RUN dotnet build "UsersManager.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UsersManager.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UsersManager.Service.dll"]
