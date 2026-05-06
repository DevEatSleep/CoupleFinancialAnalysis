FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy the entire solution
COPY . .

# Build the Frontend Blazor WebAssembly
RUN cd Frontend && dotnet publish -c Release -o /frontend-dist

# Verify Frontend files exist
RUN ls -la /frontend-dist/

# Create wwwroot in Backend and copy ALL Frontend dist files
RUN mkdir -p Backend/wwwroot && \
    cp -r /frontend-dist/wwwroot/* Backend/wwwroot/ && \
    ls -la Backend/wwwroot/ && \
    ls -la Backend/wwwroot/_framework/ 2>/dev/null || echo "Framework not found yet"

# Publish Backend with Frontend files included
RUN cd Backend && dotnet publish -c Release -o /app/publish && \
    ls -la /app/publish/wwwroot/ && \
    ls -la /app/publish/wwwroot/_framework/ 2>/dev/null || echo "Framework missing in publish"

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Verify files in runtime image
RUN ls -la /app/wwwroot/ && \
    ls -la /app/wwwroot/_framework/ 2>/dev/null || echo "Framework not found in runtime"

# Expose port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
