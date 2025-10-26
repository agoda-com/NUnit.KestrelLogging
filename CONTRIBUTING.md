# Contributing to Agoda.NUnit.KestrelLogging

Thank you for your interest in contributing to Agoda.NUnit.KestrelLogging! This document provides guidelines and instructions for contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [How to Contribute](#how-to-contribute)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Adding New Analyzers](#adding-new-analyzers)

## Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code. Please be respectful and professional in all interactions.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR-USERNAME/Agoda.NUnit.KestrelLogging.git`
3. Add upstream remote: `git remote add upstream https://github.com/agoda-com/Agoda.NUnit.KestrelLogging.git`
4. Create a feature branch: `git checkout -b feature/your-feature-name`

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (17.8+) or JetBrains Rider
- Git

### Building the Project

```powershell
# Build the solution
cd src
dotnet build Agoda.NUnit.KestrelLogging.sln

# Run tests
dotnet test Agoda.NUnit.KestrelLogging.sln
```

### Debugging Analyzers

To debug an analyzer in Visual Studio:

1. Set the `Agoda.NUnit.KestrelLogging.Test` project as the startup project
2. Set breakpoints in your analyzer code
3. Run the test project with F5

## Project Structure

```
Agoda.NUnit.KestrelLogging/
├── src/
│   ├── Agoda.NUnit.KestrelLogging/          # Analyzer implementation
│   │   ├── Analyzers/                        # Diagnostic analyzers
│   │   ├── CodeFixes/                        # Code fix providers
│   │   └── Resources/                        # Localized strings
│   ├── Agoda.NUnit.KestrelLogging.Test/     # Unit tests
│   │   ├── Analyzers/                        # Analyzer tests
│   │   └── CodeFixes/                        # Code fix tests
│   ├── Agoda.NUnit.KestrelLogging.sln       # Main solution
│   └── Agoda.NUnit.KestrelLogging.Build.sln # Build solution
├── doc/                                      # Rule documentation
│   ├── AG0001.md
│   └── ...
├── README.md
├── CONTRIBUTING.md
└── LICENSE
```

## How to Contribute

### Reporting Bugs

1. Check if the issue already exists in the [issue tracker](https://github.com/agoda-com/Agoda.NUnit.KestrelLogging/issues)
2. If not, create a new issue with:
   - Clear, descriptive title
   - Steps to reproduce
   - Expected vs actual behavior
   - Code samples demonstrating the issue
   - Environment details (OS, .NET version, IDE)

### Suggesting Enhancements

1. Check existing issues and pull requests
2. Create an issue describing:
   - The enhancement and its benefits
   - Use cases
   - Potential implementation approach

### Contributing Code

1. Discuss significant changes in an issue first
2. Follow the coding standards (see below)
3. Write tests for your changes
4. Update documentation as needed
5. Submit a pull request

## Coding Standards

### General Guidelines

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for variables, methods, and classes
- Keep methods small and focused
- Add XML documentation comments for public APIs
- Enable nullable reference types and handle nullability properly

### Analyzer-Specific Guidelines

1. **Diagnostic IDs**: Use the format `AGXXXX` where `XXXX` is a sequential number
2. **Categories**: Use appropriate categories (e.g., "Usage", "Design", "Performance")
3. **Severity**: Choose appropriate default severity (Error, Warning, Info, Hidden)
4. **Messages**: Write clear, actionable diagnostic messages
5. **Performance**: Analyzers must be performant; avoid expensive operations

### Code Example

```csharp
/// <summary>
/// Analyzer that detects improper Kestrel logging configuration in NUnit tests.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class KestrelLoggingAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AG0001";
    
    private static readonly LocalizableString Title = 
        new LocalizableResourceString(nameof(Resources.AG0001Title), Resources.ResourceManager, typeof(Resources));
    
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        messageFormat: new LocalizableResourceString(nameof(Resources.AG0001MessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.AG0001Description), Resources.ResourceManager, typeof(Resources)));
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        // Register your analysis actions here
    }
}
```

## Testing Guidelines

### Test Structure

- Use NUnit for test framework
- Use Roslyn testing helpers (`Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.NUnit`)
- One test class per analyzer/code fix
- Descriptive test method names

### Test Example

```csharp
[TestFixture]
public class KestrelLoggingAnalyzerTests
{
    [Test]
    public async Task TestMethod_WithImproperConfiguration_ReportsDiagnostic()
    {
        var test = @"
using NUnit.Framework;

public class Tests
{
    [Test]
    public void TestMethod()
    {
        // Test code here
    }
}";

        var expected = VerifyCS.Diagnostic(KestrelLoggingAnalyzer.DiagnosticId)
            .WithLocation(8, 5)
            .WithArguments("TestMethod");
            
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }
}
```

### Test Coverage

- Aim for high test coverage (>80%)
- Test both positive and negative cases
- Test edge cases and boundary conditions
- Test code fixes thoroughly

## Pull Request Process

1. **Before Submitting**:
   - Ensure all tests pass: `dotnet test`
   - Ensure code builds without warnings: `dotnet build`
   - Update documentation if needed
   - Add/update tests for your changes

2. **PR Description**:
   - Reference related issues
   - Describe what changed and why
   - Include before/after examples if applicable
   - Note any breaking changes

3. **Review Process**:
   - Maintainers will review your PR
   - Address feedback and comments
   - Keep your PR up to date with the main branch
   - Once approved, a maintainer will merge your PR

4. **After Merge**:
   - Delete your feature branch
   - Update your fork: `git pull upstream main`

## Adding New Analyzers

When adding a new analyzer:

1. **Create the Analyzer**:
   - Add a new class in `src/Agoda.NUnit.KestrelLogging/Analyzers/`
   - Follow naming convention: `[Feature]Analyzer.cs`
   - Assign a new diagnostic ID (next available AGXXXX)

2. **Create the Code Fix** (if applicable):
   - Add a new class in `src/Agoda.NUnit.KestrelLogging/CodeFixes/`
   - Follow naming convention: `[Feature]CodeFixProvider.cs`

3. **Add Resources**:
   - Update `Resources.resx` with diagnostic strings
   - Include: Title, MessageFormat, Description

4. **Write Tests**:
   - Create test class in `src/Agoda.NUnit.KestrelLogging.Test/Analyzers/`
   - Test all scenarios (positive, negative, edge cases)
   - Add code fix tests if applicable

5. **Document the Rule**:
   - Create a markdown file in `doc/` (e.g., `AG0001.md`)
   - Include:
     - Rule description
     - Why it's important
     - Examples of violations
     - Examples of correct code
     - How to fix violations

6. **Update README**:
   - Add the new rule to the rules table in README.md

## Questions?

If you have questions, please:
- Check existing documentation
- Search closed issues
- Open a new issue with the "question" label

Thank you for contributing! 🎉

