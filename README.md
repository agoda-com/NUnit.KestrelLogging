# Agoda.NUnit.KestrelLogging

[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Agoda.NUnit.KestrelLogging.svg)](https://www.nuget.org/packages/Agoda.NUnit.KestrelLogging/)

A helper library for NUnit tests with ASP.NET Core Kestrel that fixes `Console.WriteLine` and `TestContext` output not appearing in test results.

## The Problem

When testing ASP.NET Core applications with NUnit and `WebApplicationFactory`, output from `Console.WriteLine` and `TestContext.WriteLine` inside HTTP request handlers doesn't appear in test results. This is because NUnit's `TestExecutionContext` is stored in an `AsyncLocal<T>` that gets lost when Kestrel handles requests on different threads.

**Issue Reference**: [nunit/nunit#4860](https://github.com/nunit/nunit/issues/4860)

### Example of the Problem

```csharp
[Test]
public async Task TestEndpoint()
{
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var response = await client.GetAsync("/");
    // Console.WriteLine from inside the endpoint handler won't appear! ❌
}
```

## The Solution

This library provides a simple extension method that restores the NUnit test context for all HTTP requests, ensuring that all output appears in your test results.

## Installation

```bash
dotnet add package Agoda.NUnit.KestrelLogging
```

## Usage

Simply call `.WithNUnitTestLogging(out var scope)` on your `WebApplicationFactory` and dispose the scope when your test completes:

```csharp
using Agoda.NUnit.KestrelLogging;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

[Test]
public async Task TestEndpoint()
{
    // Create factory with NUnit logging support
    await using var factory = new WebApplicationFactory<Program>()
        .WithNUnitTestLogging(out var scope);

    using (scope)
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/");
        
        // Console.WriteLine from inside the endpoint handler will now appear! ✅
        response.EnsureSuccessStatusCode();
    }
}
```

### Your Endpoint Code

```csharp
app.MapGet("/", () =>
{
    Console.WriteLine("This will now appear in test output!");
    TestContext.WriteLine("This works too!");
    return "Hello World!";
});
```

## How It Works

The library solves the problem by:

1. **Creating a unique token** for each test instance
2. **Storing the NUnit test context** associated with that token
3. **Adding middleware** that restores the test context for each HTTP request
4. **Configuring logging** to write to NUnit's test output

This ensures that the `AsyncLocal<TestExecutionContext>` is properly restored when Kestrel handles requests, making all output appear in test results.

## Requirements

- .NET 8.0 or later
- NUnit 4.0 or later
- Microsoft.AspNetCore.Mvc.Testing 8.0 or later

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ by Agoda**
