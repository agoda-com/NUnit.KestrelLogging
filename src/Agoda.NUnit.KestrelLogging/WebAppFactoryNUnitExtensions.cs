// SPDX-License-Identifier: Apache-2.0
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agoda.NUnit.KestrelLogging;

/// <summary>
/// Extension methods for <see cref="WebApplicationFactory{TEntryPoint}"/> that simplify
/// integration with NUnit test logging.
/// </summary>
public static class WebAppFactoryNUnitExtensions
{
    /// <summary>
    /// Configures a WebApplicationFactory to work seamlessly with NUnit test output.
    /// </summary>
    /// <typeparam name="TEntryPoint">The entry point type of the web application.</typeparam>
    /// <param name="baseFactory">The WebApplicationFactory instance to configure.</param>
    /// <param name="scope">
    /// An <see cref="IDisposable"/> that must be disposed at the end of the test.
    /// This cleans up the test context mapping and prevents memory leaks.
    /// </param>
    /// <param name="configureLogging">
    /// Optional callback to configure additional logging settings (e.g., minimum log levels, filters).
    /// </param>
    /// <returns>A configured WebApplicationFactory instance.</returns>
    /// <remarks>
    /// <para>
    /// This extension method solves the issue where Console.WriteLine and TestContext output
    /// doesn't appear when called from ASP.NET Core HTTP handlers in NUnit tests.
    /// See: https://github.com/nunit/nunit/issues/4860
    /// </para>
    /// <para>
    /// The method works by:
    /// 1. Creating a unique token for this test
    /// 2. Associating the current NUnit test context with that token
    /// 3. Configuring middleware to restore the test context for each HTTP request
    /// 4. Setting up logging to write to NUnit's test output
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Test]
    /// public async Task TestEndpoint()
    /// {
    ///     await using var factory = new WebApplicationFactory&lt;Program&gt;()
    ///         .WithNUnitTestLogging(out var scope);
    ///     
    ///     using (scope)
    ///     {
    ///         var client = factory.CreateClient();
    ///         var response = await client.GetAsync("/");
    ///         response.EnsureSuccessStatusCode();
    ///     }
    /// }
    /// </code>
    /// </example>
    public static WebApplicationFactory<TEntryPoint> WithNUnitTestLogging<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> baseFactory,
        out IDisposable scope,
        Action<ILoggingBuilder>? configureLogging = null)
        where TEntryPoint : class
    {
        // Create a unique token for this test instance
        var token = NUnitTestContextBridge.NewToken();
        
        // Attach the current test context and get a disposable scope
        scope = NUnitTestContextBridge.AttachCurrentTest(token);

        // Configure the web host
        return baseFactory.WithWebHostBuilder(builder =>
        {
            // Register a startup filter to add our middleware at the beginning of the pipeline
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IStartupFilter>(_ =>
                    new NUnitAttachStartupFilter(token));
            });

            // Configure logging to use NUnit's test output
            builder.ConfigureLogging(lb =>
            {
                // Clear default providers (console, debug, etc.)
                lb.ClearProviders();
                
                // Add our NUnit logger provider
                lb.AddProvider(new NUnitLoggerProvider());
                
                // Allow additional configuration
                configureLogging?.Invoke(lb);
            });
        });
    }
}

