FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件并还原
COPY CampusVisitorApi.csproj .
RUN dotnet restore

# 复制所有源代码并发布
COPY . .
RUN dotnet publish -c Release -o /app

# 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Server=sqlserver,1433;Database=CampusVisitorDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True"

ENTRYPOINT ["dotnet", "CampusVisitorApi.dll"]
