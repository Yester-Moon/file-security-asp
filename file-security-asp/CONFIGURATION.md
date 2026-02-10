# 配置文件说明

## 重要提示
⚠️ **不要将包含敏感信息的 `appsettings.json` 提交到 Git 仓库！**

## 配置步骤

### 1. 复制模板文件
```bash
cp FileService/FileService.WebAPI/appsettings.Development.json.example FileService/FileService.WebAPI/appsettings.json
```

### 2. 生成加密密钥

#### 使用 PowerShell 生成：
```powershell
# 生成 JWT 密钥 (至少 32 字节)
$jwtKeyBytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($jwtKeyBytes)
$jwtKey = [Convert]::ToBase64String($jwtKeyBytes)
Write-Host "JWT Key: $jwtKey"

# 生成 AES 加密密钥 (32 字节 for AES-256)
$aesKeyBytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($aesKeyBytes)
$aesKey = [Convert]::ToBase64String($aesKeyBytes)
Write-Host "AES Key: $aesKey"

# 生成 AES IV (16 字节)
$aesIvBytes = New-Object byte[] 16
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($aesIvBytes)
$aesIv = [Convert]::ToBase64String($aesIvBytes)
Write-Host "AES IV: $aesIv"
```

#### 使用 C# 代码生成：
```csharp
using System;
using System.Security.Cryptography;

// JWT Key
var jwtKeyBytes = new byte[64];
RandomNumberGenerator.Fill(jwtKeyBytes);
Console.WriteLine($"JWT Key: {Convert.ToBase64String(jwtKeyBytes)}");

// AES Key
var aesKeyBytes = new byte[32];
RandomNumberGenerator.Fill(aesKeyBytes);
Console.WriteLine($"AES Key: {Convert.ToBase64String(aesKeyBytes)}");

// AES IV
var aesIvBytes = new byte[16];
RandomNumberGenerator.Fill(aesIvBytes);
Console.WriteLine($"AES IV: {Convert.ToBase64String(aesIvBytes)}");
```

### 3. 配置数据库连接

#### SQL Server (Windows 认证)
```json
"DefaultConnection": "Server=localhost;Database=FileSecurityDb_Dev;Trusted_Connection=True;TrustServerCertificate=True;"
```

#### SQL Server (用户名密码)
```json
"DefaultConnection": "Server=localhost;Database=FileSecurityDb_Dev;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

#### LocalDB
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FileSecurityDb_Dev;Trusted_Connection=True;"
```

### 4. 配置 Redis（可选）

如果没有 Redis，可以临时注释掉 Program.cs 中的 Redis 配置：
```csharp
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("Redis");
//     options.InstanceName = "FileService_";
// });
```

### 5. 配置文件存储路径

Windows:
```json
"FileStorage": {
  "Path": "D:\\FileStorage"
}
```

Linux/Mac:
```json
"FileStorage": {
  "Path": "/var/fileservice/storage"
}
```

## 配置文件结构

```
FileService/
└── FileService.WebAPI/
    ├── appsettings.json                          # ❌ 不要提交（包含敏感信息）
    ├── appsettings.Development.json              # ❌ 不要提交（开发环境配置）
    ├── appsettings.Production.json               # ❌ 不要提交（生产环境配置）
    ├── appsettings.Development.json.example      # ✅ 可以提交（模板文件）
    └── appsettings.Production.json.example       # ✅ 可以提交（模板文件）
```

## 环境变量配置（推荐用于生产环境）

也可以使用环境变量代替 appsettings.json：

### Windows (PowerShell)
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=..."
$env:Jwt__Key = "your-key-here"
$env:Encryption__Key = "your-key-here"
$env:Encryption__IV = "your-iv-here"
```

### Linux/Mac
```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=..."
export Jwt__Key="your-key-here"
export Encryption__Key="your-key-here"
export Encryption__IV="your-iv-here"
```

### Docker Compose
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=FileSecurityDb;...
  - Jwt__Key=${JWT_KEY}
  - Encryption__Key=${ENCRYPTION_KEY}
  - Encryption__IV=${ENCRYPTION_IV}
```

## Azure Key Vault（生产环境推荐）

在生产环境中，建议使用 Azure Key Vault 或 AWS Secrets Manager 存储敏感信息。

### 安装包
```bash
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
```

### Program.cs 配置
```csharp
if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["KeyVault:Endpoint"]!);
    builder.Configuration.AddAzureKeyVault(
        keyVaultEndpoint,
        new DefaultAzureCredential());
}
```

## 安全检查清单

- [ ] 不要在代码中硬编码密钥
- [ ] 不要将 appsettings.json 提交到 Git
- [ ] 使用强随机密钥（至少 32 字节）
- [ ] 定期轮换密钥
- [ ] 在生产环境使用密钥管理服务
- [ ] 使用 HTTPS
- [ ] 限制数据库用户权限
- [ ] 启用数据库审计
- [ ] 定期备份数据库

## 常见问题

### Q: 如何验证配置是否正确？
A: 运行项目后访问健康检查端点：`https://localhost:5001/health`

### Q: 密钥丢失怎么办？
A: 如果加密密钥丢失，已加密的文件将无法解密。请务必备份密钥！

### Q: 如何在团队中共享配置？
A: 使用配置管理工具（如 Azure Key Vault）或加密的密码管理器共享敏感信息，不要通过 Git 或聊天工具传输。

## 获取帮助

如有问题，请查看文档或联系团队。
