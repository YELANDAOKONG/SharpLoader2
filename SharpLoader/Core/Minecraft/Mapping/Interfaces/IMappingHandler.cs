namespace SharpLoader.Core.Minecraft.Mapping.Interfaces;

using SharpLoader.Core.Minecraft.Mapping.Models;

public interface IMappingHandler
{
    ClassMapping? GetClassMapping(string obfuscatedName);
    InnerClassMapping? GetInnerClassMapping(string obfuscatedName);
    IReadOnlyDictionary<string, ClassMapping> GetAllClassMappings();
    IReadOnlyDictionary<string, InnerClassMapping> GetAllInnerClassMappings();
}