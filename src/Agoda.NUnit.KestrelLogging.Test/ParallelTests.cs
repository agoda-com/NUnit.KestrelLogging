// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Agoda.NUnit.KestrelLogging;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Agoda.NUnit.KestrelLogging.Test;

/// <summary>
/// Tests to verify that the library works correctly with parallel test execution.
/// This is critical because the original issue (https://github.com/nunit/nunit/issues/4860)
/// involves ExecutionContext being lost across async boundaries.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class ParallelTests
{
    [Test]
    [Repeat(3)]
    public async Task ParallelTest1_WorksCorrectly()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope);

        using (scope)
        {
            var client = factory.CreateClient();
            TestContext.WriteLine($"ParallelTest1 - Thread {Environment.CurrentManagedThreadId}");
            
            var response = await client.GetAsync("/");
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            TestContext.WriteLine($"ParallelTest1 completed on thread {Environment.CurrentManagedThreadId}");
        }
    }

    [Test]
    [Repeat(3)]
    public async Task ParallelTest2_WorksCorrectly()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope);

        using (scope)
        {
            var client = factory.CreateClient();
            TestContext.WriteLine($"ParallelTest2 - Thread {Environment.CurrentManagedThreadId}");
            
            var response = await client.GetAsync("/api/test");
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            TestContext.WriteLine($"ParallelTest2 completed on thread {Environment.CurrentManagedThreadId}");
        }
    }

    [Test]
    [Repeat(3)]
    public async Task ParallelTest3_WorksCorrectly()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope);

        using (scope)
        {
            var client = factory.CreateClient();
            TestContext.WriteLine($"ParallelTest3 - Thread {Environment.CurrentManagedThreadId}");
            
            var response = await client.GetAsync("/");
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            TestContext.WriteLine($"ParallelTest3 completed on thread {Environment.CurrentManagedThreadId}");
        }
    }

    [Test]
    public async Task ConcurrentRequests_AllSucceed()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope);

        using (scope)
        {
            var client = factory.CreateClient();
            var results = new ConcurrentBag<bool>();

            // Make 10 concurrent requests
            var tasks = Enumerable.Range(0, 10).Select(async i =>
            {
                TestContext.WriteLine($"Starting request {i}");
                var response = await client.GetAsync($"/");
                var success = response.StatusCode == HttpStatusCode.OK;
                results.Add(success);
                TestContext.WriteLine($"Request {i} completed: {success}");
                return success;
            });

            await Task.WhenAll(tasks);

            // Assert all requests succeeded
            Assert.That(results.Count, Is.EqualTo(10));
            Assert.That(results.All(r => r), Is.True);
            TestContext.WriteLine("All concurrent requests completed successfully");
        }
    }

    [Test]
    public async Task MultipleFactories_CanRunInParallel()
    {
        // Create multiple factories simultaneously
        await using var factory1 = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope1);
        await using var factory2 = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope2);
        await using var factory3 = new WebApplicationFactory<Program>()
            .WithNUnitTestLogging(out var scope3);

        using (scope1)
        using (scope2)
        using (scope3)
        {
            var client1 = factory1.CreateClient();
            var client2 = factory2.CreateClient();
            var client3 = factory3.CreateClient();

            TestContext.WriteLine("Making parallel requests with multiple factories");

            // Make requests in parallel
            var task1 = client1.GetAsync("/");
            var task2 = client2.GetAsync("/api/test");
            var task3 = client3.GetAsync("/");

            var responses = await Task.WhenAll(task1, task2, task3);

            // Assert
            Assert.That(responses[0].StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responses[1].StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responses[2].StatusCode, Is.EqualTo(HttpStatusCode.OK));

            TestContext.WriteLine("All parallel factories completed successfully");
        }
    }
}

