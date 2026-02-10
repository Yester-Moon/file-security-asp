# 项目依赖和配置指南

## NuGet 包依赖

### FileService.Domain
```xml
<PackageReference Include="MediatR.Contracts" Version="2.0.1" />
```

### FileService.Application
```xml
<PackageReference Include="MediatR" Version="12.2.0" />
```
**项目引用:**
- FileService.Domain

### FileService.Infrastructure
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```
**项目引用:**
- FileService.Domain

### FileService.WebAPI
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
```
**项目引用:**
- FileService.Domain
- FileService.Application
- FileService.Infrastructure

## 数据库迁移

### 创建初始迁移

```bash
# 在 Infrastructure 项目目录下执行
cd FileService.Infrastructure

# 添加迁移
dotnet ef migrations add InitialCreate --startup-project ../FileService.WebAPI --context FileServiceDbContext

# 更新数据库
dotnet ef database update --startup-project ../FileService.WebAPI --context FileServiceDbContext
```

### 添加新迁移

```bash
dotnet ef migrations add <MigrationName> --startup-project ../FileService.WebAPI
dotnet ef database update --startup-project ../FileService.WebAPI
```

### 回滚迁移

```bash
dotnet ef database update <PreviousMigrationName> --startup-project ../FileService.WebAPI
```

### 查看迁移历史

```bash
dotnet ef migrations list --startup-project ../FileService.WebAPI
```

## 环境配置

### 开发环境 (appsettings.Development.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FileSecurityDb_Dev;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

### 生产环境 (appsettings.Production.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=FileSecurityDb;User Id=sa;Password=***;TrustServerCertificate=True;",
    "Redis": "prod-redis:6379,password=***"
  },
  "Jwt": {
    "Key": "*** Production Key ***"
  },
  "Encryption": {
    "Key": "*** Production Key ***",
    "IV": "*** Production IV ***"
  }
}
```

## Docker 配置

### Dockerfile (FileService.WebAPI)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FileService.WebAPI/FileService.WebAPI.csproj", "FileService.WebAPI/"]
COPY ["FileService.Application/FileService.Application.csproj", "FileService.Application/"]
COPY ["FileService.Infrastructure/FileService.Infrastructure.csproj", "FileService.Infrastructure/"]
COPY ["FileService.Domain/FileService.Domain.csproj", "FileService.Domain/"]
RUN dotnet restore "FileService.WebAPI/FileService.WebAPI.csproj"
COPY . .
WORKDIR "/src/FileService.WebAPI"
RUN dotnet build "FileService.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileService.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.WebAPI.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

  redis:
    image: redis:7-alpine
    container_name: redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data

  fileservice:
    build:
      context: ./FileService
      dockerfile: FileService.WebAPI/Dockerfile
    container_name: fileservice
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=FileSecurityDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
      - ConnectionStrings__Redis=redis:6379
    ports:
      - "5000:80"
      - "5001:443"
    depends_on:
      - sqlserver
      - redis
    volumes:
      - file-storage:/app/FileStorage

volumes:
  sqlserver-data:
  redis-data:
  file-storage:
```

## Redis 配置

### 安装 Redis (Windows)

```bash
# 使用 Chocolatey
choco install redis-64

# 或下载 Redis for Windows
# https://github.com/microsoftarchive/redis/releases
```

### 启动 Redis

```bash
redis-server
```

### 测试 Redis 连接

```bash
redis-cli ping
# 应该返回: PONG
```

## SQL Server 配置

### 创建数据库

```sql
CREATE DATABASE FileSecurityDb;
GO

USE FileSecurityDb;
GO

-- EF Core 会自动创建表，但你可以手动验证
SELECT * FROM INFORMATION_SCHEMA.TABLES;
```

### 数据库备份

```bash
# 使用 SQL Server Management Studio (SSMS)
# 或使用命令行
sqlcmd -S localhost -Q "BACKUP DATABASE FileSecurityDb TO DISK='C:\Backups\FileSecurityDb.bak'"
```

## ClamAV 病毒扫描配置 (可选)

### 安装 ClamAV (Windows)

1. 下载 ClamAV: https://www.clamav.net/downloads
2. 安装并配置
3. 启动服务

### Docker 运行 ClamAV

