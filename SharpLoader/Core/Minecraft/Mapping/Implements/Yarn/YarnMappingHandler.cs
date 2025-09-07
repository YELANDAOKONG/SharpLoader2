namespace SharpLoader.Core.Minecraft.Mapping.Implements.Yarn;

using SharpLoader.Core.Minecraft.Mapping.Interfaces;
using SharpLoader.Core.Minecraft.Mapping.Models;
using System.IO.Compression;
using System.Text;

public class YarnMappingHandler : IMappingHandler
{
    private readonly MappingSet _mappingSet = new();

    public YarnMappingHandler(string zipFilePath)
    {
        if (string.IsNullOrEmpty(zipFilePath))
            throw new ArgumentException("ZIP file path cannot be null or empty.", nameof(zipFilePath));

        if (!File.Exists(zipFilePath))
            throw new FileNotFoundException($"ZIP file not found: {zipFilePath}", zipFilePath);

        LoadMappingsFromZip(zipFilePath);
    }

    public ClassMapping? GetClassMapping(string obfuscatedName)
    {
        _mappingSet.Classes.TryGetValue(obfuscatedName, out var classMapping);
        return classMapping;
    }

    public InnerClassMapping? GetInnerClassMapping(string obfuscatedName)
    {
        _mappingSet.InnerClasses.TryGetValue(obfuscatedName, out var innerClassMapping);
        return innerClassMapping;
    }

    public IReadOnlyDictionary<string, ClassMapping> GetAllClassMappings()
    {
        return _mappingSet.Classes;
    }

    public IReadOnlyDictionary<string, InnerClassMapping> GetAllInnerClassMappings()
    {
        return _mappingSet.InnerClasses;
    }

    private void LoadMappingsFromZip(string zipFilePath)
    {
        using var zipArchive = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in zipArchive.Entries)
        {
            if (entry.FullName.EndsWith(".mapping", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = entry.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                ParseMappingContent(reader);
            }
        }
    }

    private void ParseMappingContent(StreamReader reader)
    {
        string? line;
        ClassMapping? currentClass = null;
        MethodMapping? currentMethod = null;
        InnerClassMapping? currentInnerClass = null;

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith("CLASS"))
            {
                currentMethod = null;
                currentInnerClass = null;
                ParseClassLine(line, ref currentClass);
            }
            else if (line.StartsWith("FIELD"))
            {
                ParseFieldLine(line, currentClass, currentInnerClass);
            }
            else if (line.StartsWith("METHOD"))
            {
                currentMethod = ParseMethodLine(line, currentClass, currentInnerClass);
            }
            else if (line.StartsWith("ARG"))
            {
                ParseArgumentLine(line, currentMethod);
            }
            else if (line.StartsWith("COMMENT"))
            {
                ParseCommentLine(line, currentClass, currentMethod, currentInnerClass);
            }
            else if (char.IsDigit(line[0]) && currentClass != null)
            {
                currentInnerClass = ParseInnerClassLine(line, currentClass);
            }
        }
    }

    private void ParseClassLine(string line, ref ClassMapping? currentClass)
    {
        var parts = line.Split(' ', 3);
        if (parts.Length < 3) return;

        currentClass = new ClassMapping
        {
            ObfuscatedName = parts[1],
            MappedName = parts[2]
        };

        _mappingSet.Classes[currentClass.ObfuscatedName] = currentClass;
    }

    private InnerClassMapping? ParseInnerClassLine(string line, ClassMapping parentClass)
    {
        var parts = line.Split(' ', 3);
        if (parts.Length < 3) return null;

        var innerClass = new InnerClassMapping
        {
            ObfuscatedName = $"{parentClass.ObfuscatedName}${parts[1]}",
            MappedName = parts[2]
        };

        parentClass.InnerClasses.Add(innerClass);
        _mappingSet.InnerClasses[innerClass.ObfuscatedName] = innerClass;
        return innerClass;
    }

    private void ParseFieldLine(string line, ClassMapping? currentClass, InnerClassMapping? currentInnerClass)
    {
        var parts = line.Split(' ', 4);
        if (parts.Length < 4) return;

        var field = new FieldMapping
        {
            ObfuscatedName = parts[1],
            MappedName = parts[2],
            Descriptor = parts[3]
        };

        if (currentInnerClass != null)
        {
            currentInnerClass.Fields.Add(field);
        }
        else if (currentClass != null)
        {
            currentClass.Fields.Add(field);
        }
    }

    private MethodMapping? ParseMethodLine(string line, ClassMapping? currentClass, InnerClassMapping? currentInnerClass)
    {
        var parts = line.Split(' ', 4);
        if (parts.Length < 4) return null;

        var method = new MethodMapping
        {
            ObfuscatedName = parts[1],
            MappedName = parts[2],
            Descriptor = parts[3]
        };

        if (currentInnerClass != null)
        {
            currentInnerClass.Methods.Add(method);
        }
        else if (currentClass != null)
        {
            currentClass.Methods.Add(method);
        }

        return method;
    }

    private void ParseArgumentLine(string line, MethodMapping? currentMethod)
    {
        if (currentMethod == null) return;

        var parts = line.Split(' ', 3);
        if (parts.Length < 3) return;

        if (int.TryParse(parts[1], out var index))
        {
            currentMethod.Parameters.Add(new ParameterMapping
            {
                Index = index,
                Name = parts[2]
            });
        }
    }

    private void ParseCommentLine(string line, ClassMapping? currentClass, MethodMapping? currentMethod, InnerClassMapping? currentInnerClass)
    {
        var comment = line.Substring(7).Trim(); // Remove "COMMENT" prefix

        if (currentMethod != null)
        {
            currentMethod.Comment = CombineComment(currentMethod.Comment, comment);
        }
        else if (currentInnerClass != null)
        {
            currentInnerClass.Comment = CombineComment(currentInnerClass.Comment, comment);
        }
        else if (currentClass != null)
        {
            currentClass.Comment = CombineComment(currentClass.Comment, comment);
        }
    }

    private static string? CombineComment(string? existingComment, string newComment)
    {
        return string.IsNullOrEmpty(existingComment) 
            ? newComment 
            : $"{existingComment}\n{newComment}";
    }
}
