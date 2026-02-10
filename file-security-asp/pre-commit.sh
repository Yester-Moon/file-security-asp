#!/bin/bash

# Pre-commit hook to prevent committing sensitive files
# Copy this file to .git/hooks/pre-commit and make it executable:
# chmod +x .git/hooks/pre-commit

echo "🔍 Running pre-commit security checks..."

# Colors for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# Check for sensitive files
SENSITIVE_FILES=(
    "appsettings.json"
    "appsettings.Development.json"
    "appsettings.Production.json"
    "secrets.json"
    ".env"
    "*.pfx"
    "*.p12"
    "*.key"
)

FOUND_SENSITIVE=0

for file_pattern in "${SENSITIVE_FILES[@]}"; do
    if git diff --cached --name-only | grep -q "$file_pattern"; then
        echo -e "${RED}❌ 错误: 检测到敏感文件: $file_pattern${NC}"
        FOUND_SENSITIVE=1
    fi
done

if [ $FOUND_SENSITIVE -eq 1 ]; then
    echo -e "${RED}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${RED}不能提交敏感文件！${NC}"
    echo -e "${YELLOW}请移除这些文件或使用模板文件（.example）${NC}"
    echo -e "${YELLOW}使用以下命令移除：${NC}"
    echo -e "  ${GREEN}git reset HEAD <file>${NC}"
    echo -e "${RED}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    exit 1
fi

# Check for hardcoded secrets in code
PATTERNS=(
    "password\s*=\s*['\"][^'\"]+['\"]"
    "secret\s*=\s*['\"][^'\"]+['\"]"
    "apikey\s*=\s*['\"][^'\"]+['\"]"
    "token\s*=\s*['\"][^'\"]+['\"]"
    "AKIA[0-9A-Z]{16}"  # AWS Access Key
    "AIza[0-9A-Za-z\\-_]{35}"  # Google API Key
)

FOUND_PATTERN=0

for pattern in "${PATTERNS[@]}"; do
    if git diff --cached | grep -iE "$pattern" > /dev/null; then
        echo -e "${YELLOW}⚠️  警告: 检测到可能的硬编码密钥模式${NC}"
        FOUND_PATTERN=1
    fi
done

if [ $FOUND_PATTERN -eq 1 ]; then
    echo -e "${YELLOW}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${YELLOW}检测到疑似硬编码的密钥或密码${NC}"
    echo -e "${YELLOW}请确认这不是真实的敏感信息！${NC}"
    echo -e "${YELLOW}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    
    read -p "确定要继续提交吗? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${RED}❌ 提交已取消${NC}"
        exit 1
    fi
fi

# Check for large files
LARGE_FILES=$(git diff --cached --name-only | while read file; do
    size=$(git cat-file -s ":0:$file" 2>/dev/null || echo 0)
    if [ $size -gt 10485760 ]; then  # 10MB
        echo "$file ($((size / 1024 / 1024))MB)"
    fi
done)

if [ ! -z "$LARGE_FILES" ]; then
    echo -e "${YELLOW}⚠️  警告: 检测到大文件:${NC}"
    echo "$LARGE_FILES"
    echo -e "${YELLOW}考虑使用 Git LFS 或将文件添加到 .gitignore${NC}"
    
    read -p "确定要继续提交吗? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${RED}❌ 提交已取消${NC}"
        exit 1
    fi
fi

echo -e "${GREEN}✅ 安全检查通过！${NC}"
exit 0