```yaml
# 添加到 docker-compose.yml
clamav:
  image: clamav/clamav:latest
  container_name: clamav
  ports:
    - "3310:3310"
```

### 测试 ClamAV

```bash
# 下载 EICAR 测试文件
curl https://www.eicar.org/download/eicar.com.txt -o eicar.txt

# 扫描文件
clamscan eicar.txt
```

## 安全密钥生成

### 生成 JWT 密钥

```csharp
// C# 代码
var key = new byte[32];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(key);
}
var base64Key = Convert.ToBase64String(key);
Console.WriteLine(base64Key);
```

### 生成 AES 加密密钥

```powershell
# PowerShell
function Generate-EncryptionKeys {
    $key = New-Object byte[] 32
    $iv = New-Object byte[] 16
    
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($key)
    $rng.GetBytes($iv)
    
    $keyBase64 = [Convert]::ToBase64String($key)
    $ivBase64 = [Convert]::ToBase64String($iv)
    
    Write-Host "Key: $keyBase64"
    Write-Host "IV: $ivBase64"
}

Generate-EncryptionKeys
```

## 性能监控

### Application Insights (可选)

```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.21.0" />
```

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:InstrumentationKey"]);
```

### Prometheus + Grafana (可选)

```xml
<PackageReference Include="prometheus-net.AspNetCore" Version="8.0.0" />
```

```csharp
// Program.cs
app.UseMetricServer();
app.UseHttpMetrics();
```

## 日志配置

### Serilog (推荐)

```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
```

```csharp
// Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
```

## 测试配置

### 单元测试项目

```bash
dotnet new xunit -n FileService.Tests
cd FileService.Tests
dotnet add reference ../FileService.Domain/FileService.Domain.csproj
dotnet add reference ../FileService.Application/FileService.Application.csproj
dotnet add package Moq
dotnet add package FluentAssertions
```

### 示例测试

```csharp
public class FileEntityTests
{
    [Fact]
    public void CreateFile_ShouldSetStatusToUploading()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var metadata = new FileMetadata("test.txt", "text/plain", 1024, "hash");

        // Act
        var file = new FileEntity(ownerId, metadata);

        // Assert
        file.Status.Should().Be(FileStatus.Uploading);
        file.OwnerId.Should().Be(ownerId);
    }
}
```

## API 测试

### 使用 REST Client (VS Code 扩展)

创建 `api-tests.http` 文件:

```http
### 上传文件
POST https://localhost:5001/api/files/upload
Authorization: Bearer {{token}}
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="file"; filename="test.txt"
Content-Type: text/plain

Test file content
------WebKitFormBoundary7MA4YWxkTrZu0gW--

### 获取文件列表
GET https://localhost:5001/api/files
Authorization: Bearer {{token}}

### 创建分享链接
POST https://localhost:5001/api/files/{{fileId}}/share
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "expirationDate": "2024-12-31T23:59:59Z",
  "maxAccessCount": 10,
  "password": "secret123"
}
```

## 故障排查

### 常见问题

1. **数据库连接失败**
   - 检查 SQL Server 是否运行
   - 验证连接字符串
   - 检查防火墙设置

2. **Redis 连接失败**
   - 确认 Redis 服务运行
   - 检查端口 6379 是否开放
   - 验证连接字符串

3. **JWT 验证失败**
   - 确认密钥配置正确
   - 检查令牌是否过期
   - 验证 Issuer 和 Audience

4. **文件上传失败**
   - 检查文件大小限制
   - 验证存储路径权限
   - 查看日志文件

### 日志位置

```
./logs/file-service-{Date}.log
```

### 启用详细日志

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "FileService": "Trace"
    }
  }
}
```

## 部署检查清单

- [ ] 更新生产环境配置文件
- [ ] 生成强密钥和 IV
- [ ] 配置 SSL 证书
- [ ] 设置数据库备份
- [ ] 配置 Redis 持久化
- [ ] 启用日志监控
- [ ] 配置防火墙规则
- [ ] 测试病毒扫描集成
- [ ] 验证文件加密
- [ ] 设置速率限制
- [ ] 配置 CORS 策略
- [ ] 运行性能测试
- [ ] 准备回滚计划

## 联系支持

如有问题，请联系技术支持团队。
