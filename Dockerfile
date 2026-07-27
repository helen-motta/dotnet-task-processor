FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY TaskProcessor.csproj .
COPY Tests/Unit/TaskProcessor.UnitTests.csproj Tests/Unit/

RUN dotnet restore Tests/Unit/TaskProcessor.UnitTests.csproj

COPY . .


FROM build AS unit-tests

CMD ["dotnet", "test", "Tests/Unit/TaskProcessor.UnitTests.csproj", "--configuration", "Release", "--no-restore"]

FROM build AS publish

RUN dotnet publish TaskProcessor.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TaskProcessor.dll"]