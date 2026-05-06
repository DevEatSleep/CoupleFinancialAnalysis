FROM mcr.microsoft.com/dotnet/sdk:10.0 AS frontend-build
WORKDIR /src
COPY Frontend ./Frontend
COPY Shared ./Shared
RUN cd Frontend && dotnet publish -c Release

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY . .
RUN dotnet restore

# Copy Frontend source wwwroot to Backend before publishing
RUN mkdir -p Backend/wwwroot && \
    cp -r Frontend/wwwroot/* Backend/wwwroot/

RUN cd Backend && dotnet publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy Backend publish (which now includes Frontend wwwroot)
COPY --from=backend-build /src/Backend/bin/Release/net10.0/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
