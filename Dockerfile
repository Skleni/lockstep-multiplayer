#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["MultiplayerTest.HolePunchServer/MultiplayerTest.HolePunchServer.csproj", "MultiplayerTest.HolePunchServer/"]
RUN dotnet restore "MultiplayerTest.HolePunchServer/MultiplayerTest.HolePunchServer.csproj"
COPY . .
WORKDIR "/src/MultiplayerTest.HolePunchServer"
RUN dotnet build "MultiplayerTest.HolePunchServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MultiplayerTest.HolePunchServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5463/udp
ENTRYPOINT ["dotnet", "MultiplayerTest.HolePunchServer.dll", "5463"]