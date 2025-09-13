namespace SharpLoader.Modding.Models;

public class ModuleDependency
{
    public string ModuleId { get; set; } = string.Empty;
    public ModuleVersionRange? VersionRange { get; set; } = null;
}