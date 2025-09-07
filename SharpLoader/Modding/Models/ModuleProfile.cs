namespace SharpLoader.Modding.Models;

[Serializable]
public class ModuleProfile
{
    // Core
    public required string Id { get; set; } // Unique Id
    public required string Namespace { get; set; } // Namespace
    public ModuleVersion Version { get; set; } = new ModuleVersion();
    
    // Code
    public string EntryPoint { get; set; } = "default.dll";
    public string? MainClass { get; set; }
    
    // Information
    public string? Icon { get; set; }
    public string Title { get; set; } = "Unknown";
    public string? Description { get; set; }
    public List<string> Authors { get; set; } = new List<string>();
    public List<string> Urls { get; set; } = new List<string>();
    
    // TODO: Dependency, GameVersion...
}