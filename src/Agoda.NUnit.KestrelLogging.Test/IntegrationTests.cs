// SPDX-License-Identifier: Apache-2.0
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Agoda.NUnit.KestrelLogging;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Agoda.NUnit.KestrelLogging.Test;

[TestFixture]
public class IntegrationTests
{
    [Test]
    public async Task GetRootEndpoint_ReturnsHelloWorld()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope);

        using (scope)
        {
            var client = factory.CreateClient();

            // Act
            TestContext.WriteLine("Calling GET /");
            var response = await client.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Is.EqualTo("Hello World!"));
            TestContext.WriteLine($"Response: {content}");
        }
    }

    [Test]
    public async Task GetTestEndpoint_ReturnsJson()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope);

        using (scope)
        {
            var client = factory.CreateClient();

            // Act
            TestContext.WriteLine("Calling GET /api/test");
            var response = await client.GetAsync("/api/test");
            var result = await response.Content.ReadFromJsonAsync<TestResponse>();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Message, Is.EqualTo("Test successful"));
            TestContext.WriteLine($"Response: {result.Message} at {result.Timestamp}");
        }
    }

    [Test]
    public async Task PostEchoEndpoint_EchoesMessage()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope);

        using (scope)
        {
            var client = factory.CreateClient();
            var request = new { Message = "Test message from NUnit" };

            // Act
            TestContext.WriteLine($"Calling POST /api/echo with: {request.Message}");
            var response = await client.PostAsJsonAsync("/api/echo", request);
            var result = await response.Content.ReadFromJsonAsync<EchoResponse>();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Echo, Is.EqualTo(request.Message));
            TestContext.WriteLine($"Echo response: {result.Echo} at {result.Timestamp}");
        }
    }

    [Test]
    public async Task MultipleRequests_AllHaveTestContext()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope);

        using (scope)
        {
            var client = factory.CreateClient();

            // Act & Assert - make multiple requests
            for (int i = 0; i < 5; i++)
            {
                TestContext.WriteLine($"Request {i + 1}");
                var response = await client.GetAsync("/");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }

            TestContext.WriteLine("All requests completed successfully");
        }
    }

    [Test]
    public async Task WithCustomLogging_CallbackWorks()
    {
        // Arrange
        var callbackInvoked = false;
        await using var factory = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope, logging =>
            {
                // Configure additional logging settings if needed
                callbackInvoked = true;
            });

        using (scope)
        {
            var client = factory.CreateClient();

            // Act
            TestContext.WriteLine("Testing with custom logging configuration");
            var response = await client.GetAsync("/api/test");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(callbackInvoked, Is.True, "Logging configuration callback should be invoked");
            TestContext.WriteLine("Custom logging configuration applied successfully");
        }
    }

    private record TestResponse(string Message, DateTime Timestamp);
}

