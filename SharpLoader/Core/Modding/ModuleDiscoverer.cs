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
            Directory.CreateDirectory(modulesDirectory);
            return result;
        }

        var files = Directory.GetFiles(modulesDirectory);
        
        foreach (var file in files)
        {
            try
            {
                if (!IsZipFile(file)) continue;
                using var zipArchive = ZipFile.OpenRead(file);
                
                var manifestEntry = zipArchive.GetEntry(Statics.ModuleManifestFile);
                if (manifestEntry == null) continue;
                
                using var stream = manifestEntry.Open();
                using var reader = new StreamReader(stream);
                var jsonContent = reader.ReadToEnd();
                
                var profile = JsonSerializer.Deserialize<ModuleProfile>(jsonContent, JsonOptions);
                if (profile == null)
                    continue;
                
                if (string.IsNullOrWhiteSpace(profile.Id) || 
                    string.IsNullOrWhiteSpace(profile.Namespace))
                    continue;
                
                result.Add((file, profile));
            }
            catch (Exception ex) when (
                ex is InvalidDataException || 
                ex is IOException || 
                ex is JsonException || 
                ex is NotSupportedException)
            {
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
