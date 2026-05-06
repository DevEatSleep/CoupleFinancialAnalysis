FROM mcr.microsoft.com/dotnet/sdk:10.0 AS frontend-build
WORKDIR /src
COPY Frontend ./Frontend
COPY Shared ./Shared
RUN cd Frontend && dotnet publish -c Release -o /frontend-out

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN cd Backend && dotnet publish -c Release -o /backend-out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy Backend published files
COPY --from=backend-build /backend-out .

# Copy Frontend WebAssembly files directly from frontend build
COPY --from=frontend-build /frontend-out/wwwroot ./wwwroot

# Verify files exist
RUN echo "=== wwwroot contents ===" && \
    ls -la wwwroot/ && \
    echo "=== _framework contents ===" && \
    ls -la wwwroot/_framework/ 2>&1 | head -10

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
