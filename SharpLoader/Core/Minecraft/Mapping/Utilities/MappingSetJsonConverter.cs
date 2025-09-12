using System.Text.Json;
using System.Text.Json.Serialization;
using SharpLoader.Core.Minecraft.Mapping.Models;

namespace SharpLoader.Core.Minecraft.Mapping.Utilities;

public static class MappingSetJsonConverter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
    
    public static string Serialize(MappingSet mappingSet)
    {
        return JsonSerializer.Serialize(mappingSet, Options);
    }
    
    public static MappingSet Deserialize(string json)
    {
        return JsonSerializer.Deserialize<MappingSet>(json, Options) ?? 
               throw new JsonException("Failed to deserialize MappingSet");
    }
    
    public static void SerializeToFile(MappingSet mappingSet, string filePath)
    {
        var json = Serialize(mappingSet);
        File.WriteAllText(filePath, json);
    }
    
    public static MappingSet DeserializeFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return Deserialize(json);
    }
}