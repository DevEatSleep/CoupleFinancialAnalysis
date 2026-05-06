FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and projects
COPY . .

# Restore and build solution
RUN dotnet restore

# Publish Frontend WebAssembly app
RUN cd Frontend && \
    dotnet publish -c Release -o /frontend-dist

# Ensure Backend has wwwroot directory
RUN mkdir -p Backend/wwwroot

# Copy ENTIRE Frontend publish output (including _framework) to Backend wwwroot
# The Frontend publish creates dist/wwwroot with all necessary files
RUN cp -rv /frontend-dist/wwwroot/* Backend/wwwroot/

# Verify files exist
RUN echo "=== Backend wwwroot contents ===" && \
    find Backend/wwwroot -type f | head -20 && \
    echo "=== Checking for _framework ===" && \
    ls -la Backend/wwwroot/_framework/ 2>&1 | head -5

# Publish Backend with Frontend files embedded
RUN cd Backend && dotnet publish -c Release -o /app/publish

# Verify final output
RUN echo "=== Final publish wwwroot ===" && \
    ls -la /app/publish/wwwroot/ && \
    ls -la /app/publish/wwwroot/_framework/ 2>&1 | head -5

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
