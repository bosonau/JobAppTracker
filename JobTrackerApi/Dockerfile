#Build stage: use full sdk to compile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS Build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
copy . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

#Runtime stage: uses the smaller ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "JobTrackerApi.dll"]