FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./*.sln .
COPY ./Crawlie.Server/*.csproj ./Crawlie.Server/
COPY ./Crawlie.Server.IntegrationTests/*.csproj ./Crawlie.Server.IntegrationTests/
RUN dotnet restore

# copy everything else and build app
COPY . .

RUN dotnet test Crawlie.Server.IntegrationTests -c Release
RUN dotnet publish Crawlie.Server -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/Crawlie.Server/out/* ./
ENTRYPOINT ["dotnet", "Crawlie.Server.dll"]
