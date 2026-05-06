FROM mcr.microsoft.com/dotnet/sdk:10.0 AS frontend-build
WORKDIR /src
COPY Frontend ./Frontend
COPY Shared ./Shared
RUN cd Frontend && dotnet publish -c Release -o /frontend-publish

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY . .

# Copy Frontend published wwwroot from frontend-build stage
COPY --from=frontend-build /frontend-publish/wwwroot Backend/wwwroot

RUN dotnet restore
RUN cd Backend && dotnet publish -c Release -o /backend-publish

# Verify files exist in publish output
RUN echo "=== Backend publish wwwroot ===" && ls -la /backend-publish/wwwroot/ 2>&1 | head -10 && \
    echo "=== _framework check ===" && ls /backend-publish/wwwroot/_framework/ 2>&1 | head -10

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend-build /backend-publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
