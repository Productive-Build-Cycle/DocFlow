using DocFlow.Domain.Entities;
using Xunit;

namespace DocFlow.Domain.Tests;

public class DomainArchitectureTests
{
    [Fact]
    public void DocFlowDomainAssembly_ReferencedAssemblies_AreSystemOrStandardOnly()
    {
        // Arrange
        var domainAssembly = typeof(Document).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();
        var allowedPrefixes = new[] { "System", "netstandard", "mscorlib" };

        // Act & Assert
        foreach (var asm in referencedAssemblies)
        {
            var isAllowed = allowedPrefixes.Any(prefix => 
                asm.Name != null && (asm.Name == prefix || asm.Name.StartsWith(prefix)));

            Assert.True(isAllowed, 
                $"Referenced assembly '{asm.Name}' is not allowed. " +
                $"DocFlow.Domain should only reference System.* or standard .NET assemblies.");
        }
    }
}
