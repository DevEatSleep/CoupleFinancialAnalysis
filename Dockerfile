FROM mcr.microsoft.com/dotnet/sdk:10.0 AS frontend-build
WORKDIR /src
COPY Frontend ./Frontend
COPY Shared ./Shared
# Publish to explicit output directory
RUN cd Frontend && dotnet publish -c Release -o /frontend-publish

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY . .
RUN dotnet restore

# Create wwwroot and copy Frontend published files
RUN mkdir -p Backend/wwwroot && \
    ls -la /frontend-publish/ && \
    cp -r /frontend-publish/wwwroot/* Backend/wwwroot/ 2>&1 || echo "Failed to copy Frontend files"

RUN cd Backend && dotnet publish -c Release -o /backend-publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy entire Backend publish output
COPY --from=backend-build /backend-publish .

# Verify files exist in the runtime container
RUN echo "=== Runtime container wwwroot ===" && ls -la wwwroot/ 2>&1 | head -5 || echo "No wwwroot"

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
