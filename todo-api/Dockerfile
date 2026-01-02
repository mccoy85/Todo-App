FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY todo-api/src/TodoApi.Api/TodoApi.Api.csproj todo-api/src/TodoApi.Api/
COPY todo-api/src/TodoApi.Core/TodoApi.Core.csproj todo-api/src/TodoApi.Core/
COPY todo-api/src/TodoApi.Infrastructure/TodoApi.Infrastructure.csproj todo-api/src/TodoApi.Infrastructure/
RUN dotnet restore todo-api/src/TodoApi.Api/TodoApi.Api.csproj

COPY todo-api/ todo-api/
WORKDIR /src/todo-api/src/TodoApi.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5121
EXPOSE 5121
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TodoApi.Api.dll"]
