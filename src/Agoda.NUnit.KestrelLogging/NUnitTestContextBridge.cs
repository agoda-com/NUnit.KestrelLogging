// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Concurrent;
using NUnit.Framework.Internal;

namespace Agoda.NUnit.KestrelLogging;

/// <summary>
/// Parallel-safe registry mapping a per-test token to its NUnit TestExecutionContext.
/// This bridge allows ASP.NET Core middleware to restore the NUnit test context
/// that would otherwise be lost when crossing async boundaries in Kestrel.
/// </summary>
/// <remarks>
/// This class solves the issue described in https://github.com/nunit/nunit/issues/4860
/// where Console.WriteLine and TestContext output doesn't appear when called from
/// ASP.NET Core HTTP handlers due to lost ExecutionContext.
/// </remarks>
public static class NUnitTestContextBridge
{
    private static readonly ConcurrentDictionary<string, TestExecutionContext> Contexts = new();

    /// <summary>
    /// Creates a new unique token for associating a test context.
    /// </summary>
    /// <returns>A unique token string.</returns>
    public static string NewToken() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// Attaches the current NUnit test execution context to the given token.
    /// </summary>
    /// <param name="token">The unique token to associate with the current test context.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that removes the token mapping when disposed.
    /// Always dispose this at the end of your test to prevent memory leaks.
    /// </returns>
    /// <example>
    /// <code>
    /// var token = NUnitTestContextBridge.NewToken();
    /// using (NUnitTestContextBridge.AttachCurrentTest(token))
    /// {
    ///     // Test code here
    /// }
    /// </code>
    /// </example>
    public static IDisposable AttachCurrentTest(string token)
    {
        Contexts[token] = TestExecutionContext.CurrentContext;
        return new RemoveOnDispose(token);
    }

    /// <summary>
    /// Attempts to retrieve the test execution context associated with the given token.
    /// </summary>
    /// <param name="token">The token to look up.</param>
    /// <param name="ctx">The test execution context if found.</param>
    /// <returns>True if the token was found; otherwise false.</returns>
    internal static bool TryGet(string token, out TestExecutionContext? ctx) =>
        Contexts.TryGetValue(token, out ctx);

    private sealed class RemoveOnDispose(string token) : IDisposable
    {
        public void Dispose() => Contexts.TryRemove(token, out _);
    }
}

