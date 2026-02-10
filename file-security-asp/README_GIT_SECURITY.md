# 🛡️ 文件安全管理系统 - Git 安全配置

## ⚠️ 重要：首次使用必读

本项目包含敏感配置文件（数据库密码、加密密钥等）。为了保护这些信息，我们已经配置了完善的 Git 安全措施。

## 🚀 快速开始

### 1. 克隆仓库
```bash
git clone https://github.com/Yester-Moon/file-security-asp.git
cd file-security-asp
```

### 2. 运行安全配置脚本

#### Linux/Mac
```bash
chmod +x setup-git-security.sh
./setup-git-security.sh
```

#### Windows (PowerShell)
```powershell
.\setup-git-security.bat
```

### 3. 配置应用程序

脚本会自动：
- ✅ 安装 pre-commit hook（防止提交敏感文件）
- ✅ 检查 Git 历史中的敏感文件
- ✅ 移除已跟踪的敏感文件
- ✅ 从模板创建配置文件
- ✅ 生成安全密钥
- ✅ 测试配置

## 📁 项目结构

```
file-security-asp/
├── .gitignore                          # Git 忽略配置
├── FileService/                        # 文件服务
│   └── FileService.WebAPI/
│       ├── appsettings.json           # ❌ 本地配置（不提交）
│       └── appsettings.*.json.example # ✅ 配置模板（提交）
├── IdentityService/                    # 身份认证服务
├── AuditService/                       # 审计服务
├── AdminService/                       # 管理服务
├── GIT_SECURITY.md                     # Git 安全指南
├── CONFIGURATION.md                    # 配置详细说明
├── HOOKS_SETUP.md                      # Hook 安装指南
├── setup-git-security.sh               # Linux/Mac 安装脚本
├── setup-git-security.bat              # Windows 安装脚本
└── README.md                           # 本文件
```

## 🔐 安全功能

### 1. .gitignore 配置
自动忽略：
- 敏感配置文件（`appsettings.json`）
- 用户上传的文件
- 数据库文件
- 日志文件
- 证书文件
- IDE 配置文件

### 2. Pre-commit Hook
自动检测并阻止：
- ❌ 提交 `appsettings.json`
- ❌ 提交 `.env` 文件
- ❌ 提交证书文件
- ❌ 硬编码的密码或密钥
- ⚠️ 大文件（>10MB）

### 3. 配置模板
- ✅ 提供配置文件结构
- ✅ 不包含真实敏感信息
- ✅ 团队成员可以参考

## 📚 详细文档

| 文档 | 描述 |
|------|------|
| [GIT_SECURITY.md](GIT_SECURITY.md) | Git 安全最佳实践和应急响应 |
| [CONFIGURATION.md](CONFIGURATION.md) | 详细的配置说明和密钥生成 |
| [HOOKS_SETUP.md](HOOKS_SETUP.md) | Pre-commit Hook 安装指南 |
| [PROJECT_STATUS.md](PROJECT_STATUS.md) | 项目实现状态和待办事项 |
| [GIT_SETUP_COMPLETE.md](GIT_SETUP_COMPLETE.md) | 完整的安全配置总结 |

## ⚡ 手动配置（如果不使用脚本）

### 1. 安装 Pre-commit Hook

```bash
# Linux/Mac
cp pre-commit.sh .git/hooks/pre-commit
chmod +x .git/hooks/pre-commit

# Windows
copy pre-commit.bat .git\hooks\pre-commit
```

### 2. 创建配置文件

```bash
# 复制模板
cp FileService/FileService.WebAPI/appsettings.Development.json.example \
   FileService/FileService.WebAPI/appsettings.json
```

### 3. 生成密钥

使用 PowerShell：
```powershell
# JWT Key
$jwtKeyBytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($jwtKeyBytes)
[Convert]::ToBase64String($jwtKeyBytes)

# AES Key
$aesKeyBytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($aesKeyBytes)
[Convert]::ToBase64String($aesKeyBytes)

# AES IV
$aesIvBytes = New-Object byte[] 16
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($aesIvBytes)
[Convert]::ToBase64String($aesIvBytes)
```

### 4. 更新配置

编辑 `appsettings.json`，填入：
- 数据库连接字符串
- 生成的 JWT 密钥
- 生成的 AES 密钥和 IV
- 文件存储路径

## 🚨 重要提醒

### ❌ 永远不要

- ❌ 将 `appsettings.json` 提交到 Git
- ❌ 在代码中硬编码密钥或密码
- ❌ 通过聊天工具分享密钥
- ❌ 在公开的 Issue 或 PR 中暴露敏感信息
- ❌ 使用 `--no-verify` 绕过 pre-commit hook（除非紧急情况）

### ✅ 务必做到

- ✅ 使用配置模板文件
- ✅ 为每个环境生成独立的密钥
- ✅ 定期轮换密钥
- ✅ 安全地存储密钥（使用密码管理器）
- ✅ 在生产环境使用 Azure Key Vault 或类似服务
- ✅ 提交前检查 `git status`

## 🔍 验证配置

```bash
# 1. 检查 appsettings.json 是否被忽略
git status
# 应该看不到 appsettings.json

# 2. 测试 pre-commit hook
echo "test" > test-sensitive.json
git add test-sensitive.json
git commit -m "Test"
# 如果文件名包含敏感关键字，应该被阻止

# 3. 清理测试
git reset HEAD test-sensitive.json
rm test-sensitive.json
```

## 🆘 如果已经提交了敏感文件

**立即行动：**

1. **不要 panic，但要快速行动**
2. **参考 [GIT_SECURITY.md](GIT_SECURITY.md) 的详细步骤**
3. **使用 `git-filter-repo` 清理历史**
4. **立即更换所有密钥**
5. **通知团队成员**

简化步骤：
```bash
# 安装 git-filter-repo
pip install git-filter-repo

# 清理文件
git filter-repo --path appsettings.json --invert-paths

# 强制推送（⚠️ 会改写历史）
git push --force --all
```

## 🎯 团队协作

### 对于新成员

1. 克隆仓库
2. 运行 `setup-git-security.sh` 或 `.bat`
3. 阅读 `CONFIGURATION.md` 配置应用
4. 向团队获取数据库连接信息（不要通过 Git）
5. 阅读 `GIT_SECURITY.md` 了解安全实践

### 对于维护者

1. 确保 `.gitignore` 已提交
2. 确保配置模板已提交
3. 不要提交 `appsettings.json`
4. 审查 Pull Request 时检查敏感信息
5. 定期提醒团队成员注意安全

## 📞 获取帮助

- 📖 **配置问题**：查看 [CONFIGURATION.md](CONFIGURATION.md)
- 🔐 **安全问题**：查看 [GIT_SECURITY.md](GIT_SECURITY.md)
- 🛠️ **Hook 问题**：查看 [HOOKS_SETUP.md](HOOKS_SETUP.md)
- 💬 **其他问题**：创建 Issue 或联系团队

## 🔗 相关资源

- [GitHub .gitignore 最佳实践](https://github.com/github/gitignore)
- [OWASP 安全编码实践](https://owasp.org/www-project-secure-coding-practices-quick-reference-guide/)
- [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/)
- [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/)

## 📝 许可证

[您的许可证]

## 🙏 贡献

欢迎贡献！但请注意：
- 不要提交敏感信息
- 遵循安全最佳实践
- 提交 PR 前运行安全检查

---

**安全第一！保护用户数据是我们的责任。** 🛡️
