// SPDX-License-Identifier: Apache-2.0
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Agoda.NUnit.KestrelLogging;

/// <summary>
/// Startup filter that ensures the NUnit context attachment middleware is added
/// at the beginning of the middleware pipeline.
/// </summary>
/// <remarks>
/// This filter is registered as a singleton service and ensures that the middleware
/// runs before any other middleware in the pipeline, guaranteeing that the NUnit
/// context is available for all subsequent middleware and request handlers.
/// </remarks>
internal sealed class NUnitAttachStartupFilter(string token) : IStartupFilter
{
    /// <summary>
    /// Configures the middleware pipeline to include NUnit context attachment.
    /// </summary>
    /// <param name="next">The next configuration action in the pipeline.</param>
    /// <returns>An action that configures the application builder.</returns>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            // Add our middleware at the start of the pipeline
            app.UseAttachNUnitTestContext(token);
            
            // Continue with the rest of the pipeline
            next(app);
        };
    }
}

