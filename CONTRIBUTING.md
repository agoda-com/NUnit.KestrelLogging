# Contributing to Agoda.NUnit.KestrelLogging

Thank you for helping improve this project! This guide keeps contribution steps simple and up-to-date.

### Scope and Goals
- This library ensures Console.WriteLine and NUnit TestContext output appears from ASP.NET Core/Kestrel handlers in tests (see README).
- Keep changes focused, well-tested, and small where possible.

### Getting Started
1. Fork and clone your fork
2. Add upstream
   - `git remote add upstream https://github.com/agoda-com/NUnit.KestrelLogging.git`
3. Create a branch
   - `git checkout -b feat/short-description`

### Prerequisites
- .NET 8 SDK
- Windows, macOS, or Linux
- An editor/IDE (VS 2022, Rider, or VS Code)

### Build & Test
```powershell
cd src
# Build
dotnet build Agoda.NUnit.KestrelLogging.sln
# Run tests (prints out handler output to verify behavior)
dotnet test Agoda.NUnit.KestrelLogging.sln --logger:"console;verbosity=detailed"
```

### Project Layout (high level)
```
src/
  Agoda.NUnit.KestrelLogging/           # Library
  Agoda.NUnit.KestrelLogging.Test/      # Tests (unit + integration + parallel)
  SampleApp/                            # Minimal API for test scenarios
```

### Development Workflow
- Prefer small PRs with a single logical change
- Add or update tests for all behavior changes
- Update README if user-facing behavior or usage changes
- Use clear commit messages (e.g., "feat:", "fix:", "docs:", "chore:")

### Testing Expectations
- Run tests locally before pushing
- Ensure integration tests still show handler output in NUnit results
- Keep tests deterministic and parallel-safe

### CI/CD
- GitHub Actions (/.github/workflows/dotnet-ci.yml) runs on PRs and main:
  - Restore, build, test with coverage, pack, and publish artifacts
  - Optional Codecov upload (set CODECOV_TOKEN secret)
  - NuGet publish happens on main with NUGET_API_KEY

### Pull Requests
- Target branch: `main`
- Include:
  - Summary of change and motivation
  - Notes on testing done and any risks
  - Screens/log snippets if relevant
- CI must pass before review

### Reporting Issues
- Include clear reproduction steps and expected vs. actual behavior
- Share environment details (OS, .NET SDK, IDE) and minimal repro code if possible

### Code Style
- Use .editorconfig defaults
- Write clear, readable code and minimal comments where the code is self-explanatory
- Add XML docs for public APIs

### License
- By contributing, you agree your contributions are licensed under the repository’s Apache-2.0 license.

Thanks for contributing! 🙌

