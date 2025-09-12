using SharpLoader.Core.Minecraft.Mapping.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpLoader.Core.Minecraft.Mapping.Utilities;

public class MappingSearcher
{
    public MappingSet Set { get; }
    
    private readonly Dictionary<string, ClassMapping> _classByObfuscatedName;
    private readonly Dictionary<string, ClassMapping> _classByMappedName;
    private readonly Dictionary<string, InnerClassMapping> _innerClassByObfuscatedName;
    private readonly Dictionary<string, InnerClassMapping> _innerClassByMappedName;
    private readonly Dictionary<string, List<MethodMapping>> _methodsByObfuscatedName;
    private readonly Dictionary<string, List<MethodMapping>> _methodsByMappedName;
    private readonly Dictionary<string, List<FieldMapping>> _fieldsByObfuscatedName;
    private readonly Dictionary<string, List<FieldMapping>> _fieldsByMappedName;

    public MappingSearcher(MappingSet set)
    {
        Set = set;
        
        _classByObfuscatedName = new Dictionary<string, ClassMapping>();
        _classByMappedName = new Dictionary<string, ClassMapping>();
        _innerClassByObfuscatedName = new Dictionary<string, InnerClassMapping>();
        _innerClassByMappedName = new Dictionary<string, InnerClassMapping>();
        _methodsByObfuscatedName = new Dictionary<string, List<MethodMapping>>();
        _methodsByMappedName = new Dictionary<string, List<MethodMapping>>();
        _fieldsByObfuscatedName = new Dictionary<string, List<FieldMapping>>();
        _fieldsByMappedName = new Dictionary<string, List<FieldMapping>>();
        
        BuildIndex();
    }

    private void BuildIndex()
    {
        // Index classes and their members
        foreach (var classMapping in Set.Classes.Values)
        {
            IndexClass(classMapping);
        }
        
        // Index inner classes from the separate dictionary
        foreach (var innerClassMapping in Set.InnerClasses.Values)
        {
            IndexInnerClass(innerClassMapping);
        }
    }

    private void IndexClass(ClassMapping classMapping)
    {
        var obfuscatedName = NormalizeClassName(classMapping.ObfuscatedName);
        var mappedName = NormalizeClassName(classMapping.MappedName);
        
        _classByObfuscatedName[obfuscatedName] = classMapping;
        _classByMappedName[mappedName] = classMapping;
        
        // Index fields and methods of the class
        IndexFields(classMapping.Fields);
        IndexMethods(classMapping.Methods);
        
        // Index inner classes of the class
        foreach (var innerClass in classMapping.InnerClasses)
        {
            IndexInnerClass(innerClass);
        }
    }

    private void IndexInnerClass(InnerClassMapping innerClassMapping)
    {
        var obfuscatedName = NormalizeClassName(innerClassMapping.ObfuscatedName);
        var mappedName = NormalizeClassName(innerClassMapping.MappedName);
        
        _innerClassByObfuscatedName[obfuscatedName] = innerClassMapping;
        _innerClassByMappedName[mappedName] = innerClassMapping;
        
        // Index fields and methods of the inner class
        IndexFields(innerClassMapping.Fields);
        IndexMethods(innerClassMapping.Methods);
    }

    private void IndexFields(List<FieldMapping> fields)
    {
        foreach (var field in fields)
        {
            if (!_fieldsByObfuscatedName.TryGetValue(field.ObfuscatedName, out var obfList))
            {
                obfList = new List<FieldMapping>();
                _fieldsByObfuscatedName[field.ObfuscatedName] = obfList;
            }
            obfList.Add(field);
            
            if (!_fieldsByMappedName.TryGetValue(field.MappedName, out var mappedList))
            {
                mappedList = new List<FieldMapping>();
                _fieldsByMappedName[field.MappedName] = mappedList;
            }
            mappedList.Add(field);
        }
    }

    private void IndexMethods(List<MethodMapping> methods)
    {
        foreach (var method in methods)
        {
            if (!_methodsByObfuscatedName.TryGetValue(method.ObfuscatedName, out var obfList))
            {
                obfList = new List<MethodMapping>();
                _methodsByObfuscatedName[method.ObfuscatedName] = obfList;
            }
            obfList.Add(method);
            
            if (!_methodsByMappedName.TryGetValue(method.MappedName, out var mappedList))
            {
                mappedList = new List<MethodMapping>();
                _methodsByMappedName[method.MappedName] = mappedList;
            }
            mappedList.Add(method);
        }
    }

    private string NormalizeClassName(string className)
    {
        return className.Replace('.', '/');
    }

    public bool CompareClassName(string className1, string className2)
    {
        if (className1.Equals(className2))
        {
            return true;
        }

        if (className1.Replace('/', '.').Equals(className2.Replace('/', '.')))
        {
            return true;
        }
        
        if (className1.Replace('.', '/').Equals(className2.Replace('.', '/')))
        {
            return true;
        }
        
        return false;
    }
    
    public bool MatchClass(string javaClassName, string mappedClassName)
    {
        var normalizedJavaName = NormalizeClassName(javaClassName);
        
        // Check in classes
        if (_classByObfuscatedName.TryGetValue(normalizedJavaName, out var classMapping))
        {
            return CompareClassName(classMapping.MappedName, mappedClassName);
        }
        
        // Check in inner classes
        if (_innerClassByObfuscatedName.TryGetValue(normalizedJavaName, out var innerClassMapping))
        {
            return CompareClassName(innerClassMapping.MappedName, mappedClassName);
        }
        
        return false;
    }
    
    public string? SearchField(string javaName)
    {
        if (_fieldsByObfuscatedName.TryGetValue(javaName, out var fields) && fields.Count > 0)
        {
            return fields[0].MappedName;
        }
        
        return null;
    }
    
    public FieldMapping? SearchFieldMapping(string javaName)
    {
        if (_fieldsByObfuscatedName.TryGetValue(javaName, out var fields) && fields.Count > 0)
        {
            return fields[0];
        }
        
        return null;
    }
    
    public string? SearchMethod(string javaName)
    {
        if (_methodsByObfuscatedName.TryGetValue(javaName, out var methods) && methods.Count > 0)
        {
            return methods[0].MappedName;
        }
        
        return null;
    }
    
    public MethodMapping? SearchMethodMapping(string javaName)
    {
        if (_methodsByObfuscatedName.TryGetValue(javaName, out var methods) && methods.Count > 0)
        {
            return methods[0];
        }
        
        return null;
    }

    public class SearchResults
    {
        public List<ClassMapping> Classes { get; set; } = new List<ClassMapping>();
        public List<InnerClassMapping> InnerClasses { get; set; } = new List<InnerClassMapping>();
        public List<MethodMapping> Methods { get; set; } = new List<MethodMapping>();
        public List<FieldMapping> Fields { get; set; } = new List<FieldMapping>();
    }
}
