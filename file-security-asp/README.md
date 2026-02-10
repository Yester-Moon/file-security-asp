# 文件安全管理系统 - 微服务架构

## 系统概述

基于 ASP.NET Core 8.0 的微服务架构文件安全管理系统，采用 DDD（领域驱动设计）和 CQRS 模式。

### 核心功能

1. **文件处理服务 (FileService)**
   - 文件上传、下载、预览、删除
   - 病毒扫描（ClamAV 集成）
   - AES-256 加密存储
   - 文件分享（链接、密码、有效期、访问次数限制）
   - 文件夹管理
   - 权限控制 (RBAC/ABAC)

2. **身份认证服务 (IdentityService)**
   - 用户注册、登录
   - JWT 令牌管理
   - 邮箱/手机验证
   - 角色管理

3. **审计日志服务 (AuditService)**
   - 操作日志记录
   - 访问追踪
   - 行为分析
   - 泄露溯源

4. **管理服务 (AdminService)**
   - 用户管理
   - 文件审计
   - 系统监控
   - 权限审查

## 技术栈

- **框架**: ASP.NET Core 8.0
- **数据库**: SQL Server (EF Core)
- **缓存**: 
  - 内存缓存 (IMemoryCache) - L1
  - Redis 分布式缓存 - L2
- **消息传递**: MediatR (集成事件)
- **认证**: JWT Bearer Token
- **加密**: AES-256-CBC
- **病毒扫描**: ClamAV
- **文档**: Swagger/OpenAPI

## 项目结构

```
FileService/
├── FileService.Domain/              # 领域层
│   ├── Entities/                    # 聚合根和实体
│   ├── ValueObjects/                # 值对象
│   ├── Enums/                       # 枚举
│   ├── Events/                      # 领域事件
│   ├── Repositories/                # 仓储接口
│   └── Services/                    # 领域服务接口
├── FileService.Application/         # 应用层
│   ├── Commands/                    # 命令 (CQRS)
│   ├── Queries/                     # 查询 (CQRS)
│   └── DTOs/                        # 数据传输对象
├── FileService.Infrastructure/      # 基础设施层
│   ├── Data/                        # EF Core DbContext
│   ├── Repositories/                # 仓储实现
│   ├── Services/                    # 服务实现
│   ├── Caching/                     # 缓存服务
│   └── IntegrationEvents/           # 集成事件
└── FileService.WebAPI/              # API 层
    └── Controllers/                 # REST API 控制器

IdentityService/
├── IdentityService.Domain/
└── IdentityService.Application/

AuditService/
├── AuditService.Domain/
└── AuditService.Application/

AdminService/
└── AdminService.Application/
```

## DDD 设计模式

### 聚合根 (Aggregate Roots)
- **FileEntity**: 文件聚合根，管理文件生命周期
- **Folder**: 文件夹聚合根
- **FileShare**: 分享链接聚合根

### 值对象 (Value Objects)
- **FileMetadata**: 文件元数据
- **ShareSettings**: 分享设置
- **EncryptionInfo**: 加密信息

### 领域事件 (Domain Events)
- **FileUploadedEvent**: 文件上传完成
- **FileSharedEvent**: 文件分享创建
- **FileAccessedEvent**: 文件访问记录

### 仓储模式 (Repository Pattern)
- 抽象数据访问逻辑
- 支持单元测试
- 易于切换数据源

## API 接口

### 文件管理 API

#### 1. 上传文件
```http
POST /api/files/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data

FormData:
  - file: [binary]
  - folderId: [guid] (optional)
```

#### 2. 获取文件列表
```http
GET /api/files?folderId={guid}
Authorization: Bearer {token}
```

#### 3. 下载文件
```http
GET /api/files/{fileId}/download
Authorization: Bearer {token}
```

#### 4. 创建分享链接
```http
POST /api/files/{fileId}/share
Authorization: Bearer {token}
Content-Type: application/json

{
  "expirationDate": "2024-12-31T23:59:59Z",
  "maxAccessCount": 10,
  "password": "secret123",
  "requireAuthentication": false
}
```

#### 5. 获取访问历史
```http
GET /api/files/{fileId}/access-history
Authorization: Bearer {token}
```

#### 6. 访问分享文件
```http
GET /api/files/share/{token}?password=secret123
```

#### 7. 删除文件
```http
DELETE /api/files/{fileId}
Authorization: Bearer {token}
```

### 文件夹管理 API

#### 1. 创建文件夹
```http
POST /api/folders
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "My Folder",
  "parentFolderId": null
}
```

#### 2. 获取文件夹列表
```http
GET /api/folders?parentId={guid}
Authorization: Bearer {token}
```

#### 3. 删除文件夹
```http
DELETE /api/folders/{folderId}
Authorization: Bearer {token}
```

## 安全特性

### 1. 文件加密
- 算法: AES-256-CBC
- 密钥管理: 建议使用 Azure Key Vault 或 AWS KMS
- 存储: 加密后的文件与原文件隔离

