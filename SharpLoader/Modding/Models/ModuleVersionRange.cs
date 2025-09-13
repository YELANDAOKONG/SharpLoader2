using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpLoader.Modding.Models;

public class ModuleVersionRange
{
    public ModuleVersion? MinimumVersion { get; set; } = null;
    public ModuleVersion? MaximumVersion { get; set; } = null;
    public ModuleVersion? CompatibleVersion { get; set; } = null;
    
    public ModuleVersionRange() { }
    
    public ModuleVersionRange(ModuleVersion? minVersion, ModuleVersion? maxVersion)
    {
        MinimumVersion = minVersion;
        MaximumVersion = maxVersion;
    }
    
    public ModuleVersionRange(ModuleVersion compatibleVersion)
    {
        CompatibleVersion = compatibleVersion;
    }
    
    /// <summary>
    /// Checks if a version satisfies this version range
    /// </summary>
    public bool IsSatisfiedBy(ModuleVersion version, ModuleProfile? profile = null)
    {
        if (version is null)
            throw new ArgumentNullException(nameof(version));
        
        // Handle compatible version range (~ syntax)
        if (CompatibleVersion != null)
        {
            return IsCompatibleVersionSatisfied(version, profile);
        }
        
        // Handle normal version ranges
        bool satisfiesMin = MinimumVersion == null || version >= MinimumVersion;
        bool satisfiesMax = MaximumVersion == null || version <= MaximumVersion;
        
        return satisfiesMin && satisfiesMax;
    }
    
    /// <summary>
    /// Checks if a version satisfies the compatible version range
    /// </summary>
    private bool IsCompatibleVersionSatisfied(ModuleVersion version, ModuleProfile? profile)
    {
        if (CompatibleVersion is null)
            return false;
            
        // If a profile is provided, use its compatible versions list
        if (profile != null && profile.CompatibleVersion.Any())
        {
            return profile.CompatibleVersion.Any(cv => cv.CoreVersionEquals(version));
        }
        
        // Fallback: Check if the major and minor versions match (standard semantic versioning compatibility)
        // Note: This is a fallback and should ideally use the profile's compatible versions
        return version.Major == CompatibleVersion.Major && 
               version.Minor == CompatibleVersion.Minor &&
               version >= CompatibleVersion;
    }
    
    /// <summary>
    /// Creates a version range that matches any version
    /// </summary>
    public static ModuleVersionRange Any() => new ModuleVersionRange(null, null);
    
    /// <summary>
    /// Creates a version range that matches versions less than or equal to the specified version
    /// </summary>
    public static ModuleVersionRange LessThanOrEqual(ModuleVersion version) => new ModuleVersionRange(null, version);
    
    /// <summary>
    /// Creates a version range that matches versions greater than or equal to the specified version
    /// </summary>
    public static ModuleVersionRange GreaterThanOrEqual(ModuleVersion version) => new ModuleVersionRange(version, null);
    
    /// <summary>
    /// Creates a version range that matches versions between min and max (inclusive)
    /// </summary>
    public static ModuleVersionRange Between(ModuleVersion min, ModuleVersion max) => new ModuleVersionRange(min, max);
    
    /// <summary>
    /// Creates a compatible version range (~ syntax)
    /// </summary>
    public static ModuleVersionRange CompatibleWith(ModuleVersion version) => new ModuleVersionRange(version);
    
    /// <summary>
    /// Parses a version range string into a ModuleVersionRange object
    /// </summary>
    public static ModuleVersionRange Parse(string range)
    {
        if (TryParse(range, out var result))
            return result;
            
        throw new FormatException($"Invalid version range format: {range}");
    }
    
    /// <summary>
    /// Attempts to parse a version range string into a ModuleVersionRange object
    /// </summary>
    public static bool TryParse(string range, out ModuleVersionRange result)
    {
        result = Any();
        
        if (string.IsNullOrWhiteSpace(range))
            return false;
            
        range = range.Trim();
        
        // Handle compatible version syntax (~X.X.X)
        if (range.StartsWith("~"))
        {
            var versionStr = range.Substring(1).Trim();
            if (ModuleVersion.TryParse(versionStr, out var version))
            {
                result = CompatibleWith(version);
                return true;
            }
            return false;
        }
        
        // Handle range syntax (X.X.X - Y.Y.Y)
        if (range.Contains("-"))
        {
            var parts = range.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return false;
                
            var minStr = parts[0].Trim();
            var maxStr = parts[1].Trim();
            
            // Handle any version (*.*.* - *.*.*)
            if (minStr == "*.*.*" && maxStr == "*.*.*")
            {
                result = Any();
                return true;
            }
            
            // Handle less than or equal (*.*.* - X.X.X)
            if (minStr == "*.*.*" && ModuleVersion.TryParse(maxStr, out var maxVersion))
            {
                result = LessThanOrEqual(maxVersion);
                return true;
            }
            
            // Handle greater than or equal (X.X.X - *.*.*)
            if (maxStr == "*.*.*" && ModuleVersion.TryParse(minStr, out var minVersion))
            {
                result = GreaterThanOrEqual(minVersion);
                return true;
            }
            
            // Handle explicit range (X.X.X - Y.Y.Y)
            if (ModuleVersion.TryParse(minStr, out minVersion) && ModuleVersion.TryParse(maxStr, out maxVersion))
            {
                result = Between(minVersion, maxVersion);
                return true;
            }
            
            return false;
        }
        
        // Handle single version (treated as exact match)
        if (ModuleVersion.TryParse(range, out var exactVersion))
        {
            result = Between(exactVersion, exactVersion);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Returns a string representation of the version range
    /// </summary>
    public override string ToString()
    {
        if (CompatibleVersion != null)
            return $"~{CompatibleVersion}";
            
        if (MinimumVersion == null && MaximumVersion == null)
            return "*.*.* - *.*.*";
            
        if (MinimumVersion == null)
            return $"*.*.* - {MaximumVersion}";
            
        if (MaximumVersion == null)
            return $"{MinimumVersion} - *.*.*";
            
        if (MinimumVersion == MaximumVersion)
            return MinimumVersion.ToString();
            
        return $"{MinimumVersion} - {MaximumVersion}";
    }
}
