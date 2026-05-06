FROM mcr.microsoft.com/dotnet/sdk:10.0 AS frontend-build
WORKDIR /src
COPY Frontend ./Frontend
COPY Shared ./Shared
RUN cd Frontend && dotnet publish -c Release -o /frontend-publish

# Verify Frontend publish output
RUN echo "=== Frontend publish wwwroot ===" && ls -la /frontend-publish/wwwroot/ && \
    echo "=== Frontend _framework ===" && ls /frontend-publish/wwwroot/_framework/ | head -10

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN cd Backend && dotnet publish -c Release -o /backend-publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy Backend publish first
COPY --from=backend-build /backend-publish .

# Then overlay Frontend wwwroot files (this is the key step!)
COPY --from=frontend-build /frontend-publish/wwwroot ./wwwroot

# Verify
RUN echo "=== Final /app/wwwroot ===" && ls -la /app/wwwroot/ | head -10 && \
    echo "=== Final _framework ===" && ls /app/wwwroot/_framework/ | head -10

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
