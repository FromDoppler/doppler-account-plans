#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Doppler.AccountPlans/Doppler.AccountPlans.csproj", "Doppler.AccountPlans/"]
RUN dotnet restore "Doppler.AccountPlans/Doppler.AccountPlans.csproj"
COPY . .
WORKDIR "/src/Doppler.AccountPlans"
RUN dotnet build "Doppler.AccountPlans.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Doppler.AccountPlans.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Doppler.AccountPlans.dll"]
