// SPDX-License-Identifier: Apache-2.0
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NUnit.Framework.Internal;

namespace Agoda.NUnit.KestrelLogging;

/// <summary>
/// Middleware extensions that re-attach NUnit's AsyncLocal context for the duration of HTTP requests.
/// </summary>
/// <remarks>
/// This middleware solves the issue where NUnit's TestExecutionContext is lost when
/// ASP.NET Core handles requests on different threads, causing Console.WriteLine and
/// TestContext output to not appear in test results.
/// See: https://github.com/nunit/nunit/issues/4860
/// </remarks>
public static class AttachNUnitContextExtensions
{
    // Reflect NUnit's internal AsyncLocal<TestExecutionContext> once at startup
    private static readonly AsyncLocal<TestExecutionContext?> NUnitAsyncLocal =
        (AsyncLocal<TestExecutionContext?>)typeof(TestExecutionContext)
            .GetField("AsyncLocalCurrentContext", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;

    /// <summary>
    /// Adds middleware that attaches the NUnit test context for all requests through this pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="token">
    /// The unique token identifying which test context to restore.
    /// This token should be created per WebApplicationFactory instance to ensure parallel test safety.
    /// </param>
    /// <returns>The application builder for chaining.</returns>
    /// <remarks>
    /// This middleware is parallel-safe when each WebApplicationFactory instance uses its own token.
    /// The middleware temporarily sets NUnit's AsyncLocal context for the duration of the request,
    /// then restores the previous value.
    /// </remarks>
    public static IApplicationBuilder UseAttachNUnitTestContext(this IApplicationBuilder app, string token)
    {
        return app.Use(async (context, next) =>
        {
            var prev = NUnitAsyncLocal.Value;

            // Restore the NUnit context for this request
            if (NUnitTestContextBridge.TryGet(token, out var desired))
            {
                NUnitAsyncLocal.Value = desired;
            }

            try
            {
                await next();
            }
            finally
            {
                // Always restore the previous context
                NUnitAsyncLocal.Value = prev;
            }
        });
    }
}

