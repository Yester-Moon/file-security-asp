#!/bin/bash

# Git Security Setup Script for File Security System
# This script automates the setup of git security configurations

set -e  # Exit on error

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_header() {
    echo -e "\n${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

# Check if running in git repository
check_git_repo() {
    if [ ! -d ".git" ]; then
        print_error "Not a git repository!"
        exit 1
    fi
    print_success "Git repository detected"
}

# Install pre-commit hook
install_hook() {
    print_header "Installing Pre-commit Hook"
    
    if [ -f ".git/hooks/pre-commit" ]; then
        print_warning "Pre-commit hook already exists"
        read -p "Overwrite? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            print_info "Skipping hook installation"
            return
        fi
    fi

    if [ -f "pre-commit.sh" ]; then
        cp pre-commit.sh .git/hooks/pre-commit
        chmod +x .git/hooks/pre-commit
        print_success "Pre-commit hook installed"
    else
        print_error "pre-commit.sh not found!"
        exit 1
    fi
}

# Check for sensitive files in git history
check_git_history() {
    print_header "Checking Git History for Sensitive Files"
    
    SENSITIVE_PATTERNS=(
        "appsettings.json"
        "appsettings.*.json"
        "secrets.json"
        ".env"
    )

    FOUND_SENSITIVE=0

    for pattern in "${SENSITIVE_PATTERNS[@]}"; do
        if git log --all --full-history --source --name-only -- "*$pattern" 2>/dev/null | grep -q "$pattern"; then
            print_warning "Found $pattern in git history!"
            FOUND_SENSITIVE=1
        fi
    done

    if [ $FOUND_SENSITIVE -eq 1 ]; then
        echo -e "\n${RED}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
        print_error "Sensitive files found in git history!"
        print_warning "You MUST clean the git history!"
        print_info "See GIT_SECURITY.md for instructions"
        echo -e "${RED}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"
    else
        print_success "No sensitive files found in git history"
    fi
}

# Remove tracked sensitive files
remove_tracked_files() {
    print_header "Removing Tracked Sensitive Files"
    
    FILES_TO_REMOVE=(
        "FileService/FileService.WebAPI/appsettings.json"
        "FileService/FileService.WebAPI/appsettings.Development.json"
        "FileService/FileService.WebAPI/appsettings.Production.json"
    )

    REMOVED_ANY=0

    for file in "${FILES_TO_REMOVE[@]}"; do
        if git ls-files --error-unmatch "$file" > /dev/null 2>&1; then
            print_warning "Removing $file from git tracking"
            git rm --cached "$file"
            REMOVED_ANY=1
        fi
    done

    if [ $REMOVED_ANY -eq 1 ]; then
        print_info "Files removed from tracking. Don't forget to commit!"
    else
        print_success "No tracked sensitive files found"
    fi
}

# Create configuration from template
create_config() {
    print_header "Creating Configuration Files"
    
    TEMPLATE="FileService/FileService.WebAPI/appsettings.Development.json.example"
    CONFIG="FileService/FileService.WebAPI/appsettings.json"

    if [ ! -f "$TEMPLATE" ]; then
        print_error "Template file not found: $TEMPLATE"
        return
    fi

    if [ -f "$CONFIG" ]; then
        print_warning "Configuration file already exists: $CONFIG"
        read -p "Overwrite? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            print_info "Skipping configuration creation"
            return
        fi
    fi

    cp "$TEMPLATE" "$CONFIG"
    print_success "Configuration file created from template"
    print_warning "Don't forget to update with your actual values!"
}

# Generate secure keys
generate_keys() {
    print_header "Generating Secure Keys"
    
    print_info "Use these keys in your appsettings.json:"
    echo ""

    # JWT Key
    JWT_KEY=$(openssl rand -base64 64 | tr -d '\n')
    echo -e "${GREEN}JWT Key:${NC}"
    echo "$JWT_KEY"
    echo ""

    # AES Key
    AES_KEY=$(openssl rand -base64 32 | tr -d '\n')
    echo -e "${GREEN}AES Encryption Key:${NC}"
    echo "$AES_KEY"
    echo ""

    # AES IV
    AES_IV=$(openssl rand -base64 16 | tr -d '\n')
    echo -e "${GREEN}AES IV:${NC}"
    echo "$AES_IV"
    echo ""

    print_warning "Save these keys securely!"
}

# Test pre-commit hook
test_hook() {
    print_header "Testing Pre-commit Hook"
    
    # Create a temporary sensitive file
    TEMP_FILE=".test_sensitive_file.json"
    echo '{"password": "test123"}' > "$TEMP_FILE"
    
    git add "$TEMP_FILE" 2>/dev/null || true
    
    if git commit -m "Test commit" --dry-run 2>&1 | grep -q "error\|ERROR"; then
        print_success "Pre-commit hook is working!"
    else
        print_warning "Pre-commit hook might not be working correctly"
    fi
    
    # Clean up
    git reset HEAD "$TEMP_FILE" 2>/dev/null || true
    rm -f "$TEMP_FILE"
}

# Main setup flow
main() {
    print_header "🔒 Git Security Setup for File Security System"
    
    echo "This script will:"
    echo "  1. Install pre-commit hook"
    echo "  2. Check git history for sensitive files"
    echo "  3. Remove tracked sensitive files"
    echo "  4. Create configuration from template"
    echo "  5. Generate secure keys"
    echo "  6. Test the setup"
    echo ""
    
    read -p "Continue? (Y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Nn]$ ]]; then
        print_info "Setup cancelled"
        exit 0
    fi

    check_git_repo
    install_hook
    check_git_history
    remove_tracked_files
    create_config
    
    echo ""
    read -p "Generate secure keys? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        generate_keys
    fi
    
    test_hook

    print_header "✅ Setup Complete!"
    
    echo -e "${GREEN}Next steps:${NC}"
    echo "  1. Update appsettings.json with your values"
    echo "  2. If sensitive files were found in history, clean it (see GIT_SECURITY.md)"
    echo "  3. If files were removed from tracking, commit the changes:"
    echo -e "     ${BLUE}git commit -m \"Remove sensitive files from version control\"${NC}"
    echo "  4. Review GIT_SECURITY.md for best practices"
    echo ""
    
    print_success "All done! Your git security is configured. 🎉"
}

# Run main function
main
