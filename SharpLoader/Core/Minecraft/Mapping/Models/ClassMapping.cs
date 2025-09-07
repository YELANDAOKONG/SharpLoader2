namespace SharpLoader.Core.Minecraft.Mapping.Models;

[Serializable]
public class ClassMapping
{
    public required string ObfuscatedName { get; set; }
    public required string MappedName { get; set; }
    public string? Comment { get; set; }
    public List<FieldMapping> Fields { get; set; } = new();
    public List<MethodMapping> Methods { get; set; } = new();
    public List<InnerClassMapping> InnerClasses { get; set; } = new();
}