### 2. 病毒扫描
- 集成 ClamAV 病毒扫描引擎
- 上传时自动扫描
- 检测到威胁自动隔离

### 3. 访问控制
- JWT 认证
- 基于角色的访问控制 (RBAC)
- 基于属性的访问控制 (ABAC)
- 文件级权限: Read, Write, Delete, Share, Download

### 4. 分享安全
- 唯一令牌生成
- 密码保护
- 有效期限制
- 访问次数限制
- 访问记录追踪

### 5. 审计日志
- 完整操作链记录
- IP 地址和地理位置追踪
- 用户行为分析
- 泄露溯源支持

## 配置说明

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FileSecurityDb;Trusted_Connection=True;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "YourSecureKeyHereMustBeAtLeast32CharactersLong!",
    "Issuer": "FileSecurityService",
    "Audience": "FileSecurityClients",
    "ExpiryMinutes": 60
  },
  "Encryption": {
    "Key": "Base64EncodedAES256Key",
    "IV": "Base64EncodedIV"
  },
  "FileStorage": {
    "Path": "D:\\FileStorage"
  },
  "ClamAV": {
    "Endpoint": "http://localhost:3310/scan"
  }
}
```

### 生成加密密钥

```bash
# PowerShell
$key = [System.Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
$iv = [System.Convert]::ToBase64String((1..16 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
Write-Host "Key: $key"
Write-Host "IV: $iv"
```

## 部署步骤

### 1. 安装依赖

```bash
# 安装 SQL Server
# 安装 Redis
# 安装 ClamAV (可选)
```

### 2. 配置数据库

```bash
cd FileService/FileService.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. 运行服务

```bash
cd FileService/FileService.WebAPI
dotnet run
```

### 4. 访问 Swagger UI

```
https://localhost:5001/swagger
```

## 缓存策略

### 两级缓存架构
1. **L1 缓存 (内存缓存)**
   - 缓存时间: 5 分钟
   - 适用于频繁访问的数据
   - 服务重启后清空

2. **L2 缓存 (Redis)**
   - 缓存时间: 1 小时
   - 分布式共享
   - 持久化存储

### 缓存键命名规范
```
user_files_{userId}_{folderId}
file_share_{token}
file_metadata_{fileId}
```

## 集成事件

### 事件流转

```
FileService -> FileUploadedEvent -> AuditService (记录日志)
FileService -> FileSharedEvent -> AuditService (记录分享)
FileService -> FileAccessedEvent -> AuditService (记录访问)
```

### 事件处理器

使用 MediatR 实现发布-订阅模式:

```csharp
public class FileUploadedEventHandler : INotificationHandler<FileUploadedIntegrationEvent>
{
    public async Task Handle(FileUploadedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // 记录审计日志
        // 发送通知
        // 更新统计信息
    }
}
```

## 性能优化

1. **异步处理**: 文件扫描和加密在后台异步执行
2. **流式传输**: 大文件使用流式上传/下载
3. **缓存策略**: 两级缓存减少数据库查询
4. **索引优化**: 数据库关键字段建立索引
5. **连接池**: EF Core 连接池配置

## 监控和日志

### 日志级别
- **Information**: 正常操作
- **Warning**: 潜在问题（如病毒检测）
- **Error**: 操作失败
- **Critical**: 系统故障

### 监控指标
- API 响应时间
- 文件上传/下载速度
- 病毒扫描耗时
- 缓存命中率
- 数据库查询性能

## 扩展建议

1. **存储扩展**
   - 集成 Azure Blob Storage / AWS S3
   - 实现对象存储接口

2. **消息队列**
   - RabbitMQ / Azure Service Bus
   - 异步任务处理

3. **API 网关**
   - Ocelot / YARP
   - 统一入口和路由

4. **服务发现**
   - Consul / Eureka
   - 动态服务注册

5. **容器化**
   - Docker
   - Kubernetes 编排

## 安全最佳实践

1. ? 使用 HTTPS
2. ? JWT 令牌过期时间设置
3. ? 密码强度验证
4. ? SQL 注入防护（EF Core 参数化）
5. ? XSS 防护
6. ? CSRF 保护
7. ? 文件类型验证
8. ? 文件大小限制
9. ? 速率限制
10. ? 审计日志

## 测试

### 单元测试
```bash
dotnet test FileService.Tests
```

### 集成测试
```bash
dotnet test FileService.IntegrationTests
```

## 常见问题

### Q: 如何更改文件存储路径？
A: 修改 appsettings.json 中的 `FileStorage:Path` 配置。

### Q: 如何禁用病毒扫描？
A: 可以创建一个 `NoOpVirusScanService` 实现并在 DI 中注册。

### Q: 支持哪些文件类型？
A: 默认支持所有类型，可以通过配置限制特定类型。

### Q: 最大文件大小限制？
A: 在 Program.cs 中配置 `MaxRequestBodySize`。

## 贡献指南

欢迎贡献代码和提出建议！

## 许可证

MIT License

---

**作者**: File Security Team  
**版本**: 1.0.0  
**更新日期**: 2024
