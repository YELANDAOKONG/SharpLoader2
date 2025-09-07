namespace SharpLoader.Core.Minecraft.Mapping.Models;

[Serializable]
public class FieldMapping
{
    public required string ObfuscatedName { get; set; }
    public required string MappedName { get; set; }
    public required string Descriptor { get; set; }
    public string? Comment { get; set; }
}