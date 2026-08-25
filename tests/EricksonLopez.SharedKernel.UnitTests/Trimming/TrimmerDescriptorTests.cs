// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using AwesomeAssertions;
using EricksonLopez.SharedKernel;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Trimming;

public class TrimmerDescriptorTests
{
    private static readonly Assembly TargetAssembly = typeof(Entity<>).Assembly;

    [Fact]
    public void TrimmerDescriptor_EmbeddedXml_MustExistAndBeValidXml()
    {
        using var stream = TargetAssembly.GetManifestResourceStream("ILLink.Descriptors.xml");
        stream.Should().NotBeNull(because: "ILLink.Descriptors.xml must be embedded in the assembly for IL Linker / Trimmer.");

        using var reader = new StreamReader(stream!);
        var xmlContent = reader.ReadToEnd();
        xmlContent.Should().NotBeNullOrWhiteSpace();

        var doc = XDocument.Parse(xmlContent);
        doc.Root.Should().NotBeNull();
        doc.Root!.Name.LocalName.Should().Be("linker");
    }

    [Fact]
    public void TrimmerDescriptor_AllRegisteredTypes_MustExistInAssemblyAndBePublic()
    {
        using var stream = TargetAssembly.GetManifestResourceStream("ILLink.Descriptors.xml");
        stream.Should().NotBeNull();

        var doc = XDocument.Load(stream!);
        var assemblyElements = doc.Descendants("assembly").ToList();
        assemblyElements.Should().NotBeEmpty();

        var sharedKernelAssembly = assemblyElements.FirstOrDefault(a => a.Attribute("fullname")?.Value == "EricksonLopez.SharedKernel");
        sharedKernelAssembly.Should().NotBeNull(because: "Descriptor must declare the EricksonLopez.SharedKernel assembly.");

        var typeElements = sharedKernelAssembly!.Descendants("type").ToList();
        typeElements.Should().NotBeEmpty();

        foreach (var typeElement in typeElements)
        {
            var typeFullName = typeElement.Attribute("fullname")?.Value;
            typeFullName.Should().NotBeNullOrWhiteSpace();

            var type = TargetAssembly.GetType(typeFullName!, throwOnError: false);
            type.Should().NotBeNull(because: $"Type '{typeFullName}' registered in trimmer descriptor must exist in assembly.");

            type!.IsPublic.Should().BeTrue(because: $"Type '{typeFullName}' registered in trimmer descriptor must be public.");

            var preserveAttr = typeElement.Attribute("preserve")?.Value;
            if (preserveAttr == "all")
            {
                if (type.IsInterface)
                {
                    // For interfaces, ensure interface type definition is preserved
                    type.IsInterface.Should().BeTrue();
                }
                else
                {
                    // Verify members (methods, properties, constructors) are preserved
                    var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
                    var members = type.GetMembers(bindingFlags);
                    members.Should().NotBeEmpty(because: $"Type '{typeFullName}' with preserve='all' must have preserved members.");
                }
            }
        }
    }

    [Fact]
    public void TrimmerDescriptor_MustCoverAllPublicExportedTypes()
    {
        using var stream = TargetAssembly.GetManifestResourceStream("ILLink.Descriptors.xml");
        stream.Should().NotBeNull();

        var doc = XDocument.Load(stream!);
        var registeredTypeNames = doc.Descendants("type")
            .Select(t => t.Attribute("fullname")?.Value)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToHashSet();

        var exportedTypes = TargetAssembly.GetExportedTypes();
        exportedTypes.Should().NotBeEmpty();

        foreach (var exportedType in exportedTypes)
        {
            var typeFullName = exportedType.FullName;
            typeFullName.Should().NotBeNull();

            // Normalize generic type names (e.g. Entity`1)
            registeredTypeNames.Should().Contain(typeFullName,
                because: $"Exported public type '{typeFullName}' must be explicitly registered in ILLink.Descriptors.xml to ensure trimming safety.");
        }
    }
}
