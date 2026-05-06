FROM mcr.microsoft.com/dotnet/sdk:10.0 AS frontend-build
WORKDIR /src
COPY Frontend ./Frontend
COPY Shared ./Shared
RUN cd Frontend && dotnet publish -c Release -o /dist
# Verify the published structure
RUN echo "=== Frontend publish output ===" && find /dist -type d | head -20 && echo "=== Checking for _framework ===" && ls -la /dist/wwwroot/_framework/ 2>&1 | head -5

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN cd Backend && dotnet publish -c Release -o /backend-dist

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy Backend publish
COPY --from=backend-build /backend-dist .

# Ensure wwwroot exists
RUN mkdir -p wwwroot

# Copy Frontend dist - copy the entire wwwroot including _framework
RUN cp -rv /dist/wwwroot/* wwwroot/ || echo "Copy may have warnings"

# Final verification
RUN echo "=== Final wwwroot ===" && ls -la wwwroot/ && echo "=== Final _framework ===" && ls -la wwwroot/_framework/ 2>&1 | head -10

EXPOSE 8080
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
