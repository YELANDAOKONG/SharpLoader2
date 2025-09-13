using System;

namespace SharpLoader.Modding.Models;

public class ModuleVersion : IComparable<ModuleVersion>, IEquatable<ModuleVersion>, IComparable
{
    public long Major { get; set; }
    public long Minor { get; set; }
    public long Patch { get; set; }
    public long? Build { get; set; }
    public string? Tag { get; set; }

    public ModuleVersion() { }

    public ModuleVersion(long major, long minor, long patch, long? build = null, string? tag = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Build = build;
        Tag = tag;
    }

    #region Functions

    /// <summary>
    /// Compares only the Major, Minor, and Patch components of two versions.
    /// Build and Tag are ignored in this comparison.
    /// </summary>
    public int CompareCoreVersionTo(ModuleVersion? other)
    {
        if (other is null)
            return 1;
        
        // Compare Major version
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0)
            return majorComparison;
        
        // Compare Minor version
        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0)
            return minorComparison;
        
        // Compare Patch version
        return Patch.CompareTo(other.Patch);
    }
    
    /// <summary>
    /// Determines if the core version (Major, Minor, Patch) is equal to another version
    /// </summary>
    public bool CoreVersionEquals(ModuleVersion? other)
    {
        if (other is null)
            return false;
        
        return Major == other.Major &&
               Minor == other.Minor &&
               Patch == other.Patch;
    }

    #endregion

    #region IComparable<ModuleVersion> Implementation

    public int CompareTo(ModuleVersion? other)
    {
        if (other is null)
            return 1;

        // Compare Major version
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0)
            return majorComparison;

        // Compare Minor version
        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0)
            return minorComparison;

        // Compare Patch version
        var patchComparison = Patch.CompareTo(other.Patch);
        if (patchComparison != 0)
            return patchComparison;

        // Compare Build version (null is considered as 0)
        var thisBuild = Build ?? 0;
        var otherBuild = other.Build ?? 0;
        return thisBuild.CompareTo(otherBuild);

        // Note: Tag is NOT included in comparison as per requirement
    }

    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        if (obj is not ModuleVersion other)
            throw new ArgumentException("Object must be of type ModuleVersion", nameof(obj));

        return CompareTo(other);
    }

    #endregion

    #region IEquatable<ModuleVersion> Implementation

    public bool Equals(ModuleVersion? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Major == other.Major &&
               Minor == other.Minor &&
               Patch == other.Patch &&
               (Build ?? 0) == (other.Build ?? 0) &&
               string.Equals(Tag, other.Tag, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Object Overrides

    public override bool Equals(object? obj) => obj is ModuleVersion other && Equals(other);

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Major, 
            Minor, 
            Patch, 
            Build ?? 0, 
            Tag?.ToLowerInvariant());
    }

    public override string ToString()
    {
        var baseVersion = Build.HasValue
            ? $"{Major}.{Minor}.{Patch}.{Build}"
            : $"{Major}.{Minor}.{Patch}";

        return string.IsNullOrEmpty(Tag)
            ? baseVersion
            : $"{baseVersion}-{Tag}";
    }

    #endregion

    #region Operators

    public static bool operator ==(ModuleVersion? left, ModuleVersion? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ModuleVersion? left, ModuleVersion? right) => !(left == right);

    public static bool operator <(ModuleVersion? left, ModuleVersion? right)
    {
        if (left is null)
            return right is not null;

        return left.CompareTo(right) < 0;
    }

    public static bool operator >(ModuleVersion? left, ModuleVersion? right)
    {
        if (left is null)
            return false;

        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(ModuleVersion? left, ModuleVersion? right)
    {
        if (left is null)
            return true;

        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(ModuleVersion? left, ModuleVersion? right)
    {
        if (left is null)
            return right is null;

        return left.CompareTo(right) >= 0;
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Parses a version string in the format "major.minor.patch", "major.minor.patch.build", 
    /// "major.minor.patch-tag", or "major.minor.patch.build-tag"
    /// </summary>
    /// <param name="version">The version string to parse</param>
    /// <returns>A ModuleVersion instance</returns>
    /// <exception cref="ArgumentException">Thrown when the version string is invalid</exception>
    public static ModuleVersion Parse(string version)
    {
        if (TryParse(version, out var result))
            return result;

        throw new ArgumentException($"Invalid version format: {version}", nameof(version));
    }

    /// <summary>
    /// Tries to parse a version string in the format "major.minor.patch", "major.minor.patch.build",
    /// "major.minor.patch-tag", or "major.minor.patch.build-tag"
    /// </summary>
    /// <param name="version">The version string to parse</param>
    /// <param name="result">The parsed ModuleVersion instance if successful</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParse(string? version, out ModuleVersion result)
    {
        result = new ModuleVersion();

        if (string.IsNullOrWhiteSpace(version))
            return false;

        // Split by '-' to separate version from tag
        var versionParts = version.Split('-', 2);
        var versionString = versionParts[0];
        var tag = versionParts.Length > 1 ? versionParts[1] : null;

        // Parse the version part
        var parts = versionString.Split('.');
        if (parts.Length < 3 || parts.Length > 4)
            return false;

        // Parse Major
        if (!long.TryParse(parts[0], out var major) || major < 0)
            return false;

        // Parse Minor
        if (!long.TryParse(parts[1], out var minor) || minor < 0)
            return false;

        // Parse Patch
        if (!long.TryParse(parts[2], out var patch) || patch < 0)
            return false;

        // Parse Build (optional)
        long? build = null;
        if (parts.Length == 4)
        {
            if (!long.TryParse(parts[3], out var buildValue) || buildValue < 0)
                return false;
            build = buildValue;
        }

        result = new ModuleVersion(major, minor, patch, build, tag);
        return true;
    }

    /// <summary>
    /// Creates a new ModuleVersion with the specified major, minor, and patch versions
    /// </summary>
    public static ModuleVersion Create(long major, long minor, long patch, string? tag = null) 
        => new(major, minor, patch, null, tag);

    /// <summary>
    /// Creates a new ModuleVersion with the specified major, minor, patch, and build versions
    /// </summary>
    public static ModuleVersion Create(long major, long minor, long patch, long build, string? tag = null) 
        => new(major, minor, patch, build, tag);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Determines if this version is compatible with the specified version range
    /// Note: Tag is not considered in compatibility checking
    /// </summary>
    /// <param name="minVersion">Minimum compatible version</param>
    /// <param name="maxVersion">Maximum compatible version (null means no upper limit)</param>
    /// <returns>True if compatible, false otherwise</returns>
    public bool IsCompatibleWith(ModuleVersion minVersion, ModuleVersion? maxVersion = null)
    {
        if (minVersion is null)
            throw new ArgumentNullException(nameof(minVersion));

        if (this < minVersion)
            return false;

        if (maxVersion is not null && this > maxVersion)
            return false;

        return true;
    }

    /// <summary>
    /// Determines if this version is a pre-release version (has a build number)
    /// </summary>
    public bool IsPreRelease => Build.HasValue;

    /// <summary>
    /// Determines if this version has a tag
    /// </summary>
    public bool HasTag => !string.IsNullOrEmpty(Tag);

    /// <summary>
    /// Gets the version without the build number and tag
    /// </summary>
    public ModuleVersion GetReleaseVersion() => new(Major, Minor, Patch);

    /// <summary>
    /// Gets the version without the tag
    /// </summary>
    public ModuleVersion GetVersionWithoutTag() => new(Major, Minor, Patch, Build);

    /// <summary>
    /// Creates a new version with the specified tag
    /// </summary>
    public ModuleVersion WithTag(string? tag) => new(Major, Minor, Patch, Build, tag);

    /// <summary>
    /// Creates a new version without any tag
    /// </summary>
    public ModuleVersion WithoutTag() => new(Major, Minor, Patch, Build);

    /// <summary>
    /// Creates a new version with incremented major number (resets minor, patch, build, keeps tag)
    /// </summary>
    public ModuleVersion IncrementMajor() => new(Major + 1, 0, 0, null, Tag);

    /// <summary>
    /// Creates a new version with incremented minor number (resets patch, build, keeps tag)
    /// </summary>
    public ModuleVersion IncrementMinor() => new(Major, Minor + 1, 0, null, Tag);

    /// <summary>
    /// Creates a new version with incremented patch number (resets build, keeps tag)
    /// </summary>
    public ModuleVersion IncrementPatch() => new(Major, Minor, Patch + 1, null, Tag);

    /// <summary>
    /// Creates a new version with incremented build number (keeps tag)
    /// </summary>
    public ModuleVersion IncrementBuild() => new(Major, Minor, Patch, (Build ?? 0) + 1, Tag);

    #endregion
}
