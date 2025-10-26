// SPDX-License-Identifier: Apache-2.0
using System;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Agoda.NUnit.KestrelLogging;

/// <summary>
/// A logging provider that writes logs to NUnit's test output.
/// </summary>
/// <remarks>
/// This provider ensures that logs from ASP.NET Core applications appear in NUnit test output,
/// making it easier to debug test failures and understand application behavior during tests.
/// </remarks>
public sealed class NUnitLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Creates a new logger for the specified category.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>A new <see cref="ILogger"/> instance.</returns>
    public ILogger CreateLogger(string categoryName) => new NUnitLogger(categoryName);

    /// <summary>
    /// Disposes the provider.
    /// </summary>
    public void Dispose()
    {
        // No resources to dispose
    }

    private sealed class NUnitLogger(string category) : ILogger
    {
        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoOpDisposable.Instance;

        /// <summary>
        /// Checks if the given log level is enabled.
        /// </summary>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <summary>
        /// Writes a log entry to NUnit's test output.
        /// </summary>
        public void Log<TState>(
            LogLevel level,
            EventId id,
            TState state,
            Exception? ex,
            Func<TState, Exception?, string> formatter)
        {
            var message = $"{category} [{level}] {formatter(state, ex)}{(ex is null ? "" : $" {ex}")}";
            
            // Write to NUnit's test output stream
            // Using TestContext.Out instead of TestContext.Progress ensures output appears in test results
            TestContext.Out.WriteLine(message);
        }

        private sealed class NoOpDisposable : IDisposable
        {
            public static readonly NoOpDisposable Instance = new();
            public void Dispose() { }
        }
    }
}

