# 快速启动指南

## 1. 恢复 NuGet 包

```bash
cd FileService
dotnet restore
```

## 2. 配置数据库连接

编辑 `FileService.WebAPI/appsettings.json`，更新数据库连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FileSecurityDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

## 3. 生成并配置加密密钥

在 PowerShell 中运行：

```powershell
# 生成 AES 密钥和 IV
$key = New-Object byte[] 32
$iv = New-Object byte[] 16
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($key)
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($iv)
$keyBase64 = [Convert]::ToBase64String($key)
$ivBase64 = [Convert]::ToBase64String($iv)
Write-Host "Key: $keyBase64"
Write-Host "IV: $ivBase64"
```

将生成的密钥添加到 `appsettings.json`：

```json
{
  "Encryption": {
    "Key": "你生成的Key",
    "IV": "你生成的IV"
  }
}
```

## 4. 创建数据库迁移

```bash
cd FileService.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../FileService.WebAPI --context FileServiceDbContext
dotnet ef database update --startup-project ../FileService.WebAPI --context FileServiceDbContext
```

## 5. 运行服务

```bash
cd ../FileService.WebAPI
dotnet run
```

## 6. 访问 API 文档

打开浏览器访问：`https://localhost:5001/swagger`

## 常见问题解决

### 问题 1: Redis 连接失败

**解决方案：** 注释掉 Redis 配置或安装 Redis

```bash
# Windows (使用 Chocolatey)
choco install redis-64

# 或使用 Docker
docker run -d -p 6379:6379 redis:alpine
```

如果暂时不使用 Redis，可以在 `Program.cs` 中注释掉 Redis 相关配置。

### 问题 2: SQL Server 连接失败

**解决方案：** 确保 SQL Server 正在运行，并且连接字符串正确。

可以使用 LocalDB：
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FileSecurityDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 问题 3: 编译错误

**解决方案：** 清理并重新编译

```bash
dotnet clean
dotnet build
```

## API 测试示例

### 1. 上传文件 (需要 JWT Token)

```bash
curl -X POST "https://localhost:5001/api/files/upload" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@test.txt"
```

### 2. 获取文件列表

```bash
curl -X GET "https://localhost:5001/api/files" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 3. 创建分享链接

```bash
curl -X POST "https://localhost:5001/api/files/{fileId}/share" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "expirationDate": "2024-12-31T23:59:59Z",
    "maxAccessCount": 10,
    "password": "secret123"
  }'
```

### 4. 访问分享文件（无需认证）

```bash
curl -X GET "https://localhost:5001/api/files/share/{token}?password=secret123"
```

## 下一步

1. **实现身份认证服务**：创建用户注册和登录接口
2. **实现审计服务**：记录所有操作日志
3. **实现管理服务**：提供管理员功能
4. **添加前端界面**：使用 React/Vue/Angular 创建用户界面

## 项目结构说明

```
FileService/
├── FileService.Domain/          ? 已完成 - 领域模型
├── FileService.Application/     ? 已完成 - 应用逻辑
├── FileService.Infrastructure/  ? 已完成 - 基础设施
└── FileService.WebAPI/          ? 已完成 - API 接口

待实现：
├── IdentityService/            ? 待实现 - 身份认证
├── AuditService/               ? 待实现 - 审计日志
└── AdminService/               ? 待实现 - 管理功能
```

## 核心功能状态

- ? 文件上传（带病毒扫描和加密）
- ? 文件下载（带解密）
- ? 文件删除
- ? 文件分享（带密码、有效期、访问次数限制）
- ? 访问历史追踪
- ? 文件夹管理
- ? 权限控制
- ? 缓存支持（内存 + Redis）
- ? 集成事件
- ? JWT 认证（需要实现 IdentityService）
- ? 审计日志（需要实现 AuditService）

## 技术支持

如遇到问题，请查看日志文件或提交 Issue。
