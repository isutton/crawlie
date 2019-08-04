FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./*.sln .
COPY ./Crawlie.Server/*.csproj ./Crawlie.Server/
COPY ./Crawlie.Server.IntegrationTests/*.csproj ./Crawlie.Server.IntegrationTests/
COPY ./Crawlie.Client.App/*.csproj ./Crawlie.Client.App/
COPY ./Crawlie.Client.IntegrationTests/*.csproj ./Crawlie.Client.IntegrationTests/
COPY ./Crawlie.Contracts/*.csproj ./Crawlie.Contracts/
RUN dotnet restore

# copy everything else and build app
COPY . .

RUN dotnet test Crawlie.Client.IntegrationTests -c Release
RUN dotnet publish Crawlie.Client.App -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/Crawlie.Client.App/out/* ./
ENTRYPOINT ["dotnet", "Crawlie.Client.App.dll"]
