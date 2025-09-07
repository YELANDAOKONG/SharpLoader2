namespace SharpLoader.Core.Minecraft.Mapping.Models;

[Serializable]
public class MappingSet
{
    public Dictionary<string, ClassMapping> Classes { get; set; } = new();
    public Dictionary<string, InnerClassMapping> InnerClasses { get; set; } = new();
}