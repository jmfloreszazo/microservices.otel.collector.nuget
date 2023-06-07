#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebAppForTest/WebAppForTest.csproj", "WebAppForTest/"]
RUN dotnet restore "WebAppForTest/WebAppForTest.csproj"
COPY . .
WORKDIR "/src/WebAppForTest"
RUN dotnet build "WebAppForTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAppForTest.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAppForTest.dll"]