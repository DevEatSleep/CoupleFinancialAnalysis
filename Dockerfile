FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy the entire solution
COPY . .

# Build the Frontend Blazor WebAssembly
RUN cd Frontend && dotnet publish -c Release -o /frontend-dist

# Create wwwroot in Backend and copy Frontend dist
RUN mkdir -p Backend/wwwroot && cp -r /frontend-dist/wwwroot/* Backend/wwwroot/

# Publish Backend with Frontend files included
RUN cd Backend && dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "CoupleChat.dll"]
