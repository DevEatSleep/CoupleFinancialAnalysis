FROM mcr.microsoft.com/dotnet/sdk:10.0 AS frontend-build
WORKDIR /src
COPY Frontend ./Frontend
COPY Shared ./Shared
RUN cd Frontend && dotnet publish -c Release

# Extensive diagnostics to find where files ended up
RUN echo "=== Frontend/bin structure ===" && find /src/Frontend/bin -type f -name "*icudt*" 2>/dev/null || echo "No icudt files found" && \
    echo "=== Frontend/bin full tree ===" && find /src/Frontend/bin -type d | head -30 && \
    echo "=== Looking for wwwroot ===" && find /src/Frontend -name "wwwroot" -type d && \
    echo "=== Contents of publish/wwwroot ===" && ls -la /src/Frontend/bin/Release/net10.0/publish/wwwroot/ 2>&1 | head -20

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY . .
RUN mkdir -p Backend/wwwroot
RUN dotnet restore
RUN cd Backend && dotnet publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy Backend publish
COPY --from=backend-build /src/Backend/bin/Release/net10.0/publish .

# Try to find and copy Frontend files
RUN echo "=== Attempting to find Frontend wwwroot ===" && find /src/Frontend -path "*publish*" -name "wwwroot" -type d && \
    echo "=== Copying Frontend files ===" && cp -rv /src/Frontend/bin/Release/net10.0/publish/wwwroot/* wwwroot/ 2>&1 | head -20 || echo "Copy failed"

# Final check
RUN echo "=== Final app/wwwroot ===" && ls -la wwwroot/ 2>&1 && echo "=== _framework check ===" && ls -la wwwroot/_framework/ 2>&1 || echo "NO _framework DIRECTORY"

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
