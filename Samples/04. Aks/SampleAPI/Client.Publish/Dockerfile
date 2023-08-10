#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Client.Publish/SampleAPI.Client.Publish.csproj", "SampleAPI.Client.Publish/"]
RUN dotnet restore "SampleAPI.Client.Publish/SampleAPI.Client.Publish.csproj"
COPY . .
WORKDIR "/src/Client.Publish"
RUN dotnet build "SampleAPI.Client.Publish.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SampleAPI.Client.Publish.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleAPI.Client.Publish.dll"]