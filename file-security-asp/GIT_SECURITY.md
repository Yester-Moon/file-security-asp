# 🔒 Git 安全最佳实践

## ⚠️ 重要：如果您已经提交了敏感文件

如果您已经将包含敏感信息的文件（如 `appsettings.json`）提交到 Git，请按以下步骤处理：

### 1. 从 Git 历史中移除敏感文件

```bash
# 方法 1: 使用 git filter-repo (推荐)
# 首先安装 git-filter-repo
pip install git-filter-repo

# 移除文件
git filter-repo --path FileService/FileService.WebAPI/appsettings.json --invert-paths

# 方法 2: 使用 BFG Repo-Cleaner
# 下载 BFG: https://rtyley.github.io/bfg-repo-cleaner/
java -jar bfg.jar --delete-files appsettings.json

# 清理和重新打包
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# 强制推送（⚠️ 警告：这会改写历史）
git push origin --force --all
git push origin --force --tags
```

### 2. 立即更换所有密钥

一旦敏感信息泄露到 Git，必须：
- ✅ 更换所有密码
- ✅ 重新生成 JWT 密钥
- ✅ 重新生成加密密钥
- ✅ 更新数据库连接字符串
- ✅ 通知团队成员

### 3. 添加到 .gitignore

确保 `.gitignore` 文件包含：
```gitignore
**/appsettings.json
**/appsettings.*.json
!**/appsettings.*.json.example
```

### 4. 从暂存区移除（但保留本地文件）

```bash
# 从 Git 跟踪中移除，但保留本地文件
git rm --cached FileService/FileService.WebAPI/appsettings.json

# 提交更改
git commit -m "Remove sensitive appsettings.json from version control"

# 推送
git push
```

## ✅ 正确的工作流程

### 首次设置

1. **克隆仓库**
   ```bash
   git clone https://github.com/Yester-Moon/file-security-asp.git
   cd file-security-asp
   ```

2. **复制配置模板**
   ```bash
   # Windows PowerShell
   Copy-Item FileService\FileService.WebAPI\appsettings.Development.json.example `
             FileService\FileService.WebAPI\appsettings.json

   # Linux/Mac
   cp FileService/FileService.WebAPI/appsettings.Development.json.example \
      FileService/FileService.WebAPI/appsettings.json
   ```

3. **编辑配置**
   - 打开 `appsettings.json`
   - 填入您的数据库连接字符串
   - 生成并填入安全密钥（参见 CONFIGURATION.md）

4. **验证 .gitignore**
   ```bash
   # 检查哪些文件会被 Git 跟踪
   git status

   # appsettings.json 不应该出现在列表中
   ```

### 日常开发

1. **提交前检查**
   ```bash
   # 查看将要提交的文件
   git status
   git diff --cached

   # 确保没有敏感文件
   ```

2. **提交代码**
   ```bash
   git add .
   git commit -m "Your commit message"
   git push
   ```

3. **更新配置模板**
   如果添加了新的配置项：
   ```bash
   # 更新模板文件（移除敏感值）
   git add FileService/FileService.WebAPI/appsettings.Development.json.example
   git commit -m "Update configuration template"
   ```

## 🛡️ 安全检查清单

在每次提交前，检查：

- [ ] `appsettings.json` 不在提交列表中
- [ ] 没有硬编码的密码或密钥
- [ ] 没有数据库连接字符串（除了模板）
- [ ] 没有 API 密钥或令牌
- [ ] 没有个人证书文件 (.pfx, .p12, .key)
- [ ] 没有用户上传的文件
- [ ] 没有数据库文件 (.mdf, .ldf, .db)
- [ ] 没有日志文件
- [ ] 没有 bin/obj 目录

## 🔍 检测已提交的敏感信息

### 使用 git-secrets

```bash
# 安装 git-secrets
# Windows: 下载并安装 https://github.com/awslabs/git-secrets
# Mac: brew install git-secrets
# Linux: 根据发行版安装

# 初始化
git secrets --install
git secrets --register-aws

# 扫描历史
git secrets --scan-history
```

### 使用 TruffleHog

```bash
# 安装
pip install truffleHog

# 扫描仓库
trufflehog --regex --entropy=True file:///path/to/repo
```

### 使用 GitGuardian

在线服务，自动扫描 GitHub 仓库：
https://www.gitguardian.com/

## 📋 .gitignore 文件说明

我们的 `.gitignore` 包含以下主要部分：

### 1. .NET/Visual Studio 标准忽略项
- `bin/`, `obj/` - 构建输出
- `*.user`, `*.suo` - 用户设置
- `.vs/` - Visual Studio 缓存

### 2. 敏感配置文件
```gitignore
**/appsettings.json
**/appsettings.*.json
!**/appsettings.*.json.example
```

### 3. 项目特定文件
- `FileStorage/` - 用户上传的文件
- `logs/` - 应用日志
- `*.db` - SQLite 数据库

### 4. IDE 配置
- `.vscode/` - VS Code 设置
- `.idea/` - JetBrains Rider 设置

## 🚨 泄露后的应急响应

如果敏感信息已经泄露：

1. **立即行动**
   - [ ] 撤销受影响的凭证
   - [ ] 更改所有密码
   - [ ] 重新生成密钥
   - [ ] 审查访问日志

2. **通知相关方**
   - [ ] 通知团队成员
   - [ ] 通知安全团队
   - [ ] 如果涉及用户数据，可能需要通知用户

3. **清理 Git 历史**
   - [ ] 使用 BFG 或 git-filter-repo 清理
   - [ ] 强制推送清理后的历史

4. **加强措施**
   - [ ] 审查所有配置文件
   - [ ] 实施 pre-commit hooks
   - [ ] 使用密钥管理服务

## 🔐 Pre-commit Hook 示例

创建 `.git/hooks/pre-commit` 文件：

```bash
#!/bin/bash

# 检查是否要提交敏感文件
if git diff --cached --name-only | grep -q "appsettings\.json$"; then
    echo "错误: 不能提交 appsettings.json"
    echo "请使用 appsettings.*.json.example 模板文件"
    exit 1
fi

# 检查是否包含密码或密钥模式
if git diff --cached | grep -iE "(password|secret|key|token|apikey).*=.*['\"].*['\"]"; then
    echo "警告: 检测到可能的敏感信息"
    echo "请确认这不是真实的密钥或密码"
    read -p "确定要继续吗? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

exit 0
```

使文件可执行：
```bash
chmod +x .git/hooks/pre-commit
```

## 📚 相关资源

- [GitHub .gitignore 模板](https://github.com/github/gitignore)
- [OWASP 密钥管理指南](https://cheatsheetseries.owasp.org/cheatsheets/Key_Management_Cheat_Sheet.html)
- [Azure Key Vault 文档](https://docs.microsoft.com/azure/key-vault/)
- [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/)

## 💡 最佳实践总结

1. **永远不要**提交敏感信息到 Git
2. **使用**模板文件和环境变量
3. **实施** pre-commit hooks 防止意外提交
4. **定期**扫描仓库寻找泄露
5. **使用**专业的密钥管理服务
6. **教育**团队成员关于安全的重要性
7. **审查** Pull Requests 中的敏感信息

---

**记住：一旦提交到 Git，就很难完全删除！预防总是比补救容易。** 🔒
