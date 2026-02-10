# Git Hooks 安装脚本

## 安装 Pre-commit Hook

### Linux/Mac

```bash
# 复制并设置权限
cp pre-commit.sh .git/hooks/pre-commit
chmod +x .git/hooks/pre-commit

# 测试
.git/hooks/pre-commit
```

### Windows (PowerShell)

```powershell
# 复制文件
Copy-Item pre-commit.bat .git\hooks\pre-commit

# 或者使用 bash 版本（如果安装了 Git Bash）
Copy-Item pre-commit.sh .git\hooks\pre-commit
```

### Windows (CMD)

```cmd
copy pre-commit.bat .git\hooks\pre-commit
```

## 手动安装步骤

1. **找到 .git/hooks 目录**
   ```bash
   cd .git/hooks
   ```

2. **创建 pre-commit 文件**
   - Linux/Mac: 创建 `pre-commit` 文件（无扩展名）
   - Windows: 可以使用 `.bat` 或 `.sh` 扩展名

3. **复制脚本内容**
   - 从 `pre-commit.sh` 或 `pre-commit.bat` 复制内容

4. **设置可执行权限（Linux/Mac）**
   ```bash
   chmod +x pre-commit
   ```

## 测试 Hook

```bash
# 尝试提交一个敏感文件
git add FileService/FileService.WebAPI/appsettings.json
git commit -m "Test commit"

# 应该会被拒绝
```

## 绕过 Hook（紧急情况）

⚠️ **仅在紧急情况下使用！**

```bash
git commit --no-verify -m "Your message"
```

## 全局 Git Hooks（可选）

设置全局 hooks 模板目录：

```bash
# 创建目录
mkdir -p ~/.git-templates/hooks

# 复制 hook
cp pre-commit.sh ~/.git-templates/hooks/pre-commit
chmod +x ~/.git-templates/hooks/pre-commit

# 配置 Git 使用模板
git config --global init.templatedir ~/.git-templates

# 现在所有新仓库都会包含这个 hook
```

## 更新现有仓库

```bash
# 应用到现有仓库
git init
```

## 禁用 Hook

如果需要临时禁用：

```bash
# 重命名 hook 文件
mv .git/hooks/pre-commit .git/hooks/pre-commit.disabled

# 或者删除
rm .git/hooks/pre-commit
```

## 常见问题

### Q: Hook 没有运行？
A: 确保文件有可执行权限：`chmod +x .git/hooks/pre-commit`

### Q: Windows 上 bash 脚本不工作？
A: 使用 `.bat` 版本，或安装 Git Bash

### Q: 如何调试 Hook？
A: 在脚本中添加 `echo` 语句或 `set -x`（bash）

### Q: 团队如何共享 Hook？
A: 将 hook 脚本放在仓库中，每个人手动安装

## 高级：自动安装 Hook

在项目中添加安装脚本：

```bash
#!/bin/bash
# setup-hooks.sh

echo "Installing git hooks..."

if [ ! -d ".git" ]; then
    echo "Error: Not a git repository"
    exit 1
fi

cp pre-commit.sh .git/hooks/pre-commit
chmod +x .git/hooks/pre-commit

echo "Git hooks installed successfully!"
```

在 README 中提醒新开发者运行：
```bash
bash setup-hooks.sh
```

## 参考资源

- [Git Hooks 官方文档](https://git-scm.com/book/en/v2/Customizing-Git-Git-Hooks)
- [Husky](https://github.com/typicode/husky) - Node.js 项目的 Git hooks 管理
- [pre-commit framework](https://pre-commit.com/) - 多语言 Git hooks 框架
