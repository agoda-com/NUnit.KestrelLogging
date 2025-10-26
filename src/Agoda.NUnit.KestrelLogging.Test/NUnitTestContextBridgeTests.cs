// SPDX-License-Identifier: Apache-2.0
using Agoda.NUnit.KestrelLogging;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Agoda.NUnit.KestrelLogging.Test;

[TestFixture]
public class NUnitTestContextBridgeTests
{
    [Test]
    public void NewToken_ReturnsUniqueTokens()
    {
        // Arrange & Act
        var token1 = NUnitTestContextBridge.NewToken();
        var token2 = NUnitTestContextBridge.NewToken();

        // Assert
        Assert.That(token1, Is.Not.Null);
        Assert.That(token2, Is.Not.Null);
        Assert.That(token1, Is.Not.EqualTo(token2));
    }

    [Test]
    public void NewToken_ReturnsValidGuidFormat()
    {
        // Arrange & Act
        var token = NUnitTestContextBridge.NewToken();

        // Assert
        Assert.That(token, Has.Length.EqualTo(32)); // GUID without hyphens
        Assert.That(token, Does.Match("^[a-f0-9]{32}$"));
    }

    [Test]
    public void AttachCurrentTest_StoresCurrentContext()
    {
        // Arrange
        var token = NUnitTestContextBridge.NewToken();
        var currentContext = TestExecutionContext.CurrentContext;

        // Act
        using (var scope = NUnitTestContextBridge.AttachCurrentTest(token))
        {
            // Assert
            var retrieved = typeof(NUnitTestContextBridge)
                .GetMethod("TryGet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { token, null! });

            Assert.That(retrieved, Is.True);
        }
    }

    [Test]
    public void AttachCurrentTest_DisposalRemovesContext()
    {
        // Arrange
        var token = NUnitTestContextBridge.NewToken();

        // Act
        using (NUnitTestContextBridge.AttachCurrentTest(token))
        {
            // Context should be present during scope
        }

        // Assert - context should be removed after disposal
        var tryGetMethod = typeof(NUnitTestContextBridge)
            .GetMethod("TryGet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        
        var parameters = new object?[] { token, null };
        var result = (bool)tryGetMethod.Invoke(null, parameters)!;
        
        Assert.That(result, Is.False);
    }

    [Test]
    public void AttachCurrentTest_SupportsMultipleTokensSimultaneously()
    {
        // Arrange
        var token1 = NUnitTestContextBridge.NewToken();
        var token2 = NUnitTestContextBridge.NewToken();
        var token3 = NUnitTestContextBridge.NewToken();

        // Act
        using (var scope1 = NUnitTestContextBridge.AttachCurrentTest(token1))
        using (var scope2 = NUnitTestContextBridge.AttachCurrentTest(token2))
        using (var scope3 = NUnitTestContextBridge.AttachCurrentTest(token3))
        {
            // Assert - all contexts should be retrievable
            var tryGetMethod = typeof(NUnitTestContextBridge)
                .GetMethod("TryGet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

            var result1 = (bool)tryGetMethod.Invoke(null, new object?[] { token1, null })!;
            var result2 = (bool)tryGetMethod.Invoke(null, new object?[] { token2, null })!;
            var result3 = (bool)tryGetMethod.Invoke(null, new object?[] { token3, null })!;

            Assert.That(result1, Is.True);
            Assert.That(result2, Is.True);
            Assert.That(result3, Is.True);
        }
    }

    [Test]
    public void AttachCurrentTest_CanBeCalledMultipleTimesWithSameToken()
    {
        // Arrange
        var token = NUnitTestContextBridge.NewToken();

        // Act & Assert - should not throw
        using (var scope1 = NUnitTestContextBridge.AttachCurrentTest(token))
        {
            // Reusing the same token should overwrite the previous mapping
            using (var scope2 = NUnitTestContextBridge.AttachCurrentTest(token))
            {
                Assert.Pass("Multiple attachments with same token succeeded");
            }
        }
    }
}

