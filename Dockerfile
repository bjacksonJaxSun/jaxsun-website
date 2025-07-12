# Use the official .NET 8.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000

# Use the .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["JaxSun.Web/JaxSun.Web.csproj", "JaxSun.Web/"]
RUN dotnet restore "JaxSun.Web/JaxSun.Web.csproj"
COPY . .
WORKDIR "/src/JaxSun.Web"
RUN dotnet build "JaxSun.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JaxSun.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENTRYPOINT ["dotnet", "JaxSun.Web.dll"]