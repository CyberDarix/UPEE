Follow us...
www.linkedin.com/in/cyberdarix-cyberdarix-082986402

# Contributing to UPEE

Thank you for considering contributing to the Universal Polyglot Execution Engine! 

## How to Contribute

### Reporting Issues
1. Check existing GitHub issues to avoid duplicates
2. Provide clear description, reproduction steps, and expected behavior
3. Include environment details (OS, .NET version, language versions)
4. Attach logs or error messages if relevant

### Submitting Code Changes

1. **Fork** the repository
2. **Clone** your fork: `git clone https://github.com/YOUR-USERNAME/upee.git`
3. **Create a branch**: `git checkout -b feature/my-feature`
4. **Make changes**: Follow code style guidelines (see below)
5. **Test**: Run `dotnet test` and ensure all tests pass
6. **Commit**: Use clear, descriptive commit messages
7. **Push**: `git push origin feature/my-feature`
8. **Create Pull Request**: Provide detailed description of changes

### Code Style Guidelines

- **Language**: C# 12+ (uses latest language features)
- **Naming**: Use PascalCase for public members, camelCase for local variables
- **Async**: Always use `async/await` for I/O operations
- **Logging**: Use `UniversalLogger` for all diagnostic output
- **Comments**: Only add comments for complex logic; code should be self-documenting

### Adding Language Support

To add a new language executor:

1. Update `SandboxEnvironment.GetExecutorForScript()` with new file extension mapping
2. Verify the language runtime/compiler is installed on target systems
3. Add unit tests in `UPEE.Tests/UnitTests.cs`
4. Update documentation with examples
5. Create a sample script in `/examples` directory

Example:
```csharp
// In SandboxEnvironment.cs
case ".jl": // Julia
	return "julia";
```

### Development Setup

```bash
# Prerequisites
- .NET 10 SDK or later
- Visual Studio Community 2026 (or VS Code)
- Language runtimes: Python, Node.js, Rust, Go, Lua (for testing)

# Build
dotnet build UPEE.sln --configuration Release

# Test
dotnet test UPEE.Tests/UPEE.Tests.csproj

# Run
dotnet UPEE.CLI/bin/Release/net10.0/UPEE.CLI.dll connect "C:\workspace"
```

### Pull Request Process

1. Ensure all tests pass: `dotnet test`
2. Update documentation and README if needed
3. Add/update unit tests for new functionality
4. One approval from maintainer required
5. Pass CI/CD checks

### Code Review Criteria

- **Functionality**: Does it solve the problem?
- **Testing**: Are there adequate unit/integration tests?
- **Documentation**: Is the change documented?
- **Performance**: Does it introduce regressions?
- **Security**: Does it follow secure coding practices?

## Areas for Contribution

- [ ] Additional language support (Assembly, SQL, Ruby, Perl)
- [ ] Performance optimizations
- [ ] Cross-platform testing and fixes
- [ ] Documentation improvements
- [ ] Example scripts and use cases
- [ ] CI/CD pipeline enhancements
- [ ] Security audits
- [ ] Internationalization (i18n)

## Community Standards

- Be respectful and inclusive
- Assume good intent
- Provide constructive feedback
- Help others learn and grow

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Questions?

Open a GitHub Discussion or email: **support@upee-engine.dev**

---

Thank you for making UPEE better! 🚀
