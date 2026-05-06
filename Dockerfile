FROM mcr.microsoft.com/dotnet/sdk:10.0 AS frontend-build
WORKDIR /src
COPY Frontend ./Frontend
COPY Shared ./Shared
RUN cd Frontend && dotnet publish -c Release

# Check where files ended up
RUN echo "=== Looking for Frontend dist ===" && find /src/Frontend -name "wwwroot" -type d && find /src/Frontend -name "icudt_EFIGS*" 2>/dev/null | head -5

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY . .
# Pre-create wwwroot before building
RUN mkdir -p Backend/wwwroot
RUN dotnet restore
RUN cd Backend && dotnet publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy everything from Backend publish
COPY --from=backend-build /src/Backend/bin/Release/net10.0/publish .

# Now overlay Frontend files from the frontend-build stage
# Copy entire dist folder - Frontend publishes to bin/Release/net10.0/publish
COPY --from=frontend-build /src/Frontend/bin/Release/net10.0/publish/wwwroot ./wwwroot

# Verify files
RUN echo "=== Final wwwroot structure ===" && ls -la wwwroot/ && echo "=== _framework files ===" && ls -la wwwroot/_framework/ 2>&1 | grep -i "icudt\|total" || echo "NO FRAMEWORK FILES FOUND!"

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
