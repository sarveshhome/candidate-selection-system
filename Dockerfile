FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CandidateSelectionSystem.csproj", "."]
RUN dotnet restore "CandidateSelectionSystem.csproj"
COPY . .
RUN dotnet build "CandidateSelectionSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CandidateSelectionSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CandidateSelectionSystem.dll"]