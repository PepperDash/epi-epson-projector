using FluentAssertions;
using Xunit;

namespace EpsonProjector.Tests;

public class ConfigDeserializationTests
{
    [Fact]
    public void PropsConfig_Class_Exists()
    {
        AssemblyFixture.PluginAssembly.GetType("PepperDash.Essentials.Plugins.PropsConfig")
            .Should().NotBeNull();
    }

    [Fact]
    public void PropsConfig_Has_Parameterless_Constructor()
    {
        var type = AssemblyFixture.PluginAssembly.GetType("PepperDash.Essentials.Plugins.PropsConfig");
        type!.GetConstructor(Type.EmptyTypes).Should().NotBeNull();
    }

    [Theory]
    [InlineData("Control")]
    [InlineData("Monitor")]
    [InlineData("EnableBridgeComms")]
    [InlineData("WarmupTimeMs")]
    [InlineData("CooldownTimeMs")]
    [InlineData("ActiveInputs")]
    [InlineData("DontUnmuteVideoOnRoute")]
    public void PropsConfig_Has_Expected_Property(string propertyName)
    {
        var type = AssemblyFixture.PluginAssembly.GetType("PepperDash.Essentials.Plugins.PropsConfig");
        type!.GetProperty(propertyName).Should().NotBeNull();
    }

    [Theory]
    [InlineData("ActiveInputs", "activeInputs")]
    [InlineData("DontUnmuteVideoOnRoute", "dontUnmuteVideoOnRoute")]
    public void PropsConfig_Property_Has_JsonPropertyAttribute(string propertyName, string jsonName)
    {
        var type = AssemblyFixture.PluginAssembly.GetType("PepperDash.Essentials.Plugins.PropsConfig");
        var property = type!.GetProperty(propertyName);

        var hasAttribute = property!.CustomAttributes.Any(a =>
            a.AttributeType.Name == "JsonPropertyAttribute"
            && a.ConstructorArguments.Any(arg =>
                string.Equals(arg.Value?.ToString(), jsonName, StringComparison.Ordinal)));

        hasAttribute.Should().BeTrue($"{propertyName} should be decorated with [JsonProperty(\"{jsonName}\")]");
    }

    [Fact]
    public void ActiveInputs_Class_Exists()
    {
        AssemblyFixture.PluginAssembly.GetType("PepperDash.Essentials.Plugins.ActiveInputs")
            .Should().NotBeNull();
    }

    [Fact]
    public void ActiveInputs_Has_Parameterless_Constructor()
    {
        var type = AssemblyFixture.PluginAssembly.GetType("PepperDash.Essentials.Plugins.ActiveInputs");
        type!.GetConstructor(Type.EmptyTypes).Should().NotBeNull();
    }

    [Theory]
    [InlineData("Key", "key")]
    [InlineData("Name", "name")]
    public void ActiveInputs_Property_Has_JsonPropertyAttribute(string propertyName, string jsonName)
    {
        var type = AssemblyFixture.PluginAssembly.GetType("PepperDash.Essentials.Plugins.ActiveInputs");
        var property = type!.GetProperty(propertyName);

        var hasAttribute = property!.CustomAttributes.Any(a =>
            a.AttributeType.Name == "JsonPropertyAttribute"
            && a.ConstructorArguments.Any(arg =>
                string.Equals(arg.Value?.ToString(), jsonName, StringComparison.Ordinal)));

        hasAttribute.Should().BeTrue($"{propertyName} should be decorated with [JsonProperty(\"{jsonName}\")]");
    }
}
