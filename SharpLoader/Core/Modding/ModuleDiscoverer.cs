using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using SharpLoader.Modding.Models;
using SharpLoader.Utilities;

namespace SharpLoader.Core.Modding;

public class ModuleDiscoverer
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
    };
    
    public static List<(string, ModuleProfile)> SearchModules(string modulesDirectory, LoggerService? logger = null)
    {
        var result = new List<(string, ModuleProfile)>();
        
        if (!Directory.Exists(modulesDirectory))
        {
            logger?.Info($"Modules directory does not exist, creating: {modulesDirectory}");
            Directory.CreateDirectory(modulesDirectory);
            return result;
        }

        var files = Directory.GetFiles(modulesDirectory);
        logger?.Info($"Found {files.Length} files in modules directory");
        
        foreach (var file in files)
        {
            try
            {
                if (!IsZipFile(file))
                {
                    logger?.Debug($"Skipping non-zip file: {file}");
                    continue;
                }
                
                logger?.Debug($"Processing module file: {file}");
                using var zipArchive = ZipFile.OpenRead(file);
                
                var manifestEntry = zipArchive.GetEntry(Statics.ModuleManifestFile);
                if (manifestEntry == null)
                {
                    logger?.Warn($"Manifest file '{Statics.ModuleManifestFile}' not found in module: {file}");
                    continue;
                }
                
                using var stream = manifestEntry.Open();
                using var reader = new StreamReader(stream);
                var jsonContent = reader.ReadToEnd();
                
                logger?.Debug($"Parsing manifest from: {file}");
                var profile = JsonSerializer.Deserialize<ModuleProfile>(jsonContent, JsonOptions);
                if (profile == null)
                {
                    logger?.Warn($"Failed to parse manifest in module: {file}");
                    continue;
                }
                
                if (string.IsNullOrWhiteSpace(profile.Id) || 
                    string.IsNullOrWhiteSpace(profile.Namespace))
                {
                    logger?.Warn($"Invalid profile (missing Id or Namespace) in module: {file}");
                    continue;
                }
                
                result.Add((file, profile));
                logger?.Info($"Found module: {profile.Id} v{profile.Version} at {file}");
            }
            catch (Exception ex) when (
                ex is InvalidDataException || 
                ex is IOException || 
                ex is JsonException || 
                ex is NotSupportedException)
            {
                logger?.Warn($"Error processing file {file}: {ex.Message}");
                continue;
            }
            catch (Exception ex)
            {
                logger?.Warn($"Unexpected error processing file {file}: {ex.Message}");
                if (ex.StackTrace != null) logger?.Trace(ex.StackTrace);
                continue;
            }
        }
        
        return result;
    }

    private static bool IsZipFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension == ".zip" || extension == ".mod" || extension == ".sharp";
    }
}
