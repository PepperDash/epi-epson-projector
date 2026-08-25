using FluentAssertions;
using Xunit;

namespace EpsonProjector.Tests;

// MinimumEssentialsFrameworkVersion and TypeNames are assigned at runtime in the factory
// constructor, which MetadataLoadContext cannot execute — source scanning is the only way
// to verify these without spinning up the Essentials runtime.
public class FactoryMetadataTests
{
    private const string ExpectedMinimumEssentialsFrameworkVersion = "3.0.0";

    [Fact]
    public void Factory_Source_Sets_MinimumEssentialsFrameworkVersion()
    {
        var source = AssemblyFixture.FindSourceForClass("DeviceFactory");
        source.Should().NotBeNull();
        source!.Should().Contain($"MinimumEssentialsFrameworkVersion = \"{ExpectedMinimumEssentialsFrameworkVersion}\"");
    }

    [Fact]
    public void Factory_Source_Sets_TypeNames()
    {
        var source = AssemblyFixture.FindSourceForClass("DeviceFactory");
        source.Should().NotBeNull();
        source!.Should().Contain("TypeNames = new List<string>()");
    }

    [Theory]
    [InlineData("DeviceFactory", "epsonProjector")]
    public void Factory_Source_Contains_TypeName(string factoryClassName, string typeName)
    {
        var source = AssemblyFixture.FindSourceForClass(factoryClassName);
        source.Should().NotBeNull();
        source!.Should().Contain($"\"{typeName}\"");
    }
}